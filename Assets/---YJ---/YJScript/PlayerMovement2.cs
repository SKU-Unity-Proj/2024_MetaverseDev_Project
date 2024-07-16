
using Fusion;
using UnityEngine;

namespace _01_04_Network_Properties
{
    /// <summary>
    /// https://doc-api.photonengine.com/en/fusion/v2/class_fusion_1_1_network_behaviour.html
    /// NetworkBehaviour.FixedUpdateNetwork�� ����Ѵ�.
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
        /// ��Ʈ��ũ ������Ʈ�� ���� ������ ������ �� ����Ǵ� �Լ�
        /// </summary>
        public override void Spawned()
        {
            // �÷��̾� �����̸� ���� �ִ� ���� ī�޶��� Ÿ���� �ڽ����� ����
            if (HasStateAuthority)
            {
                Camera = Camera.main;
                Camera.GetComponent<FirstPersonCamera>().Target = transform;
            }
        }

        /// <summary>
        /// ���� ��Ʈ��ũ ������ �ǽð����� ����ȭ�Ǵ� Update �޼ҵ�
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            // �ڽ��� �÷��̾����� üũ
            // ��� �ٸ� �÷��̾ �ƴ� �ڽ��� �÷��̾ �̵�. �� �÷��̾�� �ڽ��� �÷��̾� ��ü�� �����Ѵ�.
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

