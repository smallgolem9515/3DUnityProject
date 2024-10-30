using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum WeaponMode
{
    None,
    Pistol,
    ShotGun,
    Rifle,
    SMG
}
public class StudyPlayerManager : MonoBehaviour
{
    public static StudyPlayerManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("-----PlayerMove-----")]
    float playerSpeedWalk = 3f; //걷는 속도
    float playerSpeedRun = 5f; //달리기 속도
    public float currentSpeed = 1f; //변경 속도
    public float hp;
    float gravity = -9.81f; //중력 - 기본적인 유니티에서의 중력
    Vector3 velocity; //현재 속도 저장
    CharacterController characterController; //리지드바디는 3D에서 이런저런 문제가 있어서 캐릭터 컨트롤러를 사용한다.
    //단 중력이 없어서 따로 설정해야한다.
    public float damageDelay = 2.0f;

    [Header("-----PlayerBool-----")]
    public bool isFire = true;
    public bool isDamage = false;
    bool isGameOver = false;
    bool isJumping = false;

    [Header("-----Camera-----")]
    public Transform cameraTransform; //카메라 Transform
    public Transform playerHead; //플레이어 머리 위치(1인칭 모드일때 사용)
    public float thirdPersonDistance = 3.0f; // 3인칭 모드에서 플레이어와 카메라 시야 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); //3인칭 모드에서 카메라오프셋
    float mouseSensitivity = 100f; //마우스 감도
    public Transform playerLookObj; //플레이어의 시야 위치

    [Header("-----Zoom-----")]
    public float zoomedDistance = 1.0f; //카메라가 확대될 때의 거리(3인칭 모드일때 사용)
    public float zoomSpeed = 5f; //확대 축소가 되는 속도
    public float defaultFov = 60f; //기본 카메라 시야각
    public float zoomedFov = 30f; //확대시 카메라 시야각 

    float currentDistance; //현재 카메라와의 거리
    float targetDistance; //목표 카메라 거리
    float targetFov; //목표 Fov
    public bool isZoomed = false; //확대 여부
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대/축소
    Camera mainCamera; //카메라 컴포넌트 

    float pitch = 0f; //위아래 회전값
    float yaw = 0f; //좌우 회전값
    bool isFirstPerson = false; //1인칭 모드 여부
    bool rotaterAroundPlayer = false; //카메라가 플레이어 주위를 회전하는 여부

    [Header("-----Jump-----")]
    public float jumpHeight = 2f; //점프 높이
    bool isGround; //땅에 충돌 여부
    public LayerMask groundLayer; //땅의 레이어

    Animator animator;
    float horizontal; //X축 이동
    float vertical; //Z축 이동
    [Header("-----FootSoundSetting-----")]
    public Transform leftFoot; //왼쪽 발
    public Transform rightFoot; //오른쪽 발
    bool isLeftFootGround = false;
    bool isRightFootGround = false;

    Vector3 previousPosition;
    public float fallThreshold = -10f;
    Vector3 startPosition;

    [Header("-----UpperBody-----")]
    public Transform upperBody; //상체 본을 할당(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //상체 Aim 모드에서만 회전을 한다.
    private quaternion originalUpperBodyRotation; //원래 상체 회전 값

    public Transform aimTarget;
    [Header("-----SlowMotion-----")]
    public float slowMotionScale = 0.5f;
    private float defaltTimeScale = 1f;
    private bool isSlowMotion = false;

    [Header("-----ShotRayCast-----")]
    public float maxShotDistance = 100f;
    public LayerMask targetLayer;

    [Header("-----WeaponSetting-----")]
    public float recoilStrength = 2f; //반동의 세기
    public float maxRecoilAngle = 10.0f; //반동의 최대 각도
    private float currentRecoil = 0f; //현재 반동 값을 저장하는 변수
    private int shotGunRayCount = 5; //샷건의 총알이 퍼지는 수
    private float shotGunSpreadAngle = 5f; //샷건의 총알이 퍼지는 각도

    //BoxCast 변수
    [Header("-----BoxCast-----")]
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5f; //BoxCast 멀리 감지 거리
    public LayerMask itemLayer; //아이템 레이어
    public Transform itemGetPos; //BoxCast 위치
    public float debugDuration = 2.0f; //디버그 라인 여부

    public GameObject crossHair;
    public GameObject noneCrossHair;

    private float fireDelay = 0.1f; //발사 딜레이

    public Light flashLight;
    private bool isFlashLight = false;

    private Rigidbody[] ragdollbodies;
    private Collider[] ragdollcolliders;

    private bool lastOpenedForward = true; //마지막으로 문이 정방향으로 열렸는지 여부
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;
    private Vector3 orizinalPos;
    public Image bloodImage;

    [Header("-----Weapon Bullet-----")]
    public bool isReloading = false;
    public int pistolBulletCount = 120;
    public int pistolCurrentBulletCount = 12;
    public int pistolMaxBulletCount = 12;
    public int shotGunBulletCount = 60;
    public int shotGunCurrentBulletCount = 4;
    public int shotGunMaxBulletCount = 4;
    public int rifleBulletCount = 90;
    public int rifleCurrentBulletCount = 8;
    public int rifleMaxBulletCount = 8;

    public GameObject gunBackGround;
    public GameObject gunImage;
    public Text bulletCountText;
    public Sprite[] gunSprites;
    public GameObject escMenu;
    public bool isESCMenu = false;
    public Text hpText;
    public GameObject clearImage;
    bool isClear = false;
    public GameObject dieImage;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        UnityEngine.Cursor.lockState = CursorLockMode.Locked; //커서 안보이게 설정
        currentDistance = thirdPersonDistance; //초기 카메라 거리를 설정
        targetDistance = thirdPersonDistance; //목표 카메라 거리 설정
        targetFov = defaultFov; //초기 Fov 설정
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov; //기본 Fov설정

        currentSpeed = playerSpeedWalk;
        startPosition = transform.position + Vector3.up;
        previousPosition = transform.position; //시작시 현재 위치를 이전 위치로 초기화
        animator.SetLayerWeight(1, 0);
        animator.SetInteger("WeaponType", 0);

        if (upperBody != null)
        {
            originalUpperBodyRotation = upperBody.localRotation;
        }
        flashLight.enabled = false;

        ragdollbodies = GetComponentsInChildren<Rigidbody>();
        ragdollcolliders = GetComponentsInChildren<Collider>();
        StudySoundManager.Instance.PlayBGM("GameLevel1");
    }
    private void Update()
    {
        if (transform.position.y < fallThreshold && !isGameOver)
        {
            GameOver();
        }
        hpText.text = "HP : " + hp + "/10";
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (!isGameOver && !isClear)
        {

            //bool isMoving = characterController.velocity.magnitude > 0.1f;
            if (horizontal != 0 || vertical != 0)
            {
                FootStepSound();
            }
            //현재 위치를 이전 위치로 저장(다음 프레임에서 비교하기위함)
            previousPosition = transform.position;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (escMenu.activeSelf)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    isESCMenu = false;
                    escMenu.SetActive(false);
                    Time.timeScale = 1.0f;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    isESCMenu = true;
                    escMenu.SetActive(true);
                    Time.timeScale = 0;
                }
            }
            UpdateCameraRotation();
            if(!isESCMenu)
            {
                if (isFirstPerson)
                {
                    FirstPersonMovement(); //1인칭
                }
                else
                {
                    ThirdPersonMovement(); //3인칭
                }
                RunAction();
                Jump(); //점프

                BulletCountText();

                FireWeapon();
                ChangeWeapon();
                ZoomMouseRightButton(); //오른쪽 마우스버튼으로 줌
                if (Input.GetKeyDown(KeyCode.V)) //V키로 1인칭/3인칭 전환
                {
                    isFirstPerson = !isFirstPerson;
                    Debug.Log(isFirstPerson ? "1인칭모드" : "3인칭모드");
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson) //Alt키로 시점 전환
                {
                    rotaterAroundPlayer = !rotaterAroundPlayer;
                    Debug.Log(rotaterAroundPlayer ? "카메라가 플레이어 주위를 회전" : "플레이어가 직접 회전");
                }
                if (currentRecoil > 0) //반동으로 초점이 어긋나면 원래 초점으로 돌아오는 기능
                {
                    currentRecoil -= recoilStrength * Time.deltaTime;

                    currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    isSlowMotion = !isSlowMotion;
                    ToggleSlowMotion();
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    GetItem();
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    ToggleFlashLight();
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    StartCoroutine(WeaponReload());
                }
            }
        }
    }
    private void LateUpdate()
    {
        if (isZoomed)
        {
            if (upperBody != null)
            {
                upperBody.localRotation = quaternion.Euler(upperBodyRotationAngle, 0, 0);
            }
        }
        else
        {
            if (upperBody != null)
            {
                upperBody.localRotation = originalUpperBodyRotation;
            }
        }
    }
    private void FixedUpdate()
    {

    }
    void UpdateAimTarget() //플레이어가 바라보는 방향
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        aimTarget.position = ray.GetPoint(10);
    }
    void FirstPersonMovement() //1인칭
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical + transform.up * velocity.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        mainCamera.transform.position = playerHead.position;
    }
    void ThirdPersonMovement() //3인칭
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical + transform.up * velocity.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        if (rotaterAroundPlayer)
        {
            UpdateCameraPositionRotater();
        }
        else
        {
            UpdateCameraPosition();
        }
    }
    void UpdateCameraRotation()//카메라 회전
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; //마우스값을 가져오고 민감도와 시간을 곱한다.
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -45f, 45f);

        mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
    void UpdateCameraPosition()//플레이어가 직접 회전
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        //mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
        //카메라의 위치가 자기 자신에서 살짝위 + 방향을 바꿔가며 + 살짝 뒤에서

        //카메라가 플레이어의 위치를 따라가도록 설정
        cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y + currentRecoil, 0));

        UpdateAimTarget();
    }
    void UpdateCameraPositionRotater()// 카메라가 플레이어 주위를 회전 
    {
        Vector3 direction = new Vector3(0, 0, -currentDistance); //카메라 거리 설정
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
        cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;
        //카메라의 위치가 자기 자신에서 살짝위 + 방향을 바꿔가며 + 살짝 뒤에서

        //카메라가 플레이어의 위치를 따라가도록 설정
        cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }
    void Jump()// 점프 함수
    {
        isGround = CheckIfGround();
        // "JumpUp" 애니메이션이 완료되었는지 확인 (normalizedTime >= 1)
        bool jumpUpCompleted = animator.GetCurrentAnimatorStateInfo(0).IsName("JumpingUp")
            && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        if (isGround && isJumping && jumpUpCompleted)
        {
            animator.SetTrigger("JumpDown");
            isJumping = false;
        }
        if (Input.GetButtonDown("Jump") && isGround)
        {
            StudySoundManager.Instance.PlaySFX("Jump", transform.position);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpUp");
            isJumping = true;
        }
        velocity.y += gravity * Time.deltaTime;
    }
    bool CheckIfGround()// 레이캐스트로 바닥 확인
    {
        // 캐릭터 발 아래의 중심 위치 설정 (약간 위로 올려서 시작)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast의 크기 설정 (캐릭터의 발 크기와 맞게 조정)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast 발사
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, quaternion.identity, 0.2f, groundLayer);

        // 디버깅용 BoxCast 시각화
        Color castColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxCenter, Vector3.down * 0.2f, castColor);

        return isGrounded;
        //float rayDistance = 0.2f;

        //if(Physics.Raycast(transform.position,Vector3.down,out hit, rayDistance,groundLayer))
        //{
        //    return true;
        //}
        //return false;
    }
    void RunAction() //뛰는 기능
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        if (Input.GetKey(KeyCode.LeftShift)) //애니메이션을 통한 달리기 구현
        {
            animator.SetBool("isRun", true);
            currentSpeed = playerSpeedRun;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetBool("isRun", false);
            currentSpeed = playerSpeedWalk;
        }
    }
    void FootStepSound() //발소리
    {
        bool leftHit = CheckGround(leftFoot);
        bool rightHit = CheckGround(rightFoot);

        if (leftHit && !isLeftFootGround)
        {
            if (StudySoundManager.Instance.SFXAudioSourse.isPlaying) return;
            StudySoundManager.Instance.PlaySFX("DefaltFootStep", leftFoot.position);
        }
        if (rightHit && !isRightFootGround)
        {
            if (StudySoundManager.Instance.SFXAudioSourse.isPlaying) return;
            StudySoundManager.Instance.PlaySFX("DefaltFootStep", rightFoot.position);
        }
        //현재 상태를 다음 프레임과 비교하기 위해 저장
        isLeftFootGround = leftHit;
        isRightFootGround = rightHit;
    }
    bool CheckGround(Transform foot) //발이 땅에 닿았는지 체크
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }
    void ZoomMouseRightButton()// 마우스 오른쪽 버튼으로 줌
    {
        if (Input.GetMouseButtonDown(1) && !isZoomed) //오른쪽 마우스 버튼을 눌렀을 때
        {
            isZoomed = true;

            noneCrossHair.SetActive(false);
            crossHair.SetActive(true);
            if(StudyWeaponManager.Instance.GetCurrentWeaponType() != Weapon.WeaponType.None)
            {
                gunBackGround.SetActive(true);
            }
            AimWeapon();

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);

            }
            if (isFirstPerson)
            {
                SetTargetFOV(zoomedFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(zoomedDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }
        if (Input.GetMouseButtonUp(1) && isZoomed)
        {
            isZoomed = false;

            noneCrossHair.SetActive(true);
            crossHair.SetActive(false);
            gunBackGround.SetActive(false);

            animator.SetLayerWeight(1, 0);

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);

            }
            if (isFirstPerson)
            {
                SetTargetFOV(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }
    }
    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }
    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }
    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        currentDistance = targetDistance; //목표거리에 도달한후 값을 고정
    }
    IEnumerator ZoomFieldOfView(float targetDistance)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }
    void FireWeapon() //총에 따라 달라지게하는 기능
    {
        if (Input.GetMouseButtonDown(0) && isZoomed && isFire && !isReloading)
        {
            StartCoroutine(ShotDelay());
        }
    }
    void ApplyRecoil() //반동 기능
    {
        //현재 카메라의 월드 회전을 가져옴
        Quaternion currentRotation = Camera.main.transform.rotation;

        //반동 값을 증가시킴
        currentRecoil += recoilStrength;

        //반동 값을 MAX에 맞춰서 제한
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);

        //반동을 계산하여 X축(상하) 회전에 추가(위로 올라가는 반동)
        Quaternion recoilRotation = quaternion.Euler(-currentRecoil, 0, 0);

        //현재 회전 값에 반동을 곱하여 새로운 회전값을 적용
        //Camera.main.transform.rotation = currentRotation * recoilRotation;
        Debug.Log(currentRecoil);
    }
    IEnumerator ShotDelay() //총 쏘고난 후 딜레이
    {
        
        if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
        {
            if (pistolCurrentBulletCount <= 0) yield break;
            fireDelay = 0.8f;
            recoilStrength = 0.05f;
            FirePistol();
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
        {
            if (shotGunCurrentBulletCount <= 0) yield break;
            fireDelay = 1.0f;
            recoilStrength = 0.1f;
            FireShotGun();
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
        {
            if(rifleCurrentBulletCount <= 0) yield break;
            fireDelay = 1.2f;
            recoilStrength = 0.08f;
            FireRifle();
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        {
            fireDelay = 0.1f;
            FireSMG();
        }
        CrossHair.instance.WeaponCrossSpeed();
        isFire = false;
        ApplyRecoil();
        yield return new WaitForSeconds(fireDelay);

        isFire = true;
    }
    void FirePistol()
    {
        animator.SetTrigger("FirePistol");
        StudySoundManager.Instance.PlaySFX("FirePistol", transform.position);
        Weapon currentWeapon = StudyWeaponManager.Instance.GetCurrentWeaponComponent();
        ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.PistolEffect, currentWeapon.effectPos.position);
        Debug.Log(currentWeapon.effectPos.position);
        
        pistolCurrentBulletCount--;
        pistolCurrentBulletCount = (int)MathF.Max(pistolCurrentBulletCount, 0);

        RaycastHit hit;
        Vector3 orizin = Camera.main.transform.position;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Vector2 direction = Camera.main.transform.forward;

        maxShotDistance = 100f;
        Debug.DrawRay(orizin, ray.direction * maxShotDistance, Color.red, 1.0f);

        if (Physics.Raycast(orizin, ray.direction * maxShotDistance, out hit))
        {
            if (hit.collider.tag == "Ground")
            {
                ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.RockImpactEffect, hit.point);
                Debug.Log("Hit : " + hit.collider.name);
            }
            if(hit.collider.tag == "Zombie")
            {
                StudyZombieAi zombieAi = hit.collider.GetComponent<StudyZombieAi>();

                if (zombieAi != null)
                {
                    StartCoroutine(zombieAi.TakeDamage(5, hit.collider.tag));
                }
            }
            else if(hit.collider.tag == "Head")
            {
                StudyZombieAi zombieAi = hit.collider.GetComponentInParent<StudyZombieAi>();
                if(zombieAi != null)
                {
                    StartCoroutine(zombieAi.TakeDamage(5, hit.collider.tag));
                }
            }
        }
    }
    void FireShotGun()
    {
        animator.Play("FireShotGun");
        StudySoundManager.Instance.PlaySFX("FireShotGun", transform.position);
        Weapon currentWeapon = StudyWeaponManager.Instance.GetCurrentWeaponComponent();
        ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.ShotGunEffect, currentWeapon.effectPos.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        shotGunCurrentBulletCount--;
        shotGunCurrentBulletCount = (int)MathF.Max(shotGunCurrentBulletCount, 0);

        maxShotDistance = 250f;

        for (int i = 0; i < shotGunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 orizin = Camera.main.transform.position;

            float spreadX = UnityEngine.Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);
            float spreadY = UnityEngine.Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);

            Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * ray.direction;

            Debug.DrawRay(orizin, spreadDirection * maxShotDistance, Color.red, 1.0f);
            if (Physics.Raycast(orizin, spreadDirection * maxShotDistance, out hit, targetLayer))
            {
                if (hit.collider.tag == "Ground")
                {
                    ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.RockImpactEffect, hit.point);
                    Debug.Log("Hit : " + hit.collider.name);
                }
                if (hit.collider.tag == "Zombie")
                {
                    StudyZombieAi zombieAi = hit.collider.GetComponent<StudyZombieAi>();

                    if (zombieAi != null)
                    {
                        StartCoroutine(zombieAi.TakeDamage(20, hit.collider.tag));
                    }
                }
                else if (hit.collider.tag == "Head")
                {
                    StudyZombieAi zombieAi = hit.collider.GetComponentInParent<StudyZombieAi>();
                    if (zombieAi != null)
                    {
                        StartCoroutine(zombieAi.TakeDamage(20, hit.collider.tag));
                    }
                }
            }
        }
    }
    void FireRifle()
    {
        animator.Play("FireRifle");
        StudySoundManager.Instance.PlaySFX("FireRifle", transform.position);

        Weapon currentWeapon = StudyWeaponManager.Instance.GetCurrentWeaponComponent();
        ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.RifleEffect, currentWeapon.effectPos.position);

        rifleCurrentBulletCount--;
        rifleCurrentBulletCount = (int)MathF.Max(rifleCurrentBulletCount, 0);

        RaycastHit hit;
        Vector3 orizin = Camera.main.transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        maxShotDistance = 1000f;

        Debug.DrawRay(orizin, ray.direction * maxShotDistance, Color.red, 1.0f);

        if (Physics.Raycast(orizin, ray.direction * maxShotDistance, out hit, targetLayer))
        {
            if (hit.collider.tag == "Ground")
            {
                ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.RockImpactEffect, hit.point);
                Debug.Log("Hit : " + hit.collider.name);
            }
            if (hit.collider.tag == "Zombie")
            {
                StudyZombieAi zombieAi = hit.collider.GetComponent<StudyZombieAi>();

                if (zombieAi != null)
                {
                    StartCoroutine(zombieAi.TakeDamage(15, hit.collider.tag));
                }
            }
            else if (hit.collider.tag == "Head")
            {
                StudyZombieAi zombieAi = hit.collider.GetComponentInParent<StudyZombieAi>();
                if (zombieAi != null)
                {
                    StartCoroutine(zombieAi.TakeDamage(15, hit.collider.tag));
                }
            }
        }
    }
    void FireSMG()
    {
        animator.SetTrigger("FireSMG");
        StudySoundManager.Instance.PlaySFX("FireSMG", transform.position);

        RaycastHit hit;
        Vector3 orizin = Camera.main.transform.position;
        Vector2 direction = Camera.main.transform.forward;

        maxShotDistance = 200f;

        Debug.DrawRay(orizin, direction * maxShotDistance, Color.red, 1.0f);

        if (Physics.Raycast(orizin, direction * maxShotDistance, out hit, targetLayer))
        {
            Debug.Log("Hit : " + hit.collider.name);
        }
    }
    void AimWeapon() //웨폰에 따라 에임이 달라지는 기능
    {
        animator.SetLayerWeight(1, 1);

        //if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
        //{
        //    AimPistol();
        //}
        //else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
        //{
        //    AimShotGun();
        //}
        //else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
        //{
        //    AimRifle();
        //}
        //else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        //{
        //    AimSMG();
        //}
    }
    void AimPistol()
    {
        animator.Play("PistolAim");
    }
    void AimShotGun()
    {
        animator.Play("ShotGunAim");
    }
    void AimRifle()
    {
        animator.Play("RifleAim");
    }
    void AimSMG()
    {
        animator.Play("SMGAim");
    }
    void ChangeWeapon() //1,2,3,4로 무기 변경
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StudyWeaponManager.Instance.EquipWeapon(Weapon.WeaponType.Pistol);
            Debug.Log("Pistol Change");
            if(StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
            {
                gunImage.GetComponent<Image>().sprite = gunSprites[0];
                animator.SetInteger("WeaponType", 1);
                StudySoundManager.Instance.PlaySFX("WeaponGet", transform.position);

            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StudyWeaponManager.Instance.EquipWeapon(Weapon.WeaponType.ShotGun);
            Debug.Log("ShotGun Change");
            if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
            {
                gunImage.GetComponent<Image>().sprite = gunSprites[1];
                animator.SetInteger("WeaponType", 2);
                StudySoundManager.Instance.PlaySFX("WeaponGet", transform.position);

            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StudyWeaponManager.Instance.EquipWeapon(Weapon.WeaponType.Rifle);
            Debug.Log("Rifle Change");
            if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
            {
                gunImage.GetComponent<Image>().sprite = gunSprites[2];
                animator.SetInteger("WeaponType", 3);
                StudySoundManager.Instance.PlaySFX("WeaponGet", transform.position);

            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StudyWeaponManager.Instance.EquipWeapon(Weapon.WeaponType.SMG);
            //currentWeaponMode = WeaponMode.SMG;
            Debug.Log("SMG Change");
        }
    }
    void GameOver() // 게임오버
    {
        isGameOver = true;
        dieImage.SetActive(true);
        Invoke(nameof(RestartGame), 2.0f);
    }
    void RestartGame() //리스타트
    {
        //animator.enabled = true;
        //SetRagdollState(false);
        //transform.position = startPosition;
        //isGameOver = false;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene("Menu");
    }
    public void TakeDamage(float damage) //데미지를 입는 기능
    {
        if(!isDamage && !isGameOver)
        {
            StudySoundManager.Instance.PlaySFX("PlayerDamage",transform.position);
            hp -= damage;
            hp = Mathf.Max(hp, 0);
            StartCoroutine(DamageTime());
            StartCoroutine(CameraShake(shakeDuration,shakeMagnitude));
            if (hp <= 0)
            {
                ActivateRagdoll();
                GameOver();
            }
        }
    }
    IEnumerator DamageTime()
    {
        isDamage = true;
        yield return new WaitForSeconds(damageDelay);
        isDamage = false;
    }
    void ToggleSlowMotion() //슬로우모션 기능
    {
        if (!isSlowMotion)
        {
            Time.timeScale = defaltTimeScale;
            currentSpeed /= 2;
            Debug.Log("슬로우 모션 해제");
        }
        else
        {
            Time.timeScale = slowMotionScale;
            currentSpeed *= 2;
            Debug.Log("슬로우 모션 활성화");
        }
    }
    void GetItem()
    {
        RaycastHit[] hits;
        Vector3 orizin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;

        hits = Physics.BoxCastAll(orizin, boxSize / 2, direction, quaternion.identity, castDistance, itemLayer);

        DebugBoxCast(orizin, direction);
        foreach (RaycastHit hit in hits)
        {
            GameObject item = hit.collider.gameObject;

            DoorBase door = item.GetComponent<DoorBase>();

            if (door != null)
            {
                if (door.isOpen)
                {
                    StudySoundManager.Instance.PlaySFX("DoorClose", hit.transform.position);
                    if (!lastOpenedForward)
                    {
                        door.CloseForward(transform);
                    }
                    else
                    {
                        door.CloseBackward(transform);
                    }
                }
                else
                {
                    if (door.Open(transform))
                    {
                        lastOpenedForward = door.lastOpenForward;
                    }
                }
                return;
            }
            Debug.Log(hit.collider.gameObject.name);

            if (item.CompareTag("Weapon"))
            {
                StudySoundManager.Instance.PlaySFX("WeaponGet",transform.position);
                StudyWeaponManager.Instance.AddWeapon(item);
                item.SetActive(false);
            }
            else if (item.CompareTag("Ammo"))
            {
                Debug.Log($"아이템 감지 : {item.name}");
                if (item.name.Contains("Pistol"))
                {
                    pistolBulletCount += 30;
                }
                else if (item.name.Contains("ShotGun"))
                {
                    shotGunBulletCount += 12;
                }
                else if (item.name.Contains("Rifle"))
                {
                    rifleBulletCount += 20;
                }
                item.SetActive(false);
            }
            else
            {
                return;
            }

            animator.SetTrigger("PickUp");

        }
    }
    void DebugBoxCast(Vector3 orizin, Vector3 direction)
    {
        Vector3 enPoint = orizin + direction * maxShotDistance;
        //BoxCast의 모양을 그리기 위한 8개의 모서리 좌표 계산
        Vector3[] corners = new Vector3[8];
        corners[0] = orizin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = orizin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = orizin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = orizin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = orizin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = orizin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = orizin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = orizin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;
        //시작점 박스의 12개 모서리를 그리기
        Debug.DrawLine(corners[0], corners[1], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[3], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[2], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[0], Color.green, debugDuration);
        Debug.DrawLine(corners[4], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[5], corners[7], Color.green, debugDuration);
        Debug.DrawLine(corners[7], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[6], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[0], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[7], Color.green, debugDuration);
    }
    void ToggleFlashLight()
    {
        StudySoundManager.Instance.PlaySFX("FlashLight",transform.position);
        isFlashLight = !isFlashLight;
        flashLight.enabled = isFlashLight;
    }
    public void ActivateRagdoll()
    {
        animator.enabled = false;
        SetRagdollState(true);
    }
    private void SetRagdollState(bool state)
    {
        foreach(Rigidbody body in ragdollbodies)
        {
            body.isKinematic = !state;
        }
        foreach(Collider collider in ragdollcolliders)
        {
            collider.enabled = state;
        }
    }
    IEnumerator CameraShake(float duration, float magnitude)
    {
        bloodImage.gameObject.SetActive(true);
        bloodImage.color = new Color(1, 0, 0, 0.1f);

        orizinalPos = mainCamera.transform.position;
        float elapsed = 0;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1, 1f) * magnitude;

            mainCamera.transform.position = new Vector3(x, y, 1f) + orizinalPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = orizinalPos;
        bloodImage.gameObject.SetActive(false);
    }
    IEnumerator WeaponReload()
    {
        int reloadBullet = 0;
        if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol &&
            pistolCurrentBulletCount != pistolMaxBulletCount && pistolBulletCount != 0)
        {
            isReloading = true;
            if (!isZoomed) animator.SetLayerWeight(1, 1);
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("PistolReload"))
            {
                Debug.Log("리로드중");
                animator.SetTrigger("WeaponReloadDont");
                isReloading = false;
                if (!isZoomed) animator.SetLayerWeight(1, 0);
                yield break;
            }
            animator.SetTrigger("WeaponReload");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);
            if(isReloading)
            {
                Debug.Log(animator.GetCurrentAnimatorStateInfo(1).length);
                reloadBullet = (pistolMaxBulletCount - pistolCurrentBulletCount);
                if(reloadBullet > pistolBulletCount)
                {
                    pistolCurrentBulletCount += pistolBulletCount;
                    pistolBulletCount = 0;
                }
                else
                {
                    pistolBulletCount -= reloadBullet;
                    pistolCurrentBulletCount = pistolMaxBulletCount;
                }
                isReloading = false;
                StudySoundManager.Instance.PlaySFX("WeaponReload", transform.position);

                if (!isZoomed) animator.SetLayerWeight(1, 0);
            }
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun &&
            shotGunCurrentBulletCount != shotGunMaxBulletCount && shotGunBulletCount != 0)
        {
            isReloading = true;
            if (!isZoomed) animator.SetLayerWeight(1, 1);
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("ShotGunReload"))
            {
                Debug.Log("리로드중");
                animator.SetTrigger("WeaponReloadDont");
                isReloading = false;
                if (!isZoomed) animator.SetLayerWeight(1, 0);
                yield break;
            }
            animator.SetTrigger("WeaponReload");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);
            if (isReloading)
            {
                Debug.Log(animator.GetCurrentAnimatorStateInfo(1).length);
                reloadBullet = (shotGunMaxBulletCount - shotGunCurrentBulletCount);
                if(reloadBullet > shotGunBulletCount)
                {
                    shotGunCurrentBulletCount += shotGunBulletCount;
                    shotGunBulletCount = 0;
                }
                else
                {
                    shotGunBulletCount -= reloadBullet;
                    shotGunCurrentBulletCount = shotGunMaxBulletCount;
                } 
                isReloading = false;
                StudySoundManager.Instance.PlaySFX("WeaponReload", transform.position);

                if (!isZoomed) animator.SetLayerWeight(1, 0);
            }
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle &&
            rifleCurrentBulletCount != rifleMaxBulletCount && rifleBulletCount != 0)
        {
            isReloading = true;
            if (!isZoomed) animator.SetLayerWeight(1, 1);
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("RifleReload"))
            {
                Debug.Log("리로드중");
                animator.SetTrigger("WeaponReloadDont");
                isReloading = false;
                if (!isZoomed) animator.SetLayerWeight(1, 0);
                yield break;
            }
            animator.SetTrigger("WeaponReload");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);
            if (isReloading)
            {
                Debug.Log(animator.GetCurrentAnimatorStateInfo(1).length);
                reloadBullet = (rifleMaxBulletCount - rifleCurrentBulletCount);
                if (reloadBullet > rifleBulletCount)
                {
                    rifleCurrentBulletCount += rifleBulletCount;
                    rifleBulletCount = 0;
                }
                else
                {
                    rifleBulletCount -= reloadBullet;
                    rifleCurrentBulletCount = rifleMaxBulletCount;
                }
                isReloading = false;
                StudySoundManager.Instance.PlaySFX("WeaponReload",transform.position);
                if (!isZoomed) animator.SetLayerWeight(1, 0);
            }
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        {

        }
    }
    void BulletCountText()
    {
        if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
        {
            bulletCountText.text = pistolCurrentBulletCount + "/" + pistolBulletCount;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
        {
            bulletCountText.text = shotGunCurrentBulletCount + "/" + shotGunBulletCount;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
        {
            bulletCountText.text = rifleCurrentBulletCount + "/" + rifleBulletCount;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        {

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Clear"))
        {
            StudySoundManager.Instance.PlayBGM("Clear");
            Cursor.lockState = CursorLockMode.Confined;
            isClear = true;
            clearImage.SetActive(true);
        }
    }
}
