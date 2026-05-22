# Cephable Unity Plugin

The official Unity plugin for [Cephable](https://cephable.com) — adaptive controls that let players use voice commands, facial expressions, head tracking, gestures, and custom switches to control your game.

## Why Add Cephable Support?

**Reach more players.** Millions of players have motor, mobility, and dexterity differences that make standard controllers difficult or impossible to use. Cephable lets them play your game their way, using whatever input method works for them.

**No redesign required.** Cephable maps adaptive inputs to your existing game actions. Players configure their own controls through the Cephable app — you don't need to rebuild your input system.

**Stand out in the Cephable marketplace.** Games with native Cephable support are featured to the Cephable community. Adding the plugin makes your game discoverable to players actively seeking accessible titles.

**Flexible integration.** Commands arrive via SignalR in real time. You can handle them with Unity's inspector-wired events, C# event subscriptions, or by polling simulated input — whichever fits your architecture.

---

## Requirements

- Unity 2019.1 or later
- A [Cephable developer account](https://developers.cephable.com/)
- Your Cephable OAuth2 Client ID and Client Secret
- A Cephable Device Type ID for your game

---

## Installation

### Via Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click the **+** button and choose **Add package from git URL**
3. Enter the repository URL and click **Add**

### Via Local Disk

1. Clone or download this repository
2. Open **Window → Package Manager**
3. Click **+** → **Add package from disk** and select `package.json`

---

## Getting Started

### 1. Set Up Your Cephable App

Log in to the [Cephable Developer Portal](https://developers.cephable.com/) and:

- Create an application to receive your **Client ID** and **Client Secret**
- Create a **Device Type** for your game and note its **Device Type ID**
- Configure the redirect URI: `http://127.0.0.1:51772/`

### 2. Add Components to Your Scene

Create a new GameObject (e.g. `CephableController`) and add two components:

- **OAuth2Manager** — handles player sign-in
- **VirtualController** — registers the device and receives commands

Link them: drag the `VirtualController` object into the **Controller** field on `OAuth2Manager`.

### 3. Configure OAuth2Manager

In the Inspector, fill in:

| Field | Value |
|---|---|
| Client ID | Your Cephable application Client ID |
| Client Secret | Your Cephable application Client Secret |

### 4. Configure VirtualController

In the Inspector, fill in:

| Field | Value |
|---|---|
| Device Type ID | Your Cephable Device Type ID |
| Default Device Name | Displayed to players in the Cephable app (e.g. `"My Game Controller"`) |

### 5. Add a Login Button

Add a UI **Button** to your scene (e.g. in a settings or pause menu). Add a **LoginButton** component to it and wire up:

- **Oauth2Manager** → your `OAuth2Manager` component
- **Login Button** → the UI Button component

When the player clicks the button, they are taken to the Cephable sign-in page in their browser. After signing in, the plugin registers a virtual device and opens the SignalR connection automatically.

---

## Handling Commands

When a player performs an adaptive action in Cephable (voice command, facial expression, etc.), the plugin fires a `DeviceCommand` event containing the command name and an optional `MacroModel` describing what keys or actions to trigger.

There are three ways to respond:

---

### Option A — Unity Events (Inspector, No Code Required)

Select your `VirtualController` GameObject and wire up the **Unity Events** section in the Inspector:

| Event | When it fires | Parameters |
|---|---|---|
| `OnCommandReceived` | Every incoming command | `string` command name |
| `OnKeyStateChanged` | A key is pressed or released by a macro | `string` key name, `bool` isPressed |
| `OnJoystickChanged` | Joystick axes change | `float` leftX, leftY, rightX, rightY (-1 to 1) |
| `OnCustomActionTriggered` | A custom device action fires | `string` deviceTypeId, actionId, additionalContent |

Drag your game component into the event slot and choose the method to call. No scripting needed for simple mappings.

---

### Option B — C# Event Subscription

Subscribe to the typed C# events on `VirtualController` from any MonoBehaviour:

```csharp
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private VirtualController cephable;

    void OnEnable()
    {
        cephable.MacroCommandReceived  += OnMacroCommand;
        cephable.CustomActionReceived  += OnCustomAction;
        cephable.OnKeyStateChanged.AddListener(OnKeyState);
        cephable.OnJoystickChanged.AddListener(OnJoystick);
    }

    void OnDisable()
    {
        cephable.MacroCommandReceived  -= OnMacroCommand;
        cephable.CustomActionReceived  -= OnCustomAction;
        cephable.OnKeyStateChanged.RemoveListener(OnKeyState);
        cephable.OnJoystickChanged.RemoveListener(OnJoystick);
    }

    void OnMacroCommand(string command, MacroModel macro)
    {
        Debug.Log($"Command received: {command}");
        // Handle command names like "jump", "fire", "crouch", etc.
    }

    void OnCustomAction(string deviceTypeId, string actionId, string content)
    {
        Debug.Log($"Custom action: {actionId}");
    }

    void OnKeyState(string key, bool isPressed)
    {
        Debug.Log($"Key {key} is now {(isPressed ? "pressed" : "released")}");
    }

    void OnJoystick(float lx, float ly, float rx, float ry)
    {
        Debug.Log($"Left stick: ({lx:F2}, {ly:F2})  Right stick: ({rx:F2}, {ry:F2})");
    }
}
```

---

### Option C — Simulated Input Polling

`VirtualController` tracks simulated key and axis states as macros execute. Poll these in `Update()` alongside or instead of Unity's `Input` API:

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private VirtualController cephable;
    [SerializeField] private Rigidbody rb;

    void Update()
    {
        // Combine standard keyboard/gamepad with Cephable adaptive input
        bool jump   = Input.GetKeyDown(KeyCode.Space) || cephable.GetKey(KeyCode.Space);
        bool fire   = Input.GetKeyDown(KeyCode.Mouse0) || cephable.GetKey("Fire");
        float moveX = Input.GetAxis("Horizontal") + cephable.GetAxis("LeftX");
        float moveY = Input.GetAxis("Vertical")   + cephable.GetAxis("LeftY");

        moveX = Mathf.Clamp(moveX, -1f, 1f);
        moveY = Mathf.Clamp(moveY, -1f, 1f);

        if (jump) rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        rb.velocity = new Vector3(moveX * 5f, rb.velocity.y, moveY * 5f);
    }
}
```

**`GetKey(string keyName)`** — returns `true` while that key is simulated as held.  
**`GetKey(KeyCode keyCode)`** — same, accepting a Unity `KeyCode`.  
**`GetAxis(string axisName)`** — returns a value from -1 to 1 for `"LeftX"`, `"LeftY"`, `"RightX"`, or `"RightY"`.

---

## Macro Events

The `MacroModel` passed to `MacroCommandReceived` contains a list of `MacroEvent` objects. The plugin executes these automatically, but you can read them for custom handling:

| Event Type | What happens |
|---|---|
| `KeyPress` | Sets named keys as held; releases after `holdTimeMilliseconds` if set |
| `KeyRelease` | Releases named keys |
| `KeyToggle` | Toggles named keys between pressed and released |
| `Pause` | Waits `holdTimeMilliseconds` before continuing |
| `JoysticksMove` | Updates left/right joystick axis values |
| `DeviceTypeCustomAction` | Fires `OnCustomActionTriggered` / `CustomActionReceived` |
| `StopOutputs` | Releases all held keys and resets joystick axes to zero |

---

## API Reference

### VirtualController

| Member | Type | Description |
|---|---|---|
| `DeviceTypeId` | `string` | Your Cephable Device Type ID (set in Inspector) |
| `DefaultDeviceName` | `string` | Device name shown to the player in Cephable |
| `CephableApiBaseUrl` | `string` | API base URL (default: `https://services.cephable.com`) |
| `OnCommandReceived` | `UnityEvent<string>` | Fires on every incoming command |
| `OnKeyStateChanged` | `UnityEvent<string, bool>` | Fires when a key is pressed or released |
| `OnJoystickChanged` | `UnityEvent<float,float,float,float>` | Fires when joystick axes change |
| `OnCustomActionTriggered` | `UnityEvent<string,string,string>` | Fires for custom device actions |
| `MacroCommandReceived` | `event Action<string, MacroModel>` | C# event with full macro data |
| `CustomActionReceived` | `event Action<string, string, string>` | C# event for custom actions |
| `GetKey(string)` | `bool` | Poll simulated key state by name |
| `GetKey(KeyCode)` | `bool` | Poll simulated key state by KeyCode |
| `GetAxis(string)` | `float` | Poll simulated joystick axis (-1 to 1) |
| `CreateVirtualController()` | `IEnumerator` | Called automatically after sign-in |

---

## Player Experience

From the player's perspective:

1. They launch your game and click **Sign In with Cephable**
2. Their browser opens the Cephable sign-in page
3. After signing in, the plugin registers a device linked to their Cephable profile
4. In the Cephable app, they assign their adaptive inputs (voice commands, facial expressions, head movements, switches) to your game's actions
5. Their inputs arrive in real time as the game runs — no extra setup needed mid-session

Players can update their configuration at any time in the Cephable app without restarting your game. Profile updates are pushed via SignalR.

---

## Support

- Developer documentation: [developers.cephable.com](https://developers.cephable.com/)
- Support: [cephable.com/resources](https://cephable.com/resources)

---

## License

MIT — see [License.md](License.md)
