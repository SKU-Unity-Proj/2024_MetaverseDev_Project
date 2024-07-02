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
    public class PullselfAbility : AbstractAbility
    {
        [Header("Animation")]
        [SerializeField] private string PullselfAnimationState = "Pullself";

        private List<Collider> _pullObjs = new List<Collider>();

        private IMover _mover = null;

        private bool _canPull = true; // Raise를 실행할 수 있는지 여부를 나타내는 변수

        public Transform pullPos;

        public GameObject giantCol;

        public UnityEvent onFallDownEvent; // 2번 이하 실행시

        private bool _startingPull = false;
        private bool _stoppingPull = false;

        private float _tapCountdown = 0.5f;  // 연타 감지 시간
        private bool _shouldContinue = false;  // 연타 감지를 위한 변수
        private int _interactCount = 0;  // interact 입력 카운터


        private void Awake()
        {
            _mover = GetComponent<IMover>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("pullCol"))
            {
                _pullObjs.Add(other);
                Debug.Log(other.name);
                Debug.Log(_pullObjs.Count);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_pullObjs.Contains(other))
            {
                _pullObjs.Remove(other);
                Debug.Log(_pullObjs.Count);
            }
        }

        public override bool ReadyToRun()
        {
            return _action.interact && _mover.IsGrounded() && _pullObjs.Count > 0 && _canPull;
        }

        public override void OnStartAbility()
        {
            if (_pullObjs.Count > 0)
            {
                this.transform.position = pullPos.position;
                this.transform.rotation = Quaternion.LookRotation(-Vector3.right);

                _mover.StopMovement();  // 이동 중지

                _canPull = false; // 추가 실행 방지

                _startingPull = true;
                _stoppingPull = false;

                SetAnimationState(PullselfAnimationState);

                StartCoroutine(WaitBeforeStoppingRaise(_tapCountdown));
            }
        }

        public override void UpdateAbility()
        {
            if ((_animator.GetCurrentAnimatorStateInfo(0).IsName(PullselfAnimationState) &&
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f &&
                !_animator.IsInTransition(0)))
            {
                _stoppingPull = true;
                _startingPull = false;

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

                    if(_interactCount == 2)
                    {
                        onFallDownEvent.Invoke();
                        giantCol.SetActive(true);
                    }
                        
                }

                else
                {
                    _animator.speed = 1;  // 에니메이션 재개

                    _interactCount = 0;   // 카운트 초기화

                }
            }

        }

        public override void OnStopAbility()
        {
            if (_stoppingPull)
            {
                _stoppingPull = false;
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
            _canPull = true;  // 다시 실행 가능
        }
    }
}
