using DiasGames.Abilities;
using System.Collections.Generic;
using DiasGames.Components;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Playables;
using Cinemachine;

namespace DiasGames.Abilities
{
    public class RaiseAbility : AbstractAbility
    {
        [Header("Animation")]
        [SerializeField] private string RaiseAnimationState = "Raise";
        [SerializeField] private string RaiseUpAnimationState = "Raise Up";

        [SerializeField] private GameObject originGiant = null;  // 헤드 트렌스폼
        [SerializeField] private GameObject setGiant = null;  // 헤드 트렌스폼

        private List<Collider> _raiseObjs = new List<Collider>();

        private IMover _mover = null;

        private int _raiseCount = 0;
        private bool _canRaise = true; // Raise를 실행할 수 있는지 여부를 나타내는 변수

        public UnityEvent onRaiseStartSecondTime; // 2번 이하 실행시
        public UnityEvent onRaiseStartThirdTime;  // 그 이상 실행시

        private bool _startingRaise = false;
        private bool _stoppingRaise = false;

        private float _tapCountdown = 1f;  // 연타 감지 시간
        private bool _shouldContinue = false;  // 연타 감지를 위한 변수
        private int _interactCount = 0;  // interact 입력 카운터

        //타임라인
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private CinemachineVirtualCamera timelineCam;

        private void Awake()
        {
            _mover = GetComponent<IMover>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("raiseCol"))
            {
                _raiseObjs.Add(other);
                Debug.Log(other.name);
                Debug.Log(_raiseObjs.Count);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_raiseObjs.Contains(other))
            {
                _raiseObjs.Remove(other);
                Debug.Log(_raiseObjs.Count);
            }
        }

        public override bool ReadyToRun()
        {
            return _action.interact && _mover.IsGrounded() && _raiseObjs.Count > 0 && _canRaise;
        }

        public override void OnStartAbility()
        {
            if (_raiseObjs.Count > 0)
            {
                _mover.StopMovement();  // 이동 중지

                this.transform.rotation = Quaternion.LookRotation(Vector3.forward);

                _canRaise = false; // 추가 실행 방지
                _startingRaise = true;
                _stoppingRaise = false;

                _raiseCount++;
                if (_raiseCount <= 2)
                {
                    SetAnimationState(RaiseAnimationState);
                }
                else
                {
                    SetAnimationState(RaiseUpAnimationState);
                }

                StartCoroutine(WaitBeforeStoppingRaise(_tapCountdown));
            }
        }

        public override void UpdateAbility()
        {
            if ((_animator.GetCurrentAnimatorStateInfo(0).IsName(RaiseAnimationState) ||
                 _animator.GetCurrentAnimatorStateInfo(0).IsName(RaiseUpAnimationState)) &&
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f &&
                !_animator.IsInTransition(0))
            {
                _stoppingRaise = true;
                _startingRaise = false;

                StopAbility();  // 어빌리티 종료

            }

            if (_shouldContinue && _action.interact)
            {
                _shouldContinue = false;  // 다음 연타 대기
                StartCoroutine(WaitBeforeNextRaise(6.8f));
            }
            else if (!_action.interact)
            {
                _shouldContinue = true;  // 연타 대기 리셋
            }

            if (_animator.speed == 0 && _action.interact)
            {
                if (_interactCount < 4)  // 첫 번째에서 네 번째 클릭까지는 카운트만 증가
                {
                    _interactCount++;

                    float frameIncrement = 0.01f; // 에니메이션의 normalizedTime을 증가시킬 값 (2~3프레임 정도 진행)
                    float newTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime + frameIncrement;
                    _animator.Play(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, newTime);

                }
                else if (_interactCount == 4 && _raiseCount <= 2)  // 다섯 번째 입력에서
                {
                    _animator.speed = 1;  // 에니메이션 재개
                    onRaiseStartSecondTime.Invoke();  // 2번 이하 실행 이벤트

                    _interactCount = 0;   // 카운트 초기화

                   
                }
                else 
                {
                    _animator.speed = 1;  // 에니메이션 재개
                    onRaiseStartThirdTime.Invoke();    // 그 이상 실행 이벤트

                    originGiant.SetActive(false);
                    setGiant.SetActive(true);

                    timelineCam.Priority = 12;
                    playableDirector.Play();

                    _raiseCount = 0;  // 카운트 리셋
                    _interactCount = 0;   // 카운트 초기화

                }
            }

        }

        public override void OnStopAbility()
        {
            if (_stoppingRaise)
            {
                _stoppingRaise = false;
            }
        }

        private IEnumerator WaitBeforeStoppingRaise(float delay)
        {
            yield return new WaitForSeconds(delay);
            _animator.speed = 0;  // 에니메이터 속도를 0으로 설정하여 에니메이션 일시 정지
            _interactCount = 0;   // 입력 카운트를 초기화
        }


        private IEnumerator WaitBeforeNextRaise(float delay)
        {
            yield return new WaitForSeconds(delay);
            _canRaise = true;  // 다시 실행 가능
        }
    }
}
