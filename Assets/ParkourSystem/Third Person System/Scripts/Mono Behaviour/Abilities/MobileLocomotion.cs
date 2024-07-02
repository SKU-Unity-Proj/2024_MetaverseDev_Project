using UnityEngine;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public enum MobileMovementStyle
    {
        HoldToWalk, HoldToRun, HoldToSprint, DoNothing
    }

    public class MobileLocomotion : AbstractAbility
    {
        [SerializeField] private float walkSpeed = 1.0f;
        [SerializeField] private float runSpeed = 3.0f;
        [SerializeField] private float sprintSpeed = 5.3f;
        [Tooltip("Determine how to use extra key button to handle movement. If shift is hold, tells system if it should walk, run, or do nothing")]
        [SerializeField] private MobileMovementStyle movementByKey = MobileMovementStyle.HoldToWalk;
        [SerializeField] private string groundedAnimBlendState = "Grounded";

        private IMover _mover = null;
        private int _animIDSpeed;

        private void Awake()
        {
            _mover = GetComponent<IMover>();

            _animIDSpeed = Animator.StringToHash("Speed");
        }

        public override bool ReadyToRun()
        {
            return _mover.IsGrounded();
        }

        public override void OnStartAbility()
        {
            SetAnimationState(groundedAnimBlendState, 0.25f);

            if (_action.move.magnitude < 0.1f)
            {
                // reset movement parameters
                _animator.SetFloat(_animIDSpeed, 0, 0, Time.deltaTime);
            }
        }

        public override void UpdateAbility()
        {
            float targetSpeed = 0;

            // 조이스틱 입력 강도에 따른 스프린트 처리
            float inputMagnitude = _action.move.magnitude;
            _action.sprint = inputMagnitude >= 1.0f; // 임계값 설정, 예: 0.99

            if (_action.sprint)
            {
                targetSpeed = sprintSpeed; // Use sprint speed if sprinting
            }
            else
            {
                // Otherwise, use the appropriate speed based on the movement style
                switch (movementByKey)
                {
                    case MobileMovementStyle.HoldToWalk:
                        targetSpeed = _action.walk ? walkSpeed : runSpeed;
                        break;
                    case MobileMovementStyle.HoldToRun:
                        targetSpeed = _action.walk ? runSpeed : walkSpeed;
                        break;
                    case MobileMovementStyle.HoldToSprint: // The HoldToSprint case is now redundant, you can remove it if you don't need it for other purposes.
                        targetSpeed = _action.sprint ? sprintSpeed : runSpeed;
                        break;
                    case MobileMovementStyle.DoNothing:
                        targetSpeed = runSpeed;
                        break;
                }
            }

            // Continue with moving the character
            _mover.Move(_action.move, targetSpeed);
        }

    }
}