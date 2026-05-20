using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float horizontalInput, verticalInput;

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }

        // Apply forces to move
        GetComponent<Rigidbody>().AddForce(
            new Vector3(horizontalInput, 0, verticalInput) * moveSpeed,
            ForceMode.Force
        );
    }
}
