using UnityEngine;
using Unity.Cinemachine;

namespace ZE.MechBattle
{  

    public class FirstPersonViewCamera : MonoBehaviour
    {
        [Header("Mouse Sensitivity")]
        public float mouseSensitivityX = 2.0f;
        public float mouseSensitivityY = 2.0f;

        [SerializeField] private float _minVerticalAngle = -80.0f; 
        [SerializeField] private float _maxVerticalAngle = 80.0f;
        [SerializeField] private float _minHorizontalAngle = -80.0f;
        [SerializeField] private float _maxHorizontalAngle = 80.0f;
        [SerializeField] private bool _inverseXRotation = true;
        private float xRotation = 0f;
        private float yRotation = 0f;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

            if (_inverseXRotation)
                xRotation -= mouseY;
            else
                xRotation += mouseY;
            xRotation = Mathf.Clamp(xRotation, _minVerticalAngle, _maxVerticalAngle);

            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, _minHorizontalAngle, _maxHorizontalAngle);

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
