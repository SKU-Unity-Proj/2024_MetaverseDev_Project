using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace _01_04_Network_Properties
{
    public class FairyFollower : MonoBehaviour
    {
        public Transform player;  // �÷��̾� Transform
        public float followDistance = 2.0f;  // �÷��̾ ������� �Ÿ�

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
        }
    }
}