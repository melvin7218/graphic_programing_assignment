using UnityEngine;

public class HorizontalRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    public bool useMouseInput = true;
    public float mouseSensitivity = 2f;

    void Update()
    {
        HandleRotationInput();
    }

    void HandleRotationInput()
    {
        float rotationAmount = 0f;

        // Keyboard input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            rotationAmount = rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            rotationAmount = -rotationSpeed * Time.deltaTime;
        }

        // Mouse input (alternative)
        if (useMouseInput && Input.GetMouseButton(0))
        {
            rotationAmount = Input.GetAxis("Mouse X") * mouseSensitivity * rotationSpeed * Time.deltaTime;
        }

        // Apply rotation around the Y-axis (horizontal rotation)
        if (rotationAmount != 0f)
        {
            transform.Rotate(0, rotationAmount, 0, Space.World);
        }
    }
}