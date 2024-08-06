using UnityEngine;

namespace _01_04_Network_Properties
{
    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform Target; // Ÿ���� Transform
        public float MouseSensitivity = 10f; // ���콺 �ΰ���

        private float verticalRotation;
        private float horizontalRotation;

        public float distance = 2.76f;  // ī�޶�� �÷��̾� ������ �Ÿ�
        public float height = 2.0f;  // ī�޶��� ����

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            // ���콺 �Է� �ޱ�
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // ī�޶� ȸ�� ����
            verticalRotation -= mouseY * MouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);

            horizontalRotation += mouseX * MouseSensitivity;

            // ī�޶��� ȸ�� ����
            Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            transform.rotation = rotation;

            // ī�޶��� ��ġ�� Ÿ�� �������� ����
            Vector3 offset = new Vector3(0, height, -distance);
            transform.position = Target.position + rotation * offset;
        }
    }
}
