using UnityEngine;

namespace _01_04_Network_Properties
{
    public class SecondCam : MonoBehaviour
    {
        public Transform Target; // 타겟의 Transform
        public float MouseSensitivity = 10f; // 마우스 민감도

        private float verticalRotation;
        private float horizontalRotation;

        public Vector3 offset = new Vector3(0, 1.6f, 0); // 카메라의 위치 오프셋 (머리 위치에 고정)

        void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
        }

        void Update()
        {
            if (Target == null)
            {
                return;
            }

            // 마우스 입력 받기
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

            verticalRotation -= mouseY * MouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);

            horizontalRotation += mouseX * MouseSensitivity;

            // 카메라의 회전 적용
            Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            transform.rotation = rotation;

            // 카메라의 위치를 타겟 기준으로 설정
            transform.position = Target.position + offset;
        }
    }
}
