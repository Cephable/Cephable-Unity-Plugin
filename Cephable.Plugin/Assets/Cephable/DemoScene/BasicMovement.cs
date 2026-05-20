using Cephable.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float horizontalInput, verticalInput;

    public VirtualController CephableController;

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Apply forces to move
        GetComponent<Rigidbody>().AddForce(
            new Vector3(horizontalInput, 0, verticalInput) * moveSpeed,
            ForceMode.Force
        );
    }

    void OnEnable()
    {
        if (CephableController == null) return;
        CephableController.MacroCommandReceived += OnMacroCommand;
        CephableController.CustomActionReceived += OnCustomAction;
        CephableController.OnKeyStateChanged.AddListener(OnKeyState);
        CephableController.OnJoystickChanged.AddListener(OnJoystick);
    }

    void OnDisable()
    {
        if(CephableController == null) return;
        CephableController.MacroCommandReceived -= OnMacroCommand;
        CephableController.CustomActionReceived -= OnCustomAction;
        CephableController.OnKeyStateChanged.RemoveListener(OnKeyState);
        CephableController.OnJoystickChanged.RemoveListener(OnJoystick);
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

        if(key.ToLower() == "space" && isPressed)
        {
            Jump();
        }
    }

    void OnJoystick(float lx, float ly, float rx, float ry)
    {
        Debug.Log($"Left stick: ({lx:F2}, {ly:F2})  Right stick: ({rx:F2}, {ry:F2})");
    }

    void Jump()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
}
