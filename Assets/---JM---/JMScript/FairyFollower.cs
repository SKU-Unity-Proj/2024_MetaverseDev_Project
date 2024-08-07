using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace _01_04_Network_Properties
{
    public class FairyFollower : MonoBehaviour
    {
        public Transform player;  // 플레이어 Transform
        public float followDistance = 2.0f;  // 플레이어를 따라오는 거리

        private NavMeshAgent agent;
        private Animator animator;

        private PlayerMovement2 playerMovement;

        private float radius = 5.0f;
        public LayerMask layerMask;

        void Start()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                Debug.Log("Hit: " + hitCollider.name);
                player = hitCollider.gameObject.transform;
            }

            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

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
        }
    }
}