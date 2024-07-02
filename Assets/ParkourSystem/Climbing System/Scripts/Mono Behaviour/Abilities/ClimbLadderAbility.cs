using UnityEngine;
using DiasGames.Components;
using DiasGames.Climbing;

namespace DiasGames.Abilities
{
    public class ClimbLadderAbility : AbstractAbility
    {
        [Header("Overlap")]
        [SerializeField] private LayerMask ladderMask; // 사다리와 겹치는지 감지하기 위한 레이어 마스크
        [SerializeField] private Transform grabReference; // 사다리를 잡을 위치
        [SerializeField] private float overlapRange = 1f; // 사다리 감지 범위
        [Header("Animation")]
        [SerializeField] private string climbLadderAnimState = "Ladder"; // 사다리 타는 애니메이션 상태
        [SerializeField] private string climbUpAnimState = "Climb.Climb up"; // 사다리를 올라가는 애니메이션 상태
        [SerializeField] private string ladderAnimFloat = "Vertical"; // 사다리 타는 애니메이션에 전달되는 수직 입력
        [Header("Movement")]
        [SerializeField] private float climbSpeed = 1.2f; // 사다리를 타는 속도
        [SerializeField] private float charOffset = 0.3f; // 캐릭터와 사다리 사이의 거리
        [SerializeField] private float smoothnessTime = 0.12f; // 사다리에 부드러운 이동을 위한 시간

        private IMover _mover;
        private ICapsule _capsule;

        private Ladder _currentLadder;
        private Ladder _blockedLadder;
        private float _blockedTime;

        private bool _stopDown;
        private bool _stopUp;
        private bool _climbingUp;

        // 사다리에 부드러운 위치와 회전을 설정하기 위한 매개 변수
        private Vector3 _startPosition, _targetPosition;
        private Quaternion _startRotation, _targetRotation;
        private float _step;
        private float _weight;

        private void Awake()
        {
            _mover = GetComponent<IMover>();
            _capsule = GetComponent<ICapsule>();
        }

        public override bool ReadyToRun()
        {
            return !_mover.IsGrounded() && FoundLadder(); // 땅에 있지 않고 사다리를 찾은 경우
        }

        public override void OnStartAbility()
        {
            _animator.CrossFadeInFixedTime(climbLadderAnimState, 0.1f); // 사다리 타는 애니메이션 재생
            _mover.DisableGravity(); // 중력 비활성화

            // 위치 설정
            _weight = 0;
            _step = 1 / smoothnessTime;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _targetPosition = GetCharPosition();
            _targetRotation = GetCharRotation();

            // 제어 변수
            _stopDown = false;
            _stopUp = false;
            _climbingUp = false;
        }

        public override void UpdateAbility()
        {
            // 능력이 시작되고 캐릭터의 위치와 회전을 설정해야 하는 경우
            if (!Mathf.Approximately(_weight, 1f))
            {
                UpdatePositionOnLadder();
                return;
            }

            // 사다리를 올라가는 동안 상단에 도달하면 애니메이션 종료 대기
            if (_climbingUp)
            {
                if (_animator.IsInTransition(0)) return;

                var state = _animator.GetCurrentAnimatorStateInfo(0);
                var normalizedTime = Mathf.Repeat(state.normalizedTime, 1f);
                if (normalizedTime > 0.95f)
                    StopAbility();

                return;
            }

            // 수직 입력
            float vertical = _action.move.y;

            // 수직 한계 확인
            CheckVerticalLimits();

            // 아래쪽 한계에 도달하면 아래쪽으로 이동 중지
            if (_stopDown && vertical < 0) vertical = 0;

            // 상단 한계에 도달한 경우
            if (_stopUp && vertical > 0)
            {
                // 사다리의 맨 위로 올라갈 수 있는 경우
                if (_currentLadder.CanClimbTop)
                {
                    _animator.CrossFadeInFixedTime(climbUpAnimState, 0.1f); // 올라가는 애니메이션 재생
                    _mover.ApplyRootMotion(Vector3.one);
                    _capsule.DisableCollision(); // 콜라이더 충돌 비활성화
                    _climbingUp = true;
                    return;
                }

                // 사다리 맨 위에 도달한 후 올라가는 것을 중지
                vertical = 0;
            }

            // 사다리 타는 애니메이션에 수직 입력 설정
            _animator.SetFloat(ladderAnimFloat, vertical, 0.1f, Time.deltaTime);
            // 캐릭터를 위아래로 이동
            _mover.Move(Vector3.up * vertical * climbSpeed);

            // 아래로 이동 중이고 땅에 도달한 경우 능력 종료
            if (_action.move.y < 0f && _mover.IsGrounded())
                StopAbility();

            // 사다리에서 떨어지기
            if (_action.drop)
            {
                StopAbility();
                BlockLadder(); // 사다리 블록
            }
        }

