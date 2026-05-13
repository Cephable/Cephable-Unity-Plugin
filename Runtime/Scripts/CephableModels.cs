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
    public string eventType;
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

    // TODO: update with custom controls
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