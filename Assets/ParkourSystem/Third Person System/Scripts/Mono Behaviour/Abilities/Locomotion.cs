using UnityEngine;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public enum MovementStyle
    {
        HoldToWalk, HoldToRun, HoldToSprint, DoNothing
    }

    public class Locomotion : AbstractAbility
    {
        [SerializeField] private float walkSpeed = 1.0f;
        [SerializeField] private float runSpeed = 3.0f;
        [SerializeField] private float sprintSpeed = 5.3f;
        [Tooltip("Determine how to use extra key button to handle movement. If shift is hold, tells system if it should walk, run, or do nothing")]
        [SerializeField] private MovementStyle movementByKey = MovementStyle.HoldToWalk;
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
            // Check if the action to sprint is activated
            if (_action.sprint)
            {
                targetSpeed = sprintSpeed; // Use sprint speed if sprinting
            }
            else
            {
                // Otherwise, use the appropriate speed based on the movement style
                switch (movementByKey)
                {
                    case MovementStyle.HoldToWalk:
                        targetSpeed = _action.walk ? walkSpeed : runSpeed;
                        break;
                    case MovementStyle.HoldToRun:
                        targetSpeed = _action.walk ? runSpeed : walkSpeed;
                        break;
                    case MovementStyle.HoldToSprint: // The HoldToSprint case is now redundant, you can remove it if you don't need it for other purposes.
                        targetSpeed = _action.sprint ? sprintSpeed : runSpeed;
                        break;
                    case MovementStyle.DoNothing:
                        targetSpeed = runSpeed;
                        break;
                }
            }

            // Continue with moving the character
            _mover.Move(_action.move, targetSpeed);
        }

    }
}