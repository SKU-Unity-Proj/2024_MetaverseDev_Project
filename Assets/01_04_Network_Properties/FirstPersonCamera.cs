using UnityEngine;

namespace _01_04_Network_Properties
{
    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform Target; // 타겟의 Transform
        public float MouseSensitivity = 10f; // 마우스 민감도

        private float verticalRotation;
        private float horizontalRotation;

        public float distance = 2.76f;  // 카메라와 플레이어 사이의 거리
        public float height = 2.0f;  // 카메라의 높이

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            // 마우스 입력 받기
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 카메라 회전 조정
            verticalRotation -= mouseY * MouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);

            horizontalRotation += mouseX * MouseSensitivity;

            // 카메라의 회전 적용
            Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            transform.rotation = rotation;

            // 카메라의 위치를 타겟 기준으로 설정
            Vector3 offset = new Vector3(0, height, -distance);
            transform.position = Target.position + rotation * offset;
        }
    }
}
