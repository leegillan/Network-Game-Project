using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float sensitivity = 100.0f;
    public float clampAngle = 85.0f;

    private float verticalRotation;
    private float horizontalRotation;

    private void Start()
    {
        ToggleCursorMode();

        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorMode();
        }

        if(Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
        }

        Debug.DrawRay(transform.position, transform.forward * 2, Color.red);
    }

    private void Look()
    {
        float mouseVert = -Input.GetAxis("Mouse Y");
        float mouseHor = Input.GetAxis("Mouse X"); ;

        verticalRotation += mouseVert * sensitivity * Time.deltaTime;
        horizontalRotation += mouseHor * sensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0.0f, 0.0f);

        player.transform.rotation = Quaternion.Euler(0.0f, horizontalRotation, 0.0f);
    }

    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
