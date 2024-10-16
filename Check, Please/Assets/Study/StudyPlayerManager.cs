using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
    float playerSpeedWalk = 5f; //걷는 속도
    float playerSpeedRun = 10f; //달리기 속도
    public float currentSpeed = 1f; //변경 속도
    float gravity = -9.81f; //중력 - 기본적인 유니티에서의 중력
    Vector3 velocity; //현재 속도 저장
    CharacterController characterController; //리지드바디는 3D에서 이런저런 문제가 있어서 캐릭터 컨트롤러를 사용한다.
    //단 중력이 없어서 따로 설정해야한다.

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
    bool isZoomed = false; //확대 여부
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대/축소
    Camera mainCamera; //카메라 컴포넌트 

    float pitch = 0f; //위아래 회전값
    float yaw = 0f; //좌우 회전값
    bool isFirstPerson = false; //1인칭 모드 여부
    bool rotaterAroundPlayer = true; //카메라가 플레이어 주위를 회전하는 여부

    [Header("-----Jump-----")]
    public float jumpHeight = 2f; //점프 높이
    bool isGround; //땅에 충돌 여부
    public LayerMask groundLayer;

    Animator animator;
    float horizontal;
    float vertical;
    public Transform footPosition;
    RaycastHit hit;
    public Transform leftFoot;
    public Transform rightFoot;

    public float minMoveDistance = 0.01f;

    bool IsLeftFootGround = false;
    public bool IsRightFootGround = false;

    public float hp;

    Vector3 previousPosition;

    public float fallThreshold = -10f;
    bool IsGameOver = false;
    bool isJumping = false;
    Vector3 startPosition;
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
    }
    private void Update()
    {
        if (transform.position.y < fallThreshold && !IsGameOver)
        {
            GameOver();
        }
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if(!IsGameOver)
        {
            RunAction();

            bool isMoving = characterController.velocity.magnitude > 0.1f;
            Debug.DrawRay(footPosition.position, Vector3.down * 0.2f, Color.red);
            if (horizontal != 0 || vertical != 0)
            {
                //StartCoroutine(FootStepSound());
            }


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

            updateCameraRotation();
            if (isFirstPerson)
            {
                FirstPersonMovement(); //1인칭
            }
            else
            {
                ThirdPersonMovement(); //3인칭
            }
            ZoomMouseRightButton(); //오른쪽 마우스버튼으로 줌

            Jump(); //점프
            Fire();
        }
        

    }
    private void FixedUpdate()
    {
        
    }
    IEnumerator FootStepSound()
    {
        
        if (Physics.Raycast(footPosition.position, Vector3.down, out hit, 0.1f, groundLayer)) ;
        {
            if (hit.collider == null)
            {

            }
            else
            {
                
                if (hit.collider.tag == "Metal")
                {
                    StudySoundManager.Instance.PlaySFX("MetalFootStep");
                }
                else if (hit.collider.tag == "Snow")
                {
                    StudySoundManager.Instance.PlaySFX("SnowFootStep");
                }
                else
                {
                    StudySoundManager.Instance.PlaySFX("DefaltFootStep");
                }
            }

        }
        yield return new WaitForSeconds(1.0f);
    }
    void RunAction()
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
    void FirstPersonMovement() //1인칭
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        mainCamera.transform.position = playerHead.position;
    }
    void ThirdPersonMovement() //3인칭
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical + transform.up * velocity.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        if(rotaterAroundPlayer)
        {
            UpdateCameraPosition();
        }
        else
        {
            UpdateCameraPositionRotater();    
        }
    }
    void updateCameraRotation()//카메라 회전
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; //마우스값을 가져오고 민감도와 시간을 곱한다.
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
    void UpdateCameraPositionRotater()//카메라 위치 오른쪽에 있음
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        //mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
        //카메라의 위치가 자기 자신에서 살짝위 + 방향을 바꿔가며 + 살짝 뒤에서

        //카메라가 플레이어의 위치를 따라가도록 설정
        cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
    }
    void UpdateCameraPosition()// 카메라 위치 
    {   
        Vector3 direction = new Vector3(0,0, -currentDistance); //카메라 거리 설정
        Quaternion rotation = Quaternion.Euler(pitch,yaw, 0);

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
        if(isGround && isJumping && jumpUpCompleted)
        {
            animator.SetTrigger("JumpDown");
            isJumping = false;
        }
        if (Input.GetButtonDown("Jump") && isGround)
        {
            StudySoundManager.Instance.PlaySFX("Jump");
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
    void ZoomMouseRightButton()// 마우스 오른쪽 버튼으로 줌
    {
        if (Input.GetMouseButtonDown(1) && !isZoomed) //오른쪽 마우스 버튼을 눌렀을 때
        {
            isZoomed = true;
            animator.SetLayerWeight(1, 1);

            animator.Play("PistolAim"); //디폴트지만 만에 하나를 위해
            
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
    void Fire()
    {
        if (Input.GetMouseButtonDown(0) && isZoomed)
        {
            animator.SetTrigger("Fire");
        }
    }
    IEnumerator ZoomCamera(float targetDistance)
    {
        while(Mathf.Abs(currentDistance-targetDistance) > 0.01f)
        {
            currentDistance = Mathf.Lerp(currentDistance,targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        currentDistance = targetDistance; //목표거리에 도달한후 값을 고정
    }
    IEnumerator ZoomFieldOfView(float targetDistance)
    {
        while(Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView,targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }

    void GameOver()
    {
        IsGameOver = true;
        transform.position = startPosition;
        Invoke("RestartGame", 2.0f);
    }
    void RestartGame()
    {
        transform.position = new Vector3(startPosition.x,startPosition.y,startPosition.z);
        Debug.Log(startPosition);
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TakeDamage(float damage)
    {
        hp -= damage;
        hp = Mathf.Max(hp, 0);
        if(hp <= 0)
        {
            GameOver();
        }
    }
}