        public override void OnStopAbility()
        {
            _mover.EnableGravity(); // 중력 활성화
            _capsule.EnableCollision(); // 콜라이더 충돌 활성화
            _mover.StopRootMotion(); // 루트 모션 중지
        }

        /// <summary>
        /// 부드러운 전환을 사용하여 위치와 회전 설정
        /// </summary>
        private void UpdatePositionOnLadder()
        {
            _weight = Mathf.MoveTowards(_weight, 1f, _step * Time.deltaTime);
            _mover.SetPosition(Vector3.Lerp(_startPosition, _targetPosition, _weight));
            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _weight);
        }

        /// <summary>
        /// 사다리에서 떨어질 때 현재 사다리를 블록합니다.
        /// </summary>
        private void BlockLadder()
        {
            _blockedLadder = _currentLadder;
            _blockedTime = Time.time;
        }

        /// <summary>
        /// 사다리의 수직 한계를 확인하여 사다리를 넘어 올라가거나 내려가는 것을 방지합니다.
        /// </summary>
        private void CheckVerticalLimits()
        {
            if (transform.position.y < _currentLadder.BottomLimit.position.y)
                _stopDown = true;
            else if (transform.position.y > _currentLadder.BottomLimit.position.y + 0.15f)
                _stopDown = false;

            if (transform.position.y + _capsule.GetCapsuleHeight() > _currentLadder.TopLimit.position.y)
                _stopUp = true;
            else if (transform.position.y + _capsule.GetCapsuleHeight() < _currentLadder.TopLimit.position.y - 0.15f)
                _stopUp = false;
        }

        /// <summary>
        /// 사다리를 찾습니다.
        /// </summary>
        private bool FoundLadder()
        {
            var overlaps = Physics.OverlapSphere(grabReference.position, overlapRange, ladderMask, QueryTriggerInteraction.Collide);

            // 모든 충돌체 확인
            foreach (var coll in overlaps)
            {
                if (coll.TryGetComponent(out Ladder ladder))
                {
                    if (ladder == _blockedLadder && Time.time - _blockedTime < 2f)
                        continue;

                    if (CanGrab(ladder))
                    {
                        _currentLadder = ladder;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 캐릭터가 현재 위치와 회전에서 사다리를 잡을 수 있는지 확인합니다.
        /// </summary>
        private bool CanGrab(Ladder ladder)
        {
            // 캐릭터가 사다리를 바라보고 있는지 확인
            if (Vector3.Dot(transform.forward, -ladder.PositionAndDirection.forward) < -0.1f) return false;

            // 하단 확인
            if (transform.position.y < ladder.BottomLimit.position.y - 0.15f) return false;

            // 상단 확인
            if (transform.position.y + _capsule.GetCapsuleHeight() > ladder.TopLimit.position.y + 0.15f) return false;

            return true;
        }

        /// <summary>
        /// 이 사다리에서 캐릭터의 목표 위치를 가져옵니다.
        /// </summary>
        private Vector3 GetCharPosition()
        {
            Vector3 position = _currentLadder.PositionAndDirection.position + _currentLadder.PositionAndDirection.forward * charOffset;
            position.y = transform.position.y;

            if (position.y < _currentLadder.BottomLimit.position.y)
                position.y = _currentLadder.BottomLimit.position.y;

            float height = _capsule.GetCapsuleHeight();
            if (position.y + height > _currentLadder.TopLimit.position.y)
                position.y = _currentLadder.TopLimit.position.y - height;

            return position;
        }

        /// <summary>
        /// 이 사다리에서 캐릭터의 목표 회전을 가져옵니다.
        /// </summary>
        private Quaternion GetCharRotation()
        {
            return Quaternion.LookRotation(-_currentLadder.PositionAndDirection.forward);
        }
    }
}