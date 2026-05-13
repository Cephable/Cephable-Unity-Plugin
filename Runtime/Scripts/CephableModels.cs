using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Cephable.Unity.Plugin
{
    [System.Serializable]
    public class UserDevice
    {
        public string id;
        public string nameOverride;
        public bool isVerified;
        public bool isConnected;
        public bool isListening;
        public bool isAutoListen;
        public bool isOptimisticModel;
        public UserDeviceProfile currentProfile;
        public static UserDevice CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<UserDevice>(jsonString);
        }
    }

    [System.Serializable]
    public class UserDeviceProfile
    {
        public string id;
        public string name;
        public string userId;
        public string profileType;
        public DeviceProfileConfiguration configuration;
        public string createdDate;
        public string modifiedDate;
    }



    [System.Serializable]
    public class DeviceProfileConfiguration
    {
        public string profileType;
        public List<KeyCommandMapping> commandKeyMappings;
        public List<MacroModel> macros;
        public List<HotkeyModel> hotkeys;
        public List<string> dictationCommands;
        public List<AudioEventModel> audioEvents;
    }

    [System.Serializable]
    public class KeyCommandMapping
    {
        public string key;
        public List<string> commands;
    }

    [System.Serializable]
    public class MacroModel
    {
        public string name;
        public List<string> commands;
        public List<MacroEvent> events;
    }

    [System.Serializable]
    public class MacroEvent
    {
        public MacroEventType eventType;
        public List<string> keys;
        public int? holdTimeMilliseconds;
        public string typedPhrase;
        public int? mouseMoveX;
        public int? mouseMoveY;
        public int? mouseMoveZ;
        public int? joystickLeftMoveX;
        public int? joystickLeftMoveY;
        public int? joystickRightMoveX;
        public int? joystickRightMoveY;
        public string outputSpeech;
        // custom device actions

        /// <summary>
        /// The ID of the custom action within the provided device type
        /// </summary>
        public string deviceTypeCustomActionId;

        /// <summary>
        /// The device type ID that indicates this macro event should only be run on a specific device type.
        /// Combine this with the <see cref="deviceTypeCustomActionId"/> for a specific action of this device type
        /// </summary>
        public string deviceTypeId;

        /// <summary>
        /// Used with prefix string content to allow for open ended commands and custom execution
        /// </summary>
        public string additionalInputContent;

        /// <summary>
        /// Used for device type custom actions to indicate it should toggle the state off or on based on the state being held on the device.
        /// </summary>
        public bool isCustomToggle;

        /// <summary>
        /// Used for a device type custom action to indicate that it should release the action if it is held or latched
        /// </summary>
        public bool isCustomRelease;
    }

    [System.Serializable]
    public enum MacroEventType
    {
        KeyPress,
        Pause,
        Type,
        MouseMove,
        JoysticksMove,
        PlayAudio,
        KeyRelease,
        StopOutputs,
        KeyToggle,
        DeviceTypeCustomAction
    }

    [System.Serializable]
    public class HotkeyModel
    {
        public string displayName;
        public string command;
    }

    [System.Serializable]
    public class AudioEventModel
    {
        public string name;
        public List<string> commands;
        public string audioFileUrl;
        public string outputSpeech;
    }

    [System.Serializable]
    public class UserDeviceToken
    {
        public string id;
        public string userDeviceId;
        public string token;
        public bool isDisabled;
        public static UserDeviceToken CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<UserDeviceToken>(jsonString);
        }

    }


}