using UnityEngine;

namespace ZE.MechBattle
{
    public class CabinController : MonoBehaviour
    {
        [SerializeField] private float _yRotationLimit = 80f;
        [SerializeField] private float _xRotationLimit = 30f;
        [SerializeField] private float _yRotationSpeed = 30f;
        [SerializeField] private float _xRotationSpeed = 15f;
        private float _xRotation = 0f;
        private float _yRotation = 0f;

        private void Update()
        {
            var dt = Time.deltaTime;

            if (Input.GetKey(KeyCode.Q))
            {
                _yRotation -= dt * _yRotationSpeed;
            }
            else
            {
                if (Input.GetKey(KeyCode.E))
                    _yRotation += dt * _yRotationSpeed;
            }

            if (Input.GetKey(KeyCode.Z))
            {
                _xRotation -= dt * _xRotationSpeed;
            }
            else
            {
                if (Input.GetKey(KeyCode.X))
                    _xRotation += dt * _xRotationSpeed;
            }
            _yRotation = Mathf.Clamp(_yRotation, - _yRotationLimit, _yRotationLimit);
            _xRotation = Mathf.Clamp(_xRotation, -_xRotationLimit, _xRotationLimit);

            transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        }

    }
}
