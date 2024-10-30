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
    float playerSpeedWalk = 3f; //�ȴ� �ӵ�
    float playerSpeedRun = 5f; //�޸��� �ӵ�
    public float currentSpeed = 1f; //���� �ӵ�
    public float hp;
    float gravity = -9.81f; //�߷� - �⺻���� ����Ƽ������ �߷�
    Vector3 velocity; //���� �ӵ� ����
    CharacterController characterController; //������ٵ�� 3D���� �̷����� ������ �־ ĳ���� ��Ʈ�ѷ��� ����Ѵ�.
    //�� �߷��� ��� ���� �����ؾ��Ѵ�.
    public float damageDelay = 2.0f;

    [Header("-----PlayerBool-----")]
    public bool isFire = true;
    public bool isDamage = false;
    bool isGameOver = false;
    bool isJumping = false;

    [Header("-----Camera-----")]
    public Transform cameraTransform; //ī�޶� Transform
    public Transform playerHead; //�÷��̾� �Ӹ� ��ġ(1��Ī ����϶� ���)
    public float thirdPersonDistance = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶� �þ� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); //3��Ī ��忡�� ī�޶������
    float mouseSensitivity = 100f; //���콺 ����
    public Transform playerLookObj; //�÷��̾��� �þ� ��ġ

    [Header("-----Zoom-----")]
    public float zoomedDistance = 1.0f; //ī�޶� Ȯ��� ���� �Ÿ�(3��Ī ����϶� ���)
    public float zoomSpeed = 5f; //Ȯ�� ��Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60f; //�⺻ ī�޶� �þ߰�
    public float zoomedFov = 30f; //Ȯ��� ī�޶� �þ߰� 

    float currentDistance; //���� ī�޶���� �Ÿ�
    float targetDistance; //��ǥ ī�޶� �Ÿ�
    float targetFov; //��ǥ Fov
    public bool isZoomed = false; //Ȯ�� ����
    private Coroutine zoomCoroutine; //�ڷ�ƾ�� ����Ͽ� Ȯ��/���
    Camera mainCamera; //ī�޶� ������Ʈ 

    float pitch = 0f; //���Ʒ� ȸ����
    float yaw = 0f; //�¿� ȸ����
    bool isFirstPerson = false; //1��Ī ��� ����
    bool rotaterAroundPlayer = false; //ī�޶� �÷��̾� ������ ȸ���ϴ� ����

    [Header("-----Jump-----")]
    public float jumpHeight = 2f; //���� ����
    bool isGround; //���� �浹 ����
    public LayerMask groundLayer; //���� ���̾�

    Animator animator;
    float horizontal; //X�� �̵�
    float vertical; //Z�� �̵�
    [Header("-----FootSoundSetting-----")]
    public Transform leftFoot; //���� ��
    public Transform rightFoot; //������ ��
    bool isLeftFootGround = false;
    bool isRightFootGround = false;

    Vector3 previousPosition;
    public float fallThreshold = -10f;
    Vector3 startPosition;

    [Header("-----UpperBody-----")]
    public Transform upperBody; //��ü ���� �Ҵ�(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //��ü Aim ��忡���� ȸ���� �Ѵ�.
    private quaternion originalUpperBodyRotation; //���� ��ü ȸ�� ��

    public Transform aimTarget;
    [Header("-----SlowMotion-----")]
    public float slowMotionScale = 0.5f;
    private float defaltTimeScale = 1f;
    private bool isSlowMotion = false;

    [Header("-----ShotRayCast-----")]
    public float maxShotDistance = 100f;
    public LayerMask targetLayer;

    [Header("-----WeaponSetting-----")]
    public float recoilStrength = 2f; //�ݵ��� ����
    public float maxRecoilAngle = 10.0f; //�ݵ��� �ִ� ����
    private float currentRecoil = 0f; //���� �ݵ� ���� �����ϴ� ����
    private int shotGunRayCount = 5; //������ �Ѿ��� ������ ��
    private float shotGunSpreadAngle = 5f; //������ �Ѿ��� ������ ����

    //BoxCast ����
    [Header("-----BoxCast-----")]
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5f; //BoxCast �ָ� ���� �Ÿ�
    public LayerMask itemLayer; //������ ���̾�
    public Transform itemGetPos; //BoxCast ��ġ
    public float debugDuration = 2.0f; //����� ���� ����

    public GameObject crossHair;
    public GameObject noneCrossHair;

    private float fireDelay = 0.1f; //�߻� ������

    public Light flashLight;
    private bool isFlashLight = false;

    private Rigidbody[] ragdollbodies;
    private Collider[] ragdollcolliders;

    private bool lastOpenedForward = true; //���������� ���� ���������� ���ȴ��� ����
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

        UnityEngine.Cursor.lockState = CursorLockMode.Locked; //Ŀ�� �Ⱥ��̰� ����
        currentDistance = thirdPersonDistance; //�ʱ� ī�޶� �Ÿ��� ����
        targetDistance = thirdPersonDistance; //��ǥ ī�޶� �Ÿ� ����
        targetFov = defaultFov; //�ʱ� Fov ����
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov; //�⺻ Fov����

        currentSpeed = playerSpeedWalk;
        startPosition = transform.position + Vector3.up;
        previousPosition = transform.position; //���۽� ���� ��ġ�� ���� ��ġ�� �ʱ�ȭ
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
            //���� ��ġ�� ���� ��ġ�� ����(���� �����ӿ��� ���ϱ�����)
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
                    FirstPersonMovement(); //1��Ī
                }
                else
                {
                    ThirdPersonMovement(); //3��Ī
                }
                RunAction();
                Jump(); //����

                BulletCountText();

                FireWeapon();
                ChangeWeapon();
                ZoomMouseRightButton(); //������ ���콺��ư���� ��
                if (Input.GetKeyDown(KeyCode.V)) //VŰ�� 1��Ī/3��Ī ��ȯ
                {
                    isFirstPerson = !isFirstPerson;
                    Debug.Log(isFirstPerson ? "1��Ī���" : "3��Ī���");
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson) //AltŰ�� ���� ��ȯ
                {
                    rotaterAroundPlayer = !rotaterAroundPlayer;
                    Debug.Log(rotaterAroundPlayer ? "ī�޶� �÷��̾� ������ ȸ��" : "�÷��̾ ���� ȸ��");
                }
                if (currentRecoil > 0) //�ݵ����� ������ ��߳��� ���� �������� ���ƿ��� ���
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
    void UpdateAimTarget() //�÷��̾ �ٶ󺸴� ����
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        aimTarget.position = ray.GetPoint(10);
    }
    void FirstPersonMovement() //1��Ī
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical + transform.up * velocity.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        mainCamera.transform.position = playerHead.position;
    }
    void ThirdPersonMovement() //3��Ī
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
    void UpdateCameraRotation()//ī�޶� ȸ��
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; //���콺���� �������� �ΰ����� �ð��� ���Ѵ�.
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -45f, 45f);

        mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
    void UpdateCameraPosition()//�÷��̾ ���� ȸ��
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        //mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
        //ī�޶��� ��ġ�� �ڱ� �ڽſ��� ��¦�� + ������ �ٲ㰡�� + ��¦ �ڿ���

        //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
        cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y + currentRecoil, 0));

        UpdateAimTarget();
    }
    void UpdateCameraPositionRotater()// ī�޶� �÷��̾� ������ ȸ�� 
    {
        Vector3 direction = new Vector3(0, 0, -currentDistance); //ī�޶� �Ÿ� ����
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
        cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;
        //ī�޶��� ��ġ�� �ڱ� �ڽſ��� ��¦�� + ������ �ٲ㰡�� + ��¦ �ڿ���

        //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
        cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }
    void Jump()// ���� �Լ�
    {
        isGround = CheckIfGround();
        // "JumpUp" �ִϸ��̼��� �Ϸ�Ǿ����� Ȯ�� (normalizedTime >= 1)
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
    bool CheckIfGround()// ����ĳ��Ʈ�� �ٴ� Ȯ��
    {
        // ĳ���� �� �Ʒ��� �߽� ��ġ ���� (�ణ ���� �÷��� ����)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast�� ũ�� ���� (ĳ������ �� ũ��� �°� ����)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast �߻�
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, quaternion.identity, 0.2f, groundLayer);

        // ������ BoxCast �ð�ȭ
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
    void RunAction() //�ٴ� ���
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        if (Input.GetKey(KeyCode.LeftShift)) //�ִϸ��̼��� ���� �޸��� ����
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
    void FootStepSound() //�߼Ҹ�
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
        //���� ���¸� ���� �����Ӱ� ���ϱ� ���� ����
        isLeftFootGround = leftHit;
        isRightFootGround = rightHit;
    }
    bool CheckGround(Transform foot) //���� ���� ��Ҵ��� üũ
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }
    void ZoomMouseRightButton()// ���콺 ������ ��ư���� ��
    {
        if (Input.GetMouseButtonDown(1) && !isZoomed) //������ ���콺 ��ư�� ������ ��
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
        currentDistance = targetDistance; //��ǥ�Ÿ��� �������� ���� ����
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
    void FireWeapon() //�ѿ� ���� �޶������ϴ� ���
    {
        if (Input.GetMouseButtonDown(0) && isZoomed && isFire && !isReloading)
        {
            StartCoroutine(ShotDelay());
        }
    }
    void ApplyRecoil() //�ݵ� ���
    {
        //���� ī�޶��� ���� ȸ���� ������
        Quaternion currentRotation = Camera.main.transform.rotation;

        //�ݵ� ���� ������Ŵ
        currentRecoil += recoilStrength;

        //�ݵ� ���� MAX�� ���缭 ����
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);

        //�ݵ��� ����Ͽ� X��(����) ȸ���� �߰�(���� �ö󰡴� �ݵ�)
        Quaternion recoilRotation = quaternion.Euler(-currentRecoil, 0, 0);

        //���� ȸ�� ���� �ݵ��� ���Ͽ� ���ο� ȸ������ ����
        //Camera.main.transform.rotation = currentRotation * recoilRotation;
        Debug.Log(currentRecoil);
    }
    IEnumerator ShotDelay() //�� ��� �� ������
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
    void AimWeapon() //������ ���� ������ �޶����� ���
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
    void ChangeWeapon() //1,2,3,4�� ���� ����
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
    void GameOver() // ���ӿ���
    {
        isGameOver = true;
        dieImage.SetActive(true);
        Invoke(nameof(RestartGame), 2.0f);
    }
    void RestartGame() //����ŸƮ
    {
        //animator.enabled = true;
        //SetRagdollState(false);
        //transform.position = startPosition;
        //isGameOver = false;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene("Menu");
    }
    public void TakeDamage(float damage) //�������� �Դ� ���
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
    void ToggleSlowMotion() //���ο��� ���
    {
        if (!isSlowMotion)
        {
            Time.timeScale = defaltTimeScale;
            currentSpeed /= 2;
            Debug.Log("���ο� ��� ����");
        }
        else
        {
            Time.timeScale = slowMotionScale;
            currentSpeed *= 2;
            Debug.Log("���ο� ��� Ȱ��ȭ");
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
                Debug.Log($"������ ���� : {item.name}");
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
        //BoxCast�� ����� �׸��� ���� 8���� �𼭸� ��ǥ ���
        Vector3[] corners = new Vector3[8];
        corners[0] = orizin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = orizin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = orizin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = orizin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = orizin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = orizin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = orizin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = orizin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;
        //������ �ڽ��� 12�� �𼭸��� �׸���
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
                Debug.Log("���ε���");
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
                Debug.Log("���ε���");
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
                Debug.Log("���ε���");
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
