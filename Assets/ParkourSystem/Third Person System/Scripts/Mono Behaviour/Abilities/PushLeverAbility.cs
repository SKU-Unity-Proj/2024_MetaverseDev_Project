using System.Collections.Generic;
using UnityEngine;
using DiasGames.Components;
using DiasGames.IK;
using System.Collections;
using UnityEngine.Playables;
using Cinemachine;

namespace DiasGames.Abilities
{
    public class PushLeverAbility : AbstractAbility
    {
        [SerializeField] private float positionSmoothnessTime = 0.12f;
        [Header("Animation")]
        [SerializeField] private string pushLeverAnimationState = "Push Lever";

        [SerializeField] private Transform leverHandle;
        [SerializeField] private Transform rightIK = null;
        [SerializeField] private Transform leftIK = null;
        [SerializeField] private Transform rightIKTarget = null; // 애니메이션 대상의 오른쪽 타겟
        [SerializeField] private Transform leftIKTarget = null;  // 애니메이션 대상의 왼쪽 타겟

        [SerializeField] private GameObject originGiant = null;  // 헤드 트렌스폼
        [SerializeField] private GameObject setGiant = null;  // 헤드 트렌스폼

        [SerializeField] private PlayableDirector playableDirector;

        [SerializeField]  private CinemachineVirtualCamera timelineCam;

        // components
        private IMover _mover;
        private IKScheduler _ikScheduler;

        // interaction components
        private IPushable _pushable;
        private List<Collider> _triggeredObjs = new List<Collider>();

        // private internal vars
        // to positioning
        private bool _isMatchingTarget;

        private void Awake()
        {
            _mover = GetComponent<IMover>();
            _ikScheduler = GetComponent<IKScheduler>();

        }


        private void OnTriggerEnter(Collider other)
        {
            if (IsAbilityRunning || Time.time - StopTime < 0.1f) return;

            _triggeredObjs.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsAbilityRunning || Time.time - StopTime < 0.1f) return;

            if (_triggeredObjs.Contains(other))
                _triggeredObjs.Remove(other);
        }

        private bool IsDraggable()
        {
            if (_triggeredObjs.Count == 0) return false;

            foreach (var trigger in _triggeredObjs)
            {
                if (trigger.TryGetComponent(out _pushable))
                    return true;
            }

            return false;
        }

        public override void OnStartAbility()
        {
            _pushable.StartPush();
            _mover.StopMovement();

            SetAnimationState(pushLeverAnimationState);

            _isMatchingTarget = true;

            StartCoroutine(WaitForAnimation()); 
        }

        public override bool ReadyToRun()
        {
            if (Time.time - StopTime < 0.1f) return false;

            return _action.interact && IsDraggable();
        }

        public override void UpdateAbility()
        {
            HandleIK();
            UpdateTransform();

            if (_isMatchingTarget) return;

            if (_action.interact)
                StopAbility();

            StartCoroutine(LeverHandleDelay(0.1f)); // 0.1초 지연

        }

        private IEnumerator WaitForAnimation()
        {
            yield return new WaitForSeconds(2.5f);

            timelineCam.Priority = 12;

            playableDirector.Play();
            
            originGiant.SetActive(false);
            setGiant.SetActive(true);


        }

        // 지연 후에 레버 핸들을 IK 타겟에 연결하는 Coroutine입니다.
        private IEnumerator LeverHandleDelay(float delay)
        {
            // 지연 시간만큼 대기합니다.
            yield return new WaitForSeconds(delay);

            // 지연 후에 실행할 코드
            rightIK.position = rightIKTarget.position;
            leftIK.position = leftIKTarget.position;

            // 레버 핸들의 위치를 중간점으로 설정합니다.
            leverHandle.position = CalculateMidpointOfHands();
        }

        // 플레이어의 양손 IK 타겟 위치의 중간점 계산
        private Vector3 CalculateMidpointOfHands()
        {
            Vector3 midpoint = (leftIK.position + rightIK.position) / 2;
            return midpoint;
        }

        public override void OnStopAbility()
        {
            _pushable.StopPush();

            // reset vars
            _isMatchingTarget = false;

            // stop IK
            if (_ikScheduler != null)
            {
                _ikScheduler.StopIK(AvatarIKGoal.LeftHand);
                _ikScheduler.StopIK(AvatarIKGoal.RightHand);
            }
        }


        private void HandleIK()
        {
            if (_pushable != null && _ikScheduler != null)
            {
                // left hand
                Transform lhEffector = _pushable.GetLeftHandTarget();
                if (lhEffector != null)
                {
                    IKPass leftHandPass = new IKPass(lhEffector.position,
                        lhEffector.rotation,
                        AvatarIKGoal.LeftHand,
                        1, 1);

                    _ikScheduler.ApplyIK(leftHandPass);
                }

                // right hand
                Transform rhEffector = _pushable.GetRightHandTarget();
                if (rhEffector != null)
                {
                    IKPass rightHandPass = new IKPass(rhEffector.position,
                        rhEffector.rotation,
                        AvatarIKGoal.RightHand,
                        1, 1);

                    _ikScheduler.ApplyIK(rightHandPass);
                }

            }
        }
        private void UpdateTransform()
        {
            if (!_isMatchingTarget || _pushable == null) return;

            Vector3 targetPos = _pushable.GetTarget().position;
            targetPos.y = transform.position.y;

           // _mover.SetPosition(Vector3.MoveTowards(transform.position, targetPos, _step * Time.deltaTime));
            transform.rotation = Quaternion.Lerp(transform.rotation, _pushable.GetTarget().rotation, positionSmoothnessTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                _isMatchingTarget = false;
                //_mover.SetPosition(targetPos);
                transform.rotation = _pushable.GetTarget().rotation;
            }
        }
    }
}