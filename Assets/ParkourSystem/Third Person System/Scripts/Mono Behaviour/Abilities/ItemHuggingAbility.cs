using DiasGames.Abilities;
using DiasGames.Components;
using UnityEngine.SceneManagement; // Add this line
using System.Collections;
using DiasGames.Debugging;
using TMPro;
using UnityEngine;

//ItemHugPos Position(0, -0.037, 0.336)

namespace DiasGames.Abilities
{
    [DisallowMultipleComponent]
    public class ItemHuggingAbility : AbstractAbility
    {
        private IMover _mover = null;

        [SerializeField] private float speed = 3f; // 아이템을 들고 있을때 속도

        [SerializeField] private float radius = 1f; // 아이템이 있는지 체크하는 원의 크기
        public Transform targetPos; // 아이템 안고 있을 위치
        public GameObject pickItem = null; // 주운 아이템 등록
        public bool haveItem = false; // 아이템 들고 있는지 상태 체크
        public bool liftingWait = false; // 집어드는 모션 기다리는 동안 움직임 제어
        private Rigidbody itemRigid; // 아이템의 리지디바디

        // 점프 매개변수
        [Header("Jump parameters")]
        [SerializeField] private float jumpHeight = 1.2f; // 점프 높이
        [SerializeField] private float speedOnAir = 6f; // 공중에서의 속도
        [SerializeField] private float airControl = 0.5f; // 공중 제어

        [SerializeField] private Transform originBottle;

        private bool isJump = false;
        private float _startSpeed;
        private Vector2 _startInput;

        private void Awake()
        {
            _mover = GetComponent<IMover>();
        }

        public override bool ReadyToRun() // 실행 조건
        {
            return _action.pickUp;
        }

        public override void OnStartAbility() // 실행될 때 호출
        {
            CheckItem();

            if(haveItem)
            {
                _mover.StopMovement(); // velocity 0

                SetAnimationState("ItemLift", 0.2f);

                Invoke("StartIdle", 2f);
            }
        }


        public override void UpdateAbility() // 실행 중에 계속 호출
        {
            if(liftingWait)
            {
                _mover.Move(_action.move, speed);

                HuggingItem();
                //originBottle.gameObject.GetComponent<MeshRenderer>().enabled = true;
                //originBottle.gameObject.GetComponent<MeshCollider>().enabled = true;
            }

            if (pickItem == null)
            {
                StopAbility();
                return;
            }

            // If the item's tag is "duck", start the coroutine to switch scenes
            if (pickItem.CompareTag("duck"))
            {
                StartCoroutine(SwitchToEndingScene());
            }

            if (_action.pickUp) // E키를 다시 누르면 어빌리티 중지
            {
                StopAbility();
                return;
            }
                
            if (!pickItem.activeSelf) // 아이템이 꺼지면 어빌리티 중지
            {
                StopAbility();
                return ;
            }
                
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("ItemLift")) // 실행중인 애니메이션이 IsName이면
                if(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)  // 애니메이션의 어느정도 완료 되었을시
                    liftingWait = true;
            
            if (_mover.IsGrounded())
            {
                if (isJump && _mover.GetVelocity().y < -3f) // 점프 후 착지
                {
                    SetAnimationState("Grounded", 0.25f);
                    isJump = false;
                }
                    
                if (_action.jump)
                    PerformJump(); // 점프 수행
            } 
        }

        public override void OnStopAbility() // 멈출때 호출
        {
            if (pickItem != null) // 물건 내려놓고 상태 초기화
            {
                pickItem.transform.position = targetPos.transform.position + targetPos.transform.forward * 0.5f;
                itemRigid.velocity = Vector3.zero;
                itemRigid.angularVelocity = Vector3.zero;

                pickItem.transform.parent = null;

                pickItem = null;
                haveItem = false;
                liftingWait = false;

                SetLayerPriority(1, 0); // 상체 애니메이션 우선순위 낮추기 (애니메이션 끄기)

                //SetAnimationState("ThrowItem", 0.5f, 0);
                SetAnimationState("Grounded", 0.3f, 1);
            }
        }



        /// <summary>
        /// 어빌리티 끝. 일반 함수들
        /// </summary>

        private void StartIdle()
        {
            SetAnimationState("ItemHugging_Idle", 0.3f, 1);
            SetLayerPriority(1, 1); // 상체 애니메이션 우선순위 높이기 (애니메이션 켜기)
        }

        private void CheckItem() // 바닥에 아이템을 주울 수 있는지 판별
        {
            if (!haveItem) // 아이템을 가지고 있지 않을때
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward, radius, 1 << 9);

                if (colliders != null)
                {
                    foreach (Collider collider in colliders)
                    {
                        haveItem = true;
                        pickItem = collider.gameObject;
                        itemRigid = pickItem.GetComponent<Rigidbody>();
                    }
                }
                else
                {
                    pickItem = null;
                    haveItem = false;
                }
            }
        }

        void HuggingItem() // 아이템을 타겟 위치 자식으로 보내기
        {
            if (haveItem && pickItem != null)
            {
                pickItem.transform.SetParent(targetPos);

                
            }
        }

        private void PerformJump()
        {
            Vector3 velocity = _mover.GetVelocity(); // 현재 속도 가져오기
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * _mover.GetGravity()); // 점프에 필요한 속도 계산

            _mover.SetVelocity(velocity); // 속도 설정
            _animator.CrossFadeInFixedTime("Air.Jump", 0.0f); // 점프 애니메이션 재생
            _startSpeed = speedOnAir; // 시작 속도 설정

            if (_startInput.magnitude > 0.1f)
                _startInput.Normalize(); // 입력 정규화

            isJump = true; //점프 했음을 알림
        }

        private IEnumerator SwitchToEndingScene()
        {
            yield return new WaitForSeconds(3f); // 3초 대기
            SceneManager.LoadScene("EndingAnimation"); // EndingAnimation 씬으로 전환
        }
    }
}

// 아이템을 내려놓으면 땅 아래로 뚫고 내려감
// 줍는 애니메이션이 실행중일때 움직여짐
// 애니메이션 상하체 분리