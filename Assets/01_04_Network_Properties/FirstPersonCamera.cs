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

        public Vector3 offset = new Vector3(0, 3.07f, 0.24f); // 카메라의 위치 오프셋 (머리 위치에 고정)

        private PlayerMovement2 playerMovement;
        private int originalCullingMask;

        void Start()
        {
            if (Target != null)
            {
                playerMovement = Target.GetComponent<PlayerMovement2>();
            }
            originalCullingMask = GetComponent<Camera>().cullingMask; // 카메라의 원래 cullingMask 저장
        }

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            // playerMovement 초기화 (한 번만 실행되도록)
            if (playerMovement == null)
            {
                playerMovement = Target.GetComponent<PlayerMovement2>();
                if (playerMovement == null)
                {
                    return;
                }
            }

            Camera cameraComponent = GetComponent<Camera>();

            if (playerMovement.IsSeated())
            {
                Debug.Log("1인칭 진입");

                cameraComponent.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));

                MouseSensitivity = 3;

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
            else
            {
                // 3인칭 시점
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

                // Player 레이어 활성화
                cameraComponent.cullingMask = originalCullingMask;
            }
        }
    }
}
