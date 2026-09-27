using UnityEngine;

namespace VRARLab1
{
    /// <summary>
    /// Simple test-camera controller that intentionally uses Unity's classic Input API,
    /// as required by Lab 1. Controls: WASD = move, Q/E = down/up,
    /// arrow keys or RMB + mouse = look, Left Shift = faster movement.
    /// </summary>
    public class CameraKeyboardController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 4f;
        public float sprintMultiplier = 2f;

        [Header("Look")]
        public float keyboardLookSpeed = 70f;
        public float mouseLookSensitivity = 2.2f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        private float yaw;
        private float pitch;

        private void Start()
        {
            Vector3 euler = transform.eulerAngles;
            yaw = euler.y;
            pitch = NormalizePitch(euler.x);
        }

        private void Update()
        {
            HandleMovement();
            HandleLook();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            float verticalWorld = 0f;
            if (Input.GetKey(KeyCode.E)) verticalWorld += 1f;
            if (Input.GetKey(KeyCode.Q)) verticalWorld -= 1f;

            Vector3 movement = transform.right * horizontal + transform.forward * vertical + Vector3.up * verticalWorld;
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            transform.position += movement * speed * Time.deltaTime;
        }

        private void HandleLook()
        {
            float lookX = 0f;
            float lookY = 0f;

            if (Input.GetKey(KeyCode.LeftArrow)) lookX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) lookX += 1f;
            if (Input.GetKey(KeyCode.UpArrow)) lookY += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) lookY -= 1f;

            yaw += lookX * keyboardLookSpeed * Time.deltaTime;
            pitch -= lookY * keyboardLookSpeed * Time.deltaTime;

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseLookSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseLookSensitivity;
            }

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private static float NormalizePitch(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
