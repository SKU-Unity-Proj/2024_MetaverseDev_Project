using UnityEngine;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public class Strafe : AbstractAbility
    {
        [SerializeField] private float strafeWalkSpeed = 2f;

        [Header("Animation")] // Animation 섹션 구분
        [SerializeField] private string strafeAnimState = "Strafe";
        [SerializeField] private string horizontalAnimFloat = "Horizontal";
        [SerializeField] private string verticalAnimFloat = "Vertical";

        private IMover _mover = null;
        private GameObject _camera = null;

        private int _animHorizontalID; // _animHorizontalID 변수 선언
        private int _animVerticalID; // _animVerticalID 변수 선언

        private void Awake()
        {
            _mover = GetComponent<IMover>();
            _camera = Camera.main.gameObject;

            _animHorizontalID = Animator.StringToHash(horizontalAnimFloat);
            _animVerticalID = Animator.StringToHash(verticalAnimFloat);
        }


        public override bool ReadyToRun()
        {
            return _mover.IsGrounded() && _action.zoom;
        }

        public override void OnStartAbility()
        {
            SetAnimationState(strafeAnimState);
        }

        public override void UpdateAbility()
        {
            _mover.Move(_action.move, strafeWalkSpeed, false);
            //transform.rotation = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0); // 카메라 방향으로 회전

            // 애니메이터 업데이트
            _animator.SetFloat(_animHorizontalID, _action.move.x, 0.1f, Time.deltaTime); // 가로 움직임 값 전달
            _animator.SetFloat(_animVerticalID, _action.move.y, 0.1f, Time.deltaTime); // 세로 움직임 값 전달

            if (!_action.zoom || !_mover.IsGrounded()) // 줌이 아니거나 땅에 붙어있지 않으면 능력 중지
                StopAbility();
        }
    }
}