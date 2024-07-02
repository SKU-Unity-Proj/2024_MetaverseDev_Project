using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using DiasGames.Components;

using Fusion;

namespace DiasGames.Controller
{
    namespace _01_04_Network_Properties 
    {
        /// <summary>
        /// https://doc-api.photonengine.com/en/fusion/v2/class_fusion_1_1_network_behaviour.html
        /// NetworkBehaviour.FixedUpdateNetwork를 사용한다.
        /// </summary>
        public class CSPlayerController : NetworkBehaviour
        {
            public Transform player;
            public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
            public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

            public float smooth = 10f;  // 카메라 반응속도
            public float horizontalAimingSpeed = 6.0f; // 수평 회전 속도.
            public float verticalAimingSpeed = 6.0f; // 수직 회전 속도.
            public float maxVerticalAngle = 30.0f; // 카메라 수직 최대 각도
            public float minverticalAngle = -60.0f; // 카메라 수직 최소 각도
            public float recoilAngleBounce = 5.0f; // 사격 반동 값

            public float pushPower = 5.0f; // 밀어내는 힘의 크기를 조절하는 변수

            public GameObject ladder1;
            public GameObject ladder2;
            public GameObject lever;
            public GameObject cellerGiant;

            public Transform cameraTransform; // 트랜스폼 캐싱.
            private Camera myCamera;
            private Vector3 relCameraPos; // 플레이어로부터 카메라까지의 벡터.
            private float relCameraPosMag; // 플레이어로부터 카메라사이 거리.
            private Vector3 smoothPivotOffset; // 카메라 피봇 보간용 벡터. 
            private Vector3 smoothCamOffset; // 카메라 위치 보간용 벡터. 
            private Vector3 targetPivotOffset; // 카메라 피봇 보간용 벡터. 
            private Vector3 targetCamOffset; // 카메라 위치 보간용 벡터.
            private float defaultFOV; // 기본 시야값.
            private float targetFOV; // 타겟 시야값.
            private float targetMaxVerticleAngle; // 카메라 수직 최대 각도. -> 사격시 반동
            private float recoilAngle = 0f; // 사격 반동 각도

            private Vector3 startPosition;


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

                if (hideCursor)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }

                // set right angle on start for camera
                _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;

                //캐싱

                myCamera = cameraTransform.GetComponent<Camera>();

                // 카메라 기본 포지션 세팅
                cameraTransform.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
                cameraTransform.rotation = Quaternion.identity;

                // 카메라와 플레이어간의 상대 벡터. -> 충돌체크에 사용함
                relCameraPos = cameraTransform.position - player.position;
                relCameraPosMag = relCameraPos.magnitude - 0.5f; // 플레이어를 뺀 거리.

                // 기본세팅
                smoothPivotOffset = pivotOffset;
                smoothCamOffset = camOffset;
                defaultFOV = myCamera.fieldOfView;
                Look.x = player.eulerAngles.y; // 초기 y값.

                ResetTargetOffsets();
                ResetFOV();
                ResetMaxVerticalAngle();
            }

            #region CAMERA Setting
            //초기화 함수
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

            // 기본 세팅 조절 함수
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

            // 충돌체크 함수
            bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight) // playerPos는 발밑이므로 플레이어의 높이를 체크해서 그 앞으로 충돌체크를 이어나감.
            {
                Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);
                // Raycast : 충돌하는 collider에 대한 거리, 위치등 자세한 정보를 RaycastHit로 반환.
                // 또한, LayerMask 필터링을 해서 원하는 layer에 충돌을 하면 true 값 반환.
                // 추가로, OverlapSphere : 중점과 반지름으로 가상의 원을 만들어 추출하려는 반경 이내에 들어와 있는 collider를 반환하는 함수.
                // 함수의 반환 값은 collider 컴포넌트 배열로 넘어온다.
                if (Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
                {
                    if (hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger) // isTrigger을 false로 두는 이유는 이벤트나 연출을 위한 Collider는 제외 시켜야 되기때문.
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
            /// 서로 체크를 통해서 true와 false 반환 true -> 충돌X false -> 둘 중에 하나는 충돌을 했다.
            /// </summary>
            bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
            {
                float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
                return ViewingPosCheck(checkPos, playerFocusHeight) && ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
            }
            #endregion

            #region 오브젝트 밀쳐내기 관련 함수
            private void OnCollisionEnter(Collision collision)
            {
                Rigidbody rb = collision.collider.attachedRigidbody;

                // Rigidbody가 없거나, Kinematic 상태이면 무시
                if (rb == null || rb.isKinematic)
                {
                    return;
                }

                /*
                 * 어차피 힘 딸리면 못 미니까 그냥 태그 뺐음. 태그 지정해주기 귀찮
                // 특정 태그를 가진 오브젝트에만 힘을 적용
                if (collision.gameObject.tag != "Pushable")
                {
                    return;
                }
                */

                // 충돌 지점과 플레이어의 위치 차이를 계산하여, 
                // 이를 단위 벡터로 정규화함. 이 벡터는 밀어내는 방향을 나타낸다.
                Vector3 pushDirection = (collision.contacts[0].point - transform.position).normalized;
                pushDirection.y = 0; // Y축 방향 제외, 수평 방향으로만 힘 적용

                // 힘의 적용 지점을 조정하여 자연스러운 회전 유도
                Vector3 forcePoint = collision.contacts[0].point + pushDirection * 0.5f; // 약간의 오프셋을 주어 힘 적용 지점 조정
                rb.AddForceAtPosition(pushDirection * pushPower, forcePoint, ForceMode.Impulse); // 계산된 방향과 지점에 힘을 작용하기 위해 사용함
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

            public override void FixedUpdateNetwork()
            {
                UpdateCharacterActions();

                if (CurrentRecoil > 0)
                    CurrentRecoil = Mathf.SmoothDamp(CurrentRecoil, 0, ref recoilReturnVel, 0.2f);

#if ENABLE_LEGACY_INPUT_MANAGER
                LegacyInput();
#endif
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
                _scheduler.characterActions.pickUp = PickUp;

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
            public bool PickUp = false;

            public void ResetActions()
            {
                Jump = false;
                Roll = false;
                Crawl = false;
                Interact = false;
                Drop = false;
                PickUp = false;
            }

            public void LegacyInput()
            {
                Move.x = Input.GetAxis("Horizontal");
                Move.y = Input.GetAxis("Vertical");

                Look.x += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
                Look.y += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1f) * verticalAimingSpeed;

                // 수직 이동 제한.
                Look.y = Mathf.Clamp(Look.y, minverticalAngle, targetMaxVerticleAngle);
                // 수직 카메라 바운스.
                Look.y = Mathf.LerpAngle(Look.y, Look.y + recoilAngle, 10f * Time.deltaTime); // 각도 보간.

                Walk = Input.GetButton("Walk");
                Sprint = Input.GetButton("Sprint");
                Jump = Input.GetButtonDown("Jump");
                Roll = Input.GetButtonDown("Roll");
                Crouch = Input.GetButton("Crouch");
                Crawl = Input.GetButtonDown("Crawl");
                Zoom = Input.GetButton("Zoom");
                Interact = Input.GetButtonDown("Interact");

                // special actions for climbing
                Drop = Input.GetButtonDown("Drop");

                PickUp = Input.GetButtonDown("PickUp");

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
            public void OnPickUp(bool value)
            {
                PickUp = value;
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
            private void OnPickUp(InputValue value)
            {
                OnPickUp(value.isPressed);
            }

#endif

            #endregion
        }
    }
}