using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Microsoft.AspNetCore.SignalR.Client;

[Serializable] public class CommandReceivedEvent : UnityEvent<string> { }
[Serializable] public class KeyStateChangedEvent : UnityEvent<string, bool> { }
[Serializable] public class JoystickChangedEvent : UnityEvent<float, float, float, float> { }
[Serializable] public class CustomActionEvent : UnityEvent<string, string, string> { }

/// <summary>
/// Manages the Cephable virtual controller device, SignalR connection, and macro execution.
///
/// Attach this component to a GameObject in your scene. After the user authenticates via
/// OAuth2Manager, this component registers a virtual device with Cephable and begins
/// receiving adaptive control commands in real time.
///
/// There are three ways to respond to commands in your game:
///   1. UnityEvents  – wire up handlers directly in the Inspector (no code required)
///   2. C# events    – subscribe to MacroCommandReceived or CustomActionReceived in script
///   3. Simulated input polling – call GetKey() / GetAxis() just like Unity's Input API
/// </summary>
public class VirtualController : MonoBehaviour
{
    [Header("Device Configuration")]
    public string DeviceTypeId;
    public string DefaultDeviceName = "Game Controller";
    public string CephableApiBaseUrl = "https://services.cephable.com";

    [Header("Unity Events")]
    [Tooltip("Fires when any Cephable command is received. Receives the command name string.")]
    public CommandReceivedEvent OnCommandReceived;

    [Tooltip("Fires when a key state changes due to a macro event. Receives key name and pressed state.")]
    public KeyStateChangedEvent OnKeyStateChanged;

    [Tooltip("Fires when joystick axes change. Receives leftX, leftY, rightX, rightY (range -1 to 1).")]
    public JoystickChangedEvent OnJoystickChanged;

    [Tooltip("Fires when a custom device action macro event runs. Receives deviceTypeId, actionId, and additionalContent.")]
    public CustomActionEvent OnCustomActionTriggered;

    /// <summary>Fires with the full command name and MacroModel for code-based handling.</summary>
    public event Action<string, MacroModel> MacroCommandReceived;

    /// <summary>Fires when a DeviceTypeCustomAction macro event is executed.</summary>
    public event Action<string, string, string> CustomActionReceived;

    // Simulated input state readable via GetKey() / GetAxis()
    private readonly Dictionary<string, bool> keyStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
    private float joystickLeftX, joystickLeftY, joystickRightX, joystickRightY;

    private string userDeviceId;
    private string userDeviceToken;
    private DeviceProfileConfiguration currentProfile;
    private HubConnection hubConnection;

    // Maps Cephable key name strings to Unity KeyCodes for GetKey(KeyCode) lookups.
    private static readonly Dictionary<string, KeyCode> KeyCodeMap = BuildKeyCodeMap();

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    public async Task Start()
    {
        userDeviceId = PlayerPrefs.GetString("userDeviceId");
        userDeviceToken = PlayerPrefs.GetString("userDeviceToken");

        if (!string.IsNullOrEmpty(userDeviceId) && !string.IsNullOrEmpty(userDeviceToken))
        {
            output($"Resuming device {userDeviceId}");
            await ConnectToHub();
        }
        else
        {
            StartCoroutine(CreateVirtualController());
        }
    }

    void Update() { }

    // ─── Simulated Input API ──────────────────────────────────────────────────

    /// <summary>Returns true while the named key is simulated as held by a Cephable macro.</summary>
    public bool GetKey(string keyName) =>
        keyStates.TryGetValue(keyName, out bool state) && state;

    /// <summary>Returns true while the corresponding KeyCode is simulated as held.</summary>
    public bool GetKey(KeyCode keyCode) => GetKey(keyCode.ToString());

    /// <summary>
    /// Returns the current simulated joystick axis value (-1 to 1).
    /// Valid axis names: "LeftX", "LeftY", "RightX", "RightY"
    /// </summary>
    public float GetAxis(string axisName)
    {
        return axisName switch
        {
            "LeftX"  => joystickLeftX,
            "LeftY"  => joystickLeftY,
            "RightX" => joystickRightX,
            "RightY" => joystickRightY,
            _        => 0f,
        };
    }

    // ─── Hub Connection ───────────────────────────────────────────────────────

