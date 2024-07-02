using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using DiasGames.Components;

namespace DiasGames.Controller
{
    public class MobilePlayerController : MonoBehaviour
    {
        public Transform player;
        public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
        public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

        public float smooth = 10f;  // ī�޶� �����ӵ�
        public float horizontalAimingSpeed = 6.0f; // ���� ȸ�� �ӵ�.
        public float verticalAimingSpeed = 6.0f; // ���� ȸ�� �ӵ�.
        public float maxVerticalAngle = 30.0f; // ī�޶� ���� �ִ� ����
        public float minverticalAngle = -60.0f; // ī�޶� ���� �ּ� ����
        public float recoilAngleBounce = 5.0f; // ��� �ݵ� ��

        public Transform cameraTransform; // Ʈ������ ĳ��.
        private Camera myCamera;
        private Vector3 relCameraPos; // �÷��̾�κ��� ī�޶������ ����.
        private float relCameraPosMag; // �÷��̾�κ��� ī�޶���� �Ÿ�.
        private Vector3 smoothPivotOffset; // ī�޶� �Ǻ� ������ ����. 
        private Vector3 smoothCamOffset; // ī�޶� ��ġ ������ ����. 
        private Vector3 targetPivotOffset; // ī�޶� �Ǻ� ������ ����. 
        private Vector3 targetCamOffset; // ī�޶� ��ġ ������ ����.
        private float defaultFOV; // �⺻ �þ߰�.
        private float targetFOV; // Ÿ�� �þ߰�.
        private float targetMaxVerticleAngle; // ī�޶� ���� �ִ� ����. -> ��ݽ� �ݵ�
        private float recoilAngle = 0f; // ��� �ݵ� ����


        // Components
        private AbilityScheduler _scheduler = null;
        private Health _health = null;
        private IMover _mover;
        private ICapsule _capsule;

        private const float _threshold = 0.01f;

        [SerializeField] private bool hideCursor = true;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;
        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;
        [Tooltip("Speed of camera turn")]
        public Vector2 CameraTurnSpeed = new Vector2(300.0f, 200.0f);
        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // for shooter ui
        public float CurrentRecoil { get; private set; } = 0f;
        private float recoilReturnVel = 0;

        private void Awake()
        {
            _scheduler = GetComponent<AbilityScheduler>();
            _health = GetComponent<Health>();
            _mover = GetComponent<IMover>();
            _capsule = GetComponent<ICapsule>();


            // set right angle on start for camera
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;

            //ĳ��

            myCamera = cameraTransform.GetComponent<Camera>();

            // ī�޶� �⺻ ������ ����
            cameraTransform.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
            cameraTransform.rotation = Quaternion.identity;

            // ī�޶�� �÷��̾�� ��� ����. -> �浹üũ�� �����
            relCameraPos = cameraTransform.position - player.position;
            relCameraPosMag = relCameraPos.magnitude - 0.5f; // �÷��̾ �� �Ÿ�.

            // �⺻����
            smoothPivotOffset = pivotOffset;
            smoothCamOffset = camOffset;
            defaultFOV = myCamera.fieldOfView;
            Look.x = player.eulerAngles.y; // �ʱ� y��.

            ResetTargetOffsets();
            ResetFOV();
            ResetMaxVerticalAngle();
        }

        #region CAMERA Setting
        //�ʱ�ȭ �Լ�
        public void ResetTargetOffsets()
        {
            targetPivotOffset = pivotOffset;
            targetCamOffset = camOffset;
        }
        public void ResetFOV()
        {
            this.targetFOV = defaultFOV;
        }
        public void ResetMaxVerticalAngle()
        {
            targetMaxVerticleAngle = maxVerticalAngle;
        }

        // �⺻ ���� ���� �Լ�
        public void BounceVertical(float degree)
        {
            recoilAngle = degree;
        }
        public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
        {
            targetPivotOffset = newPivotOffset;
            targetCamOffset = newCamOffset;
        }
        public void SetFOV(float customFOV)
        {
            this.targetFOV = customFOV;
        }

