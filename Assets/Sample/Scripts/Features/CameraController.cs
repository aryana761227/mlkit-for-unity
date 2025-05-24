using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed for WASD movement
    public float dragSpeed = 0.1f; // Speed for drag movement

    private Vector3 dragOrigin;

    void Update()
    {
        // Handle WASD movement for keyboard
        HandleKeyboardInput();

        // Handle drag movement for touch
        HandleTouchInput();
    }

    private void HandleKeyboardInput()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 touchDelta = touch.deltaPosition;
                Vector3 move = new Vector3(-touchDelta.x * dragSpeed, 0, -touchDelta.y * dragSpeed);
                transform.Translate(move, Space.World);
            }
        }
    }
}
