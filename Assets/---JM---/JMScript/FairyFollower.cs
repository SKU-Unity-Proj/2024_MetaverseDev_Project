using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace _01_04_Network_Properties
{
    public class FairyFollower : MonoBehaviour
    {
        public Transform player;  // 플레이어 Transform
        public float followDistance = 2.0f;  // 플레이어를 따라오는 거리
        public GameObject uiPanel;  // UI 패널

        private NavMeshAgent agent;
        private Animator animator;

        private PlayerMovement2 playerMovement;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            uiPanel.SetActive(false);  // 초기에는 UI 비활성화

            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement2>();
            }

            //LockCursor(true);  // 초기에는 마우스 락
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            // playerMovement 초기화 (한 번만 실행되도록)
            if (playerMovement == null)
            {
                playerMovement = player.GetComponent<PlayerMovement2>();
                if (playerMovement == null)
                {
                    return;
                }
            }

            // 플레이어와 요정 NPC 사이의 거리 계산
            float distance = Vector3.Distance(player.position, transform.position);

            // 플레이어를 따라다니는 로직
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

            // 요정 NPC와 상호작용 (Raycasting)
            if (Input.GetMouseButtonDown(0))  // 왼쪽 마우스 버튼 클릭
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

            // ESC 버튼으로 UI 비활성화
            if (Input.GetKeyDown(KeyCode.Escape) && uiPanel.activeSelf)
            {
                ToggleUI();
            }
        }

        // UI를 토글하는 함수
        void ToggleUI()
        {
            bool isActive = !uiPanel.activeSelf;
            uiPanel.SetActive(isActive);
            LockCursor(!isActive);
            Debug.Log("UI 상태가 변경되었습니다!");
        }

        // 마우스 락/해제 함수
        void LockCursor(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }
    }
}