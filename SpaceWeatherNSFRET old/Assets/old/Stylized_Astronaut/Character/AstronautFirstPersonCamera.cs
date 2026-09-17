using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AstronautFirstPersonCamera
{
    public class AstronautFirstPersonCamera : MonoBehaviour
    {
        public Transform playerBody;  // The player's body (to rotate with yaw)
        public float sensitivityX = 2.0f;
        public float sensitivityY = 2.0f;

        private float xRotation = 0f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate camera vertically (pitch)
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotate player body horizontally (yaw)
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
