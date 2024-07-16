
using Fusion;
using UnityEngine;

namespace _01_04_Network_Properties
{
    /// <summary>
    /// https://doc-api.photonengine.com/en/fusion/v2/class_fusion_1_1_network_behaviour.html
    /// NetworkBehaviour.FixedUpdateNetwork를 사용한다.
    /// </summary>
    public class PlayerMovement2 : NetworkBehaviour
    {
        private Vector3 _velocity;
        private bool _jumpPressed;

        private CharacterController _controller;
        public Animator _animator;


        public Camera Camera;

        public float PlayerSpeed = 2f;

        public float JumpForce = 5f;
        public float GravityValue = -9.81f;

        [Networked]
        public float _speed { get; set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                _jumpPressed = true;
            }
        }

        /// <summary>
        /// 네트워크 오브젝트가 세션 내에서 생성될 때 실행되는 함수
        /// </summary>
        public override void Spawned()
        {
            // 플레이어 본인이면 씬에 있는 메인 카메라의 타겟을 자신으로 설정
            if (HasStateAuthority)
            {
                Camera = Camera.main;
                Camera.GetComponent<FirstPersonCamera>().Target = transform;
            }
        }

        /// <summary>
        /// 포톤 네트워크 서버에 실시간으로 동기화되는 Update 메소드
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            // 자신의 플레이어인지 체크
            // 모든 다른 플레이어가 아닌 자신의 플레이어만 이동. 각 플레이어는 자신의 플레이어 개체를 제어한다.
            if (HasStateAuthority == false)
            {
                return;
            }

            if (_controller.isGrounded)
            {
                _velocity = new Vector3(0, -1, 0);
            }

            Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
            Vector3 move = cameraRotationY * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

            _velocity.y += GravityValue * Runner.DeltaTime;
            if (_jumpPressed && _controller.isGrounded)
            {
                _velocity.y += JumpForce;
            }
            _controller.Move(move + _velocity * Runner.DeltaTime);
            //_controller.Move(move * Runner.DeltaTime + _velocity * Runner.DeltaTime);

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z);
            _speed = horizontalVelocity.magnitude;

            _jumpPressed = false;
        }

        public override void Render()
        {
            //var interpolator = new NetworkBehaviourBufferInterpolator(this);

            //_animator.SetFloat("Speed", interpolator.Float(nameof(_speed)));

            _animator.SetFloat("Speed", _speed);
        }
    }
}

