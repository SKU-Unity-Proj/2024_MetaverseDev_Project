using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace _01_04_Network_Properties
{
    public class FairyFollower : MonoBehaviour
    {
        public Transform player;  // �÷��̾� Transform
        public float followDistance = 2.0f;  // �÷��̾ ������� �Ÿ�
        public GameObject uiPanel;  // UI �г�

        private NavMeshAgent agent;
        private Animator animator;

        private PlayerMovement2 playerMovement;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            uiPanel.SetActive(false);  // �ʱ⿡�� UI ��Ȱ��ȭ

            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement2>();
            }

            //LockCursor(true);  // �ʱ⿡�� ���콺 ��
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            // playerMovement �ʱ�ȭ (�� ���� ����ǵ���)
            if (playerMovement == null)
            {
                playerMovement = player.GetComponent<PlayerMovement2>();
                if (playerMovement == null)
                {
                    return;
                }
            }

            // �÷��̾�� ���� NPC ������ �Ÿ� ���
            float distance = Vector3.Distance(player.position, transform.position);

            // �÷��̾ ����ٴϴ� ����
            if (distance > followDistance)
            {
                agent.SetDestination(player.position);
                animator.SetBool("isWalking", true);
            }
            else
            {
                agent.ResetPath();
                animator.SetBool("isWalking", false);
            }

            // ���� NPC�� ��ȣ�ۿ� (Raycasting)
            if (Input.GetMouseButtonDown(0))  // ���� ���콺 ��ư Ŭ��
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == this.transform)
                    {
                        ToggleUI();
                    }
                }
            }

            // ESC ��ư���� UI ��Ȱ��ȭ
            if (Input.GetKeyDown(KeyCode.Escape) && uiPanel.activeSelf)
            {
                ToggleUI();
            }
        }

        // UI�� ����ϴ� �Լ�
        void ToggleUI()
        {
            bool isActive = !uiPanel.activeSelf;
            uiPanel.SetActive(isActive);
            LockCursor(!isActive);
            Debug.Log("UI ���°� ����Ǿ����ϴ�!");
        }

        // ���콺 ��/���� �Լ�
        void LockCursor(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }
    }
}