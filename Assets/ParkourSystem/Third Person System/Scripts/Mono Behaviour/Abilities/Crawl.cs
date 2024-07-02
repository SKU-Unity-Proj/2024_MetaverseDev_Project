using UnityEngine;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public class Crawl : AbstractAbility
    {
        [SerializeField] private float crawlSpeed = 2f; // 기어가는 속도
        [SerializeField] private float capsuleHeightOnCrawl = 0.5f; // 기어갈 때 캡슐의 높이

        [Header("Cast Parameters")]
        [SerializeField] private LayerMask obstaclesMask; // 장애물을 감지하는 데 사용되는 레이어 마스크
        [Tooltip("This is the height that sphere cast can reach to know when should force crawl state")]
        [SerializeField] private float MaxHeightToStartCrawl = 0.75f; // 기어가기 시작해야 하는 최대 높이

        [Header("Animation States")]
        [SerializeField] private string startCrawlAnimationState = "Stand to Crawl"; // 기어가기 시작하는 애니메이션 상태
        [SerializeField] private string stopCrawlAnimationState = "Crawl to Stand"; // 기어가기를 멈추는 애니메이션 상태

        private IMover _mover; // 이동을 제어하는 인터페이스
        private ICapsule _capsule; // 캡슐 콜라이더를 제어하는 인터페이스

        private bool _startingCrawl = false; // 기어가기 시작 중인지를 나타내는 플래그
        private bool _stoppingCrawl = false; // 기어가기를 중단 중인지를 나타내는 플래그

        private float _defaultCapsuleRadius = 0; // 기본 캡슐 반지름

        private void Awake()
        {
            _mover = GetComponent<IMover>();
            _capsule = GetComponent<ICapsule>();

            _defaultCapsuleRadius = _capsule.GetCapsuleRadius();
        }

        public override bool ReadyToRun()
        {
            // 캐릭터가 바닥에 있지 않으면 기어갈 준비가 안 됨
            if (!_mover.IsGrounded()) return false;

            // 'crawl' 액션이 활성화되거나 높이에 의해 기어가기가 강제되면 준비 완료
            if (_action.crawl || ForceCrawlByHeight())
                return true;

            return false;
        }

        public override void OnStartAbility()
        {
            // 모든 이동을 멈춤
            _mover.StopMovement();

            // 시스템에 기어가기 시작 중임을 알림
            _startingCrawl = true;

            // 기어가기 애니메이션을 설정
            SetAnimationState(startCrawlAnimationState);

            // 캡슐 콜라이더의 크기를 조절  
            _capsule.SetCapsuleSize(capsuleHeightOnCrawl, _capsule.GetCapsuleRadius());
        }

        public override void UpdateAbility()
        {
            // 기어가기 시작 애니메이션을 기다림
            if (_startingCrawl)
            {
                if (_animator.IsInTransition(0)) return; // 애니메이션의 0번 레이어가 상태를 전환중이면 return시킴

                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(startCrawlAnimationState))
                    _startingCrawl = false;

                return;
            }

            // 기어가기를 멈추는 애니메이션을 기다림
            if (_stoppingCrawl)
            {
                if (_animator.IsInTransition(0)) return;

                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.85f)
                    StopAbility();

                return;
            }

            // 캐릭터를 기어가게 이동시킴
            _mover.Move(_action.move, crawlSpeed);

            // 'crawl' 액션이 다시 활성화되면 기어가기를 멈춤
            if (_action.crawl && !ForceCrawlByHeight())
            {
                SetAnimationState(stopCrawlAnimationState);
                _stoppingCrawl = true;
                _mover.StopMovement();
            }

            if (_action.crouch)
            {
                // Crouch 상태로 전환을 위한 메소드 호출
                StartCrouchFromCrawl();
                return; // 추가 로직을 방지하기 위해 리턴
            }

            if (_action.jump)
            {
                // Crouch 상태로 전환을 위한 메소드 호출
                StartCrouchFromCrawl();
                return; // 추가 로직을 방지하기 위해 리턴
            }
        }

        public override void OnStopAbility()
        {
            // 제어 변수를 리셋
            _startingCrawl = false;
            _stoppingCrawl = false;

            // 캡슐 크기를 원래대로 복구
            _capsule.ResetCapsuleSize();
        }

        private bool ForceCrawlByHeight()
        {
            RaycastHit hit;

            // SphereCast를 사용하여 장애물이 있는지 확인
            if (Physics.SphereCast(transform.position, _defaultCapsuleRadius, Vector3.up, out hit,
                MaxHeightToStartCrawl, obstaclesMask, QueryTriggerInteraction.Ignore))
            {
                // 충돌 지점이 기어갈 때의 캡슐 높이보다 높으면 true 반환
                if (hit.point.y - transform.position.y > capsuleHeightOnCrawl)
                    return true;
            }

            return false;
        }
        private void StartCrouchFromCrawl()
        {
            // Crouch 상태로의 전환 로직 구현
            SetAnimationState(stopCrawlAnimationState); // 기어가기를 멈추는 애니메이션
            _stoppingCrawl = true; // 기어가기 중단 상태로 설정
            _mover.StopMovement(); // 캐릭터의 이동 중단

            // 바뀌고 싶은 액션을 시작하는 부분이 들어가면 됨
            // 예를 들어, Crouch 상태로 직접 전환하거나,
            // Crouch 상태를 관리하는 별도의 메커니즘을 트리거할 수 있음
            // Crawl에서 Crouch로 가는 마땅한 애니메이션이 없어서 그냥 일어서게 만듬
        }
    }
}