        // �浹üũ �Լ�
        bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight) // playerPos�� �߹��̹Ƿ� �÷��̾��� ���̸� üũ�ؼ� �� ������ �浹üũ�� �̾��.
        {
            Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);
            // Raycast : �浹�ϴ� collider�� ���� �Ÿ�, ��ġ�� �ڼ��� ������ RaycastHit�� ��ȯ.
            // ����, LayerMask ���͸��� �ؼ� ���ϴ� layer�� �浹�� �ϸ� true �� ��ȯ.
            // �߰���, OverlapSphere : ������ ���������� ������ ���� ����� �����Ϸ��� �ݰ� �̳��� ���� �ִ� collider�� ��ȯ�ϴ� �Լ�.
            // �Լ��� ��ȯ ���� collider ������Ʈ �迭�� �Ѿ�´�.
            if (Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
            {
                if (hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger) // isTrigger�� false�� �δ� ������ �̺�Ʈ�� ������ ���� Collider�� ���� ���Ѿ� �Ǳ⶧��.
                {
                    return false;
                }
            }
            return true;
        }
        bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
        {
            Vector3 origin = player.position + (Vector3.up * deltaPlayerHeight);
            if (Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
            {
                if (hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// ���� üũ�� ���ؼ� true�� false ��ȯ true -> �浹X false -> �� �߿� �ϳ��� �浹�� �ߴ�.
        /// </summary>
        bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
        {
            float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
            return ViewingPosCheck(checkPos, playerFocusHeight) && ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
        }
        #endregion

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            // subscribe reset action to scheduler to know when to reset actions
            _scheduler.OnUpdatedAbilities += ResetActions;
#endif

            // subscribe for death event
            if (_health != null)
                _health.OnDead += Die;
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            // unsubscribe reset action
            _scheduler.OnUpdatedAbilities -= ResetActions;
#endif
            // unsubscribe for death event
            if (_health != null)
                _health.OnDead -= Die;
        }

        private void Update()
        {
            UpdateCharacterActions();

            if (CurrentRecoil > 0)
                CurrentRecoil = Mathf.SmoothDamp(CurrentRecoil, 0, ref recoilReturnVel, 0.2f);

#if ENABLE_LEGACY_INPUT_MANAGER
            LegacyInput();
#endif
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void Die()
        {
            _scheduler.StopScheduler();

            // disable any movement
            _mover.DisableGravity();
            _mover.StopMovement();

            // disable main character collision
            _capsule.DisableCollision();

            // activate root motion
            _mover.ApplyRootMotion(Vector3.one);
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (Look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                _cinemachineTargetYaw += Look.x * CameraTurnSpeed.x * Time.deltaTime;
                _cinemachineTargetPitch += Look.y * CameraTurnSpeed.y * Time.deltaTime;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void UpdateCharacterActions()
        {
            _scheduler.characterActions.move = Move;
            _scheduler.characterActions.jump = Jump;
            _scheduler.characterActions.walk = Walk;
            _scheduler.characterActions.sprint = Sprint;
            _scheduler.characterActions.roll = Roll;
            _scheduler.characterActions.crouch = Crouch;
            _scheduler.characterActions.interact = Interact;
            _scheduler.characterActions.crawl = Crawl;
            _scheduler.characterActions.drop = Drop;

            // weapon
            _scheduler.characterActions.zoom = Zoom;
        }

        #region Input receiver

        [Header("Input")]
        public Vector2 Move = Vector2.zero;
        public Vector2 Look = Vector2.zero;
        public bool Jump = false;
        public bool Walk = false;
        public bool Sprint = false;
        public bool Roll = false;
        public bool Crouch = false;
        public bool Interact = false;
        public bool Crawl = false;
        public bool Zoom = false;
        public bool Drop = false;

        public void ResetActions()
        {
            Jump = false;
            Roll = false;
            Crawl = false;
            Interact = false;
            Drop = false;
        }

        public void LegacyInput()
        {
            Move.x = Input.GetAxis("Horizontal");
            Move.y = Input.GetAxis("Vertical");

            Look.x += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
            Look.y += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1f) * verticalAimingSpeed;

            // ���� �̵� ����.
            Look.y = Mathf.Clamp(Look.y, minverticalAngle, targetMaxVerticleAngle);
            // ���� ī�޶� �ٿ.
            Look.y = Mathf.LerpAngle(Look.y, Look.y + recoilAngle, 10f * Time.deltaTime); // ���� ����.

            Walk = Input.GetButton("Walk");
            Sprint = Input.GetButton("Sprint");
            Jump = Input.GetButtonDown("Jump");
            Roll = Input.GetButtonDown("Roll");
            Crouch = Input.GetButton("Crouch");
            Crawl = Input.GetButtonDown("Crawl");
            Zoom = Input.GetButtonDown("Zoom");
            Interact = Input.GetButtonDown("Interact");

            // special actions for climbing
            Drop = Input.GetButtonDown("Drop");

            /*
            // special actions for shooter
            Fire = Input.GetButton("Fire");
            Reload = Input.GetButtonDown("Reload");
            Switch = Input.GetAxisRaw("Switch");
            Toggle = Input.GetButtonDown("Toggle");*/
        }

        public void OnMove(Vector2 value)
        {
            Move = value;
        }
        public void OnLook(Vector2 value)
        {
            Look = value;
        }
        public void OnJump(bool value)
        {
            Jump = value;
        }
        public void OnWalk(bool value)
        {
            Walk = value;
        }
        public void OnRoll(bool value)
        {
            Roll = value;
        }
        public void OnCrouch(bool value)
        {
            Crouch = value;
        }
        public void OnCrawl(bool value)
        {
            Crawl = value;
        }

        public void OnZoom(bool value)
        {
            Zoom = value;
        }
        public void OnInteract(bool value)
        {
            Interact = value;
        }
        public void OnDrop(bool value)
        {
            Drop = value;
        }

#if ENABLE_INPUT_SYSTEM
        private void OnMove(InputValue value)
        {
            OnMove(value.Get<Vector2>());
        }


        private void OnLook(InputValue value)
        {
            OnLook(value.Get<Vector2>());
        }

        private void OnJump(InputValue value)
        {
            OnJump(value.isPressed);
        }

        private void OnWalk(InputValue value)
        {
            OnWalk(value.isPressed);
        }

        private void OnRoll(InputValue value)
        {
            OnRoll(value.isPressed);
        }

        private void OnCrouch(InputValue value)
        {
            OnCrouch(value.isPressed);
        }


        private void OnCrawl(InputValue value)
        {
            OnCrawl(value.isPressed);
        }

        private void OnZoom(InputValue value)
        {
            OnZoom(value.isPressed);
        }

        private void OnInteract(InputValue value)
        {
            OnInteract(value.isPressed);
        }
        private void OnDrop(InputValue value)
        {
            OnDrop(value.isPressed);
        }

#endif

        #endregion
    }
}