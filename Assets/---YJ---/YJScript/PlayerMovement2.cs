using Fusion;
using UnityEngine;

namespace _01_04_Network_Properties
{
    public class PlayerMovement2 : NetworkBehaviour
    {
        private Vector3 _velocity;
        private bool _jumpPressed;
        public bool _isRunning;

        private CharacterController _controller;
        public Animator _animator;

        public Camera Camera;

        public float PlayerSpeed = 2f;
        public float RunSpeed = 4f;
        public float JumpForce = 5f;
        public float GravityValue = -9.81f;

        [Networked]
        public float _speed { get; set; }
        private float _interactDistance = 3f;
        public float _smoothSpeed = 20f;

        private Ride _currentRide;
        private Transform _originalParent;
        private Transform _seatPosition;

        private Quaternion _originalRotation;

        public LayerMask interactLayer;

        private int _originalLayer;
        private bool _originalActiveState;
        private bool _isSeated = false;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _originalParent = transform.parent;
            _originalRotation = transform.rotation;
            _originalLayer = gameObject.layer;
            _originalActiveState = gameObject.activeSelf;
        }

        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                _jumpPressed = true;
            }
            _isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_isSeated)
                {
                    ExitRide();
                }
                else
                {
                    TryInteractWithRide();
                }
            }
        }

        void FixedUpdate()
        {
            if (_isSeated && _seatPosition != null)
            {
                transform.position = Vector3.Lerp(transform.position, _seatPosition.position, _smoothSpeed * Time.fixedDeltaTime);
                //transform.rotation = _seatPosition.rotation;
            }
        }

        void TryInteractWithRide()
        {
            Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
            RaycastHit hit;

            Debug.DrawRay(Camera.transform.position, Camera.transform.forward * _interactDistance, Color.red, 2f);

            if (Physics.Raycast(ray, out hit, _interactDistance, interactLayer))
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
                Ride ride = hit.collider.GetComponent<Ride>();
                if (ride != null)
                {
                    Debug.Log("Interacting with Ride: " + ride.name);

                    _seatPosition = ride.seatPosition;

                    transform.position = _seatPosition.position;
                    transform.rotation = _seatPosition.rotation;

                    _isSeated = true;
                    _currentRide = ride;
                    Debug.Log("Player moved to seat position.");

                    // 레이어와 활성 상태 유지
                    gameObject.layer = _originalLayer;
                    gameObject.SetActive(_originalActiveState);
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any objects.");
            }
        }

        void ExitRide()
        {
            _seatPosition = null;

            transform.position += transform.forward * 2f;

            transform.rotation = _originalRotation;

            _isSeated = false;
            _currentRide = null;
            Debug.Log("Player exited the ride.");

            // 레이어와 활성 상태 복원
            gameObject.layer = _originalLayer;
            gameObject.SetActive(_originalActiveState);
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Camera = Camera.main;
                Camera.GetComponent<FirstPersonCamera>().Target = transform;
            }

            transform.position = _originalParent != null ? _originalParent.position : transform.position;
            transform.rotation = _originalParent != null ? _originalParent.rotation : transform.rotation;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false || _isSeated)
            {
                return;
            }

            if (_controller.isGrounded)
            {
                _velocity = new Vector3(0, -1, 0);
            }

            Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
            float currentSpeed = _isRunning ? RunSpeed : PlayerSpeed;
            Vector3 move = cameraRotationY * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * currentSpeed;

            _velocity.y += GravityValue * Runner.DeltaTime;
            if (_jumpPressed && _controller.isGrounded)
            {
                _velocity.y += JumpForce;
            }
            _controller.Move(move + _velocity * Runner.DeltaTime);

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
            _animator.SetFloat("Speed", _speed);
            _animator.SetBool("isRunning", _isRunning);

            if (_controller.isGrounded)
            {
                _animator.SetBool("isJump", false);
            }
            if (_jumpPressed && _controller.isGrounded)
            {
                _animator.SetBool("isJump", true);
            }
        }
    }
}