    public async Task ConnectToHub()
    {
        try
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{CephableApiBaseUrl}/device", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(userDeviceToken);
                    options.Headers.Add("X-Device-Token", userDeviceToken);
                    options.Headers.Add("User-Agent", "Unity-Plugin");
                })
                .Build();

            hubConnection.On<string, MacroModel>("DeviceCommand", (command, macro) =>
            {
                output("Received command: " + command);

                OnCommandReceived?.Invoke(command);
                MacroCommandReceived?.Invoke(command, macro);

                if (macro?.events != null && macro.events.Count > 0)
                    StartCoroutine(ExecuteMacro(macro));
                else
                    StartCoroutine(ResetKeys());
            });

            hubConnection.On<UserDevice>("DeviceProfileUpdate", (device) =>
            {
                output("Received profile update");
                currentProfile = device.currentProfile?.configuration;
            });

            output("Connecting to hub");
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("VerifySelf");
            output("Connected to hub");
        }
        catch (Exception ex)
        {
            output(ex.Message);
            output(ex.StackTrace);
        }
    }

    // ─── Macro Execution ──────────────────────────────────────────────────────

    private IEnumerator ExecuteMacro(MacroModel macro)
    {
        foreach (var evt in macro.events)
        {
            switch (evt.eventType)
            {
                case MacroEventType.KeyPress:
                    SetKeyStates(evt.keys, true);
                    if (evt.holdTimeMilliseconds.HasValue && evt.holdTimeMilliseconds.Value > 0)
                    {
                        yield return new WaitForSeconds(evt.holdTimeMilliseconds.Value / 1000f);
                        SetKeyStates(evt.keys, false);
                    }
                    break;

                case MacroEventType.KeyRelease:
                    SetKeyStates(evt.keys, false);
                    break;

                case MacroEventType.KeyToggle:
                    ToggleKeyStates(evt.keys);
                    break;

                case MacroEventType.Pause:
                    yield return new WaitForSeconds((evt.holdTimeMilliseconds ?? 100) / 1000f);
                    break;

                case MacroEventType.JoysticksMove:
                    ApplyJoystickMove(evt);
                    break;

                case MacroEventType.DeviceTypeCustomAction:
                    TriggerCustomAction(evt);
                    break;

                case MacroEventType.StopOutputs:
                    ResetAllStates();
                    break;
            }
        }

        // Small delay then reset any non-toggled keys so held states don't linger
        yield return new WaitForSeconds(0.1f);
        ResetTransientKeys();
    }

    private IEnumerator ResetKeys()
    {
        yield return new WaitForSeconds(0.1f);
        ResetTransientKeys();
    }

    // ─── Input State Helpers ──────────────────────────────────────────────────

    private void SetKeyStates(List<string> keys, bool pressed)
    {
        if (keys == null) return;
        foreach (var key in keys)
        {
            keyStates[key] = pressed;
            OnKeyStateChanged?.Invoke(key, pressed);
        }
    }

    private void ToggleKeyStates(List<string> keys)
    {
        if (keys == null) return;
        foreach (var key in keys)
        {
            bool next = !(keyStates.TryGetValue(key, out bool cur) && cur);
            keyStates[key] = next;
            OnKeyStateChanged?.Invoke(key, next);
        }
    }

    private void ApplyJoystickMove(MacroEvent evt)
    {
        if (evt.joystickLeftMoveX.HasValue)  joystickLeftX  = Normalize(evt.joystickLeftMoveX.Value);
        if (evt.joystickLeftMoveY.HasValue)  joystickLeftY  = Normalize(evt.joystickLeftMoveY.Value);
        if (evt.joystickRightMoveX.HasValue) joystickRightX = Normalize(evt.joystickRightMoveX.Value);
        if (evt.joystickRightMoveY.HasValue) joystickRightY = Normalize(evt.joystickRightMoveY.Value);
        OnJoystickChanged?.Invoke(joystickLeftX, joystickLeftY, joystickRightX, joystickRightY);
    }

    private void TriggerCustomAction(MacroEvent evt)
    {
        string typeId = evt.deviceTypeId ?? string.Empty;
        string actionId = evt.deviceTypeCustomActionId ?? string.Empty;
        string content = evt.additionalInputContent ?? string.Empty;

        OnCustomActionTriggered?.Invoke(typeId, actionId, content);
        CustomActionReceived?.Invoke(typeId, actionId, content);
        output($"Custom action: deviceType={typeId} action={actionId} content={content}");
    }

    /// <summary>Resets keys that were set via KeyPress (not KeyToggle), and joystick axes.</summary>
    private void ResetTransientKeys()
    {
        var keys = new List<string>(keyStates.Keys);
        foreach (var key in keys)
        {
            if (keyStates[key])
            {
                keyStates[key] = false;
                OnKeyStateChanged?.Invoke(key, false);
            }
        }

        if (joystickLeftX != 0 || joystickLeftY != 0 || joystickRightX != 0 || joystickRightY != 0)
        {
            joystickLeftX = joystickLeftY = joystickRightX = joystickRightY = 0f;
            OnJoystickChanged?.Invoke(0f, 0f, 0f, 0f);
        }
    }

    /// <summary>Resets all simulated input state immediately (e.g. for StopOutputs macro events).</summary>
    private void ResetAllStates()
    {
        var keys = new List<string>(keyStates.Keys);
        foreach (var key in keys)
        {
            keyStates[key] = false;
            OnKeyStateChanged?.Invoke(key, false);
        }
        joystickLeftX = joystickLeftY = joystickRightX = joystickRightY = 0f;
        OnJoystickChanged?.Invoke(0f, 0f, 0f, 0f);
    }

    /// <summary>Normalizes an integer joystick value (assumed -100 to 100) to -1..1.</summary>
    private static float Normalize(int value) => Mathf.Clamp(value / 100f, -1f, 1f);

    // ─── Device Registration ──────────────────────────────────────────────────

    public IEnumerator CreateVirtualController()
    {
        var accessToken = PlayerPrefs.GetString("accessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            output("No access token found, please sign in");
            yield break;
        }

        var www = UnityWebRequest.Post(
            $"{CephableApiBaseUrl}/api/Device/userDevices/new/{DeviceTypeId}?name={DefaultDeviceName}",
            string.Empty);
        www.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var deviceResponse = UserDevice.CreateFromJSON(www.downloadHandler.text);
            PlayerPrefs.SetString("userDeviceId", deviceResponse.id);
            userDeviceId = deviceResponse.id;
            StartCoroutine(CreateDeviceToken());
        }
        else
        {
            output($"Failed to create device: {www.error} {www.downloadHandler.text}");
        }
    }

    public IEnumerator CreateDeviceToken()
    {
        var accessToken = PlayerPrefs.GetString("accessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            output("No access token found, please sign in");
            yield break;
        }

        var www = UnityWebRequest.Post(
            $"{CephableApiBaseUrl}/api/Device/userDevices/{userDeviceId}/tokens",
            string.Empty);
        www.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var tokenResponse = UserDeviceToken.CreateFromJSON(www.downloadHandler.text);
            PlayerPrefs.SetString("userDeviceToken", tokenResponse.token);
            userDeviceToken = tokenResponse.token;
            _ = ConnectToHub();
        }
        else
        {
            output($"Failed to create device token: {www.error} {www.downloadHandler.text}");
        }
    }

    // ─── Utilities ────────────────────────────────────────────────────────────

    public void output(string message)
    {
        Debug.Log($"[Cephable] {message}");
    }

    private static Dictionary<string, KeyCode> BuildKeyCodeMap()
    {
        var map = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase)
        {
            { "Space",        KeyCode.Space },
            { "Return",       KeyCode.Return },
            { "Enter",        KeyCode.Return },
            { "Escape",       KeyCode.Escape },
            { "Tab",          KeyCode.Tab },
            { "Backspace",    KeyCode.Backspace },
            { "Delete",       KeyCode.Delete },
            { "Insert",       KeyCode.Insert },
            { "Home",         KeyCode.Home },
            { "End",          KeyCode.End },
            { "PageUp",       KeyCode.PageUp },
            { "PageDown",     KeyCode.PageDown },
            { "UpArrow",      KeyCode.UpArrow },
            { "Up",           KeyCode.UpArrow },
            { "DownArrow",    KeyCode.DownArrow },
            { "Down",         KeyCode.DownArrow },
            { "LeftArrow",    KeyCode.LeftArrow },
            { "Left",         KeyCode.LeftArrow },
            { "RightArrow",   KeyCode.RightArrow },
            { "Right",        KeyCode.RightArrow },
            { "LeftShift",    KeyCode.LeftShift },
            { "RightShift",   KeyCode.RightShift },
            { "Shift",        KeyCode.LeftShift },
            { "LeftControl",  KeyCode.LeftControl },
            { "RightControl", KeyCode.RightControl },
            { "Control",      KeyCode.LeftControl },
            { "LeftAlt",      KeyCode.LeftAlt },
            { "RightAlt",     KeyCode.RightAlt },
            { "Alt",          KeyCode.LeftAlt },
            { "F1",  KeyCode.F1  }, { "F2",  KeyCode.F2  }, { "F3",  KeyCode.F3  },
            { "F4",  KeyCode.F4  }, { "F5",  KeyCode.F5  }, { "F6",  KeyCode.F6  },
            { "F7",  KeyCode.F7  }, { "F8",  KeyCode.F8  }, { "F9",  KeyCode.F9  },
            { "F10", KeyCode.F10 }, { "F11", KeyCode.F11 }, { "F12", KeyCode.F12 },
        };

        // A-Z
        for (char c = 'A'; c <= 'Z'; c++)
            if (Enum.TryParse(c.ToString(), out KeyCode kc))
                map[c.ToString()] = kc;

        // 0-9 (both "0" and "Alpha0" forms)
        for (int i = 0; i <= 9; i++)
        {
            if (Enum.TryParse($"Alpha{i}", out KeyCode kc))
            {
                map[i.ToString()] = kc;
                map[$"Alpha{i}"] = kc;
                map[$"D{i}"] = kc;
            }
        }

        return map;
    }
}
