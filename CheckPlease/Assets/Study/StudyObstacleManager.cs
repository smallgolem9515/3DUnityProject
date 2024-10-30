using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Moving,             //이동 장애물
    Rotating,           //회전 장애물
    Visivility,         //투명 장애물
    ShrinkingPlatform,  //크기가 점점 줄어드는 장애물
    DamagingPlatform    //데미지를 입는 장애물
}
public class StudyObstacleManager : MonoBehaviour
{
    public ObstacleType obstacleType;

    public List<Transform> points; //이동 경로 리스트
    public float moveSpeed = 2.0f; //이동 속도
    private int currentPointIndex = 0; //현재 목표 지점 인덱스

    public Vector3 rotationAxis = Vector3.up; //회전할 방향을 인스펙터에서 정한다.
    public float rotationSpeed = 50.0f; //회전 속도

    private Renderer objectRenderer;
    public bool isVisivle = true;

    public float shrinkRate = 0.1f; //매 프레임 크기 감소 비율
    private Vector3 initialScale; //초기 크기

    public float damageRate = 10.0f;

    Vector3 previousPosition;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if(points.Count > 0)
        {
            transform.position = points[0].position;
        }

        initialScale = transform.localScale;
    }
    void Update()
    {
        switch (obstacleType)
        {
            case ObstacleType.Moving:
                Movement();
                break;
            case ObstacleType.Rotating:
                Rotation();
                break;
            case ObstacleType.Visivility:
                Visivleility();
                break;
        }
        previousPosition = transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            switch(obstacleType)
            {
                case ObstacleType.ShrinkingPlatform:
                    ShrinkingPlatform(); 
                    break;
                case ObstacleType.DamagingPlatform:
                    DamagingPlatform();
                    break;
                    
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(collision.gameObject.name);
            switch (obstacleType)
            {
                case ObstacleType.Moving:
                    MovingPlayer();
                    break;

            }
        }
    }
    void Movement()
    {
        if (points.Count == 0) return;

        // 현재 목표 지점
        Transform targetPoint = points[currentPointIndex];

        // 현재 위치에서 목표 지점으로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        //목표 지점에 도달하면 다음 지점으로 인덱스를 순환
        if (transform.position == targetPoint.position)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Count;
        }
    }
    void MovingPlayer()
    {
        StudyPlayerManager.Instance.transform.parent.position = previousPosition;
        
    }
    void Rotation()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
    public void Visivleility()
    {
        objectRenderer.enabled = isVisivle;
    }
    void ShrinkingPlatform()
    {
        if(transform.localScale.x > 0.1f)
        {
            transform.localScale -= Vector3.one * shrinkRate;
        }
    }
    void DamagingPlatform()
    {
        StudyPlayerManager.Instance.TakeDamage(damageRate);
    }
}
