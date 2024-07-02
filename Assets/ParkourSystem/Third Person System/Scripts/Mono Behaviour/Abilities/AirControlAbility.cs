using UnityEngine;
using UnityEngine.Events;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public class AirControlAbility : AbstractAbility
    {
        // 애니메이션 상태
        [Header("Animation State")]
        [SerializeField] private string animJumpState = "Air.Jump"; // 점프 애니메이션 상태
        [SerializeField] private string animFallState = "Air.Falling"; // 낙하 애니메이션 상태
        [SerializeField] private string animHardLandState = "Air.Hard Land"; // 강하게 착지하는 애니메이션 상태

        // 점프 매개변수
        [Header("Jump parameters")]
        public float jumpHeight = 1.2f; // 점프 높이
        [SerializeField] private float speedOnAir = 6f; // 공중에서의 속도
        [SerializeField] private float airControl = 0.5f; // 공중 제어

        // 착지
        [Header("Landing")]
        [SerializeField] private float heightForHardLand = 3f; // 강하게 착지하기 위한 높이
        [SerializeField] private float heightForKillOnLand = 7f; // 착지로 인한 사망 높이

        // 사운드 효과
        [Header("Sound FX")]
        [SerializeField] private Transform voicePos; 
        //[SerializeField] SoundList jumpFootSound, jumpVoiceSound, landSound;

        // 이벤트
        [Header("Event")]
        [SerializeField] private UnityEvent OnLanded = null; // 착지 이벤트

        private IMover _mover = null;
        private IDamage _damage;

        private float _startSpeed;
        private Vector2 _startInput;

        private Vector2 _inputVel;
        private float _angleVel;

        private float _targetRotation;
        private Transform _camera;

        // 착지 제어 변수
        private float _highestPosition = 0;
        private bool _hardLanding = false;

        private void Awake()
        {
            _mover = GetComponent<IMover>(); // 이동자 컴포넌트 가져오기
            _damage = GetComponent<IDamage>(); // 데미지 컴포넌트 가져오기
            _camera = Camera.main.transform; // 메인 카메라의 트랜스폼 가져오기
        }

        public override bool ReadyToRun()
        {
            return !_mover.IsGrounded() || _action.jump; // 땅에 있지 않거나 점프 액션이 발동되었을 때 실행 준비 확인
        }

        public override void OnStartAbility()
        {
            _startInput = _action.move; // 초기 입력값 설정
            _targetRotation = _camera.eulerAngles.y; // 타겟 회전값 설정

            if (_action.jump && _mover.IsGrounded()) // 점프 액션이 발동되고 땅에 있을 때
                PerformJump(); // 점프 수행
            else
            {
                SetAnimationState(animFallState, 0.25f); // 낙하 애니메이션 상태로 설정
                _startSpeed = Vector3.Scale(_mover.GetVelocity(), new Vector3(1, 0, 1)).magnitude; // 시작 속도 설정

                _startInput.x = Vector3.Dot(_camera.right, transform.forward); // 카메라 오른쪽과 전방 벡터의 내적값 설정
                _startInput.y = Vector3.Dot(Vector3.Scale(_camera.forward, new Vector3(1, 0, 1)), transform.forward); // 카메라 전방 벡터와 전방 벡터의 내적값 설정

                if (_startSpeed > 3.5f)
                    _startSpeed = speedOnAir; // 시작 속도가 설정되어 있으면 공중에서의 속도로 설정
            }

            _highestPosition = transform.position.y; // 최고 높이 설정
            _hardLanding = false; // 강하게 착지 여부 설정
        }

        public override void UpdateAbility()
        {
            if (_hardLanding)
            {
                // 루트 모션 적용
                _mover.ApplyRootMotion(Vector3.one, false);

                // 애니메이션이 완료될 때까지 대기
                if (HasFinishedAnimation(animHardLandState))
                    StopAbility();

                return;
            }

            if (_mover.IsGrounded()) // 땅에 있을 때
            {
                if (_highestPosition - transform.position.y >= heightForHardLand) // 최고 높이에서 강하게 착지하는 경우
                {
                    _hardLanding = true; // 강하게 착지 상태로 변경
                    SetAnimationState(animHardLandState, 0.02f); // 강하게 착지 애니메이션 상태로 변경

                    // 이벤트 호출
                    OnLanded.Invoke();

                    // 사운드 재생
                    //SoundManager.Instance.PlayOneShotEffect((int)landSound, transform.position, 0.2f);

                    // 데미지 발생
                    if (_damage != null)
                    {
                        // 데미지 계산
                        float currentHeight = _highestPosition - transform.position.y - heightForHardLand;
                        float ratio = currentHeight / (heightForKillOnLand - heightForHardLand);

                        _damage.Damage((int)(500 * ratio));
                    }

                    return;
                }

                StopAbility(); // 능력 중지
            }

            if (transform.position.y > _highestPosition) // 현재 높이가 최고 높이보다 높을 때
                _highestPosition = transform.position.y; // 최고 높이 업데이트

            _startInput = Vector2.SmoothDamp(_startInput, _action.move, ref _inputVel, airControl); // 입력값 스무딩
            _mover.Move(_startInput, _startSpeed, false); // 이동 수행

            RotateCharacter(); // 캐릭터 회전
        }

        private void RotateCharacter()
        {
            // 참고: Vector2의 != 연산자는 근사치를 사용하므로 부동 소수점 오차에 민감
            // 있지 않고 magnitude 연산보다 저렴함
            // 움직임 입력이 있는 경우 캐릭터를 회전
            if (_action.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(_startInput.x, _startInput.y) * Mathf.Rad2Deg + _camera.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _angleVel, airControl);

                // 입력 방향에 대해 카메라 위치를 기준으로 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        public override void OnStopAbility()
        {
            base.OnStopAbility(); // 부모 클래스의 능력 중지 메서드 호출

            // 땅에 있고 강하게 착지하지 않았으며 수직 속도가 -3f 이하인 경우
            if (_mover.IsGrounded() && !_hardLanding && _mover.GetVelocity().y < -3f)
                OnLanded.Invoke(); // 착지 이벤트 호출

            _hardLanding = false; // 강하게 착지 여부 초기화
            _highestPosition = 0; // 최고 높이 초기화
            _mover.StopRootMotion(); // 루트 모션 정지
        }

        /// <summary>
        /// 점프를 수행하여 리지드바디에 힘을 추가합니다.
        /// </summary>
        private void PerformJump()
        {
            Vector3 velocity = _mover.GetVelocity(); // 현재 속도 가져오기
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * _mover.GetGravity()); // 점프에 필요한 속도 계산

            _mover.SetVelocity(velocity); // 속도 설정
            _animator.CrossFadeInFixedTime(animJumpState, 0.0f); // 점프 애니메이션 재생
            _startSpeed = speedOnAir; // 시작 속도 설정

            if (_startInput.magnitude > 0.1f)
                _startInput.Normalize(); // 입력 정규화

            //Debug.Log("Playing foot sound");
            //SoundManager.Instance.PlayOneShotEffect((int)jumpFootSound, transform.position, 0.2f);

           // Debug.Log("Playing voice sound");
           // SoundManager.Instance.PlayOneShotEffect((int)jumpVoiceSound, voicePos.position, 2f);

        }

        private void HardLand()
        {

        }
    }
}