using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Moving,             //�̵� ��ֹ�
    Rotating,           //ȸ�� ��ֹ�
    Visivility,         //���� ��ֹ�
    ShrinkingPlatform,  //ũ�Ⱑ ���� �پ��� ��ֹ�
    DamagingPlatform    //�������� �Դ� ��ֹ�
}
public class StudyObstacleManager : MonoBehaviour
{
    public ObstacleType obstacleType;

    public List<Transform> points; //�̵� ��� ����Ʈ
    public float moveSpeed = 2.0f; //�̵� �ӵ�
    private int currentPointIndex = 0; //���� ��ǥ ���� �ε���

    public Vector3 rotationAxis = Vector3.up; //ȸ���� ������ �ν����Ϳ��� ���Ѵ�.
    public float rotationSpeed = 50.0f; //ȸ�� �ӵ�

    private Renderer objectRenderer;
    public bool isVisivle = true;

    public float shrinkRate = 0.1f; //�� ������ ũ�� ���� ����
    private Vector3 initialScale; //�ʱ� ũ��

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

        // ���� ��ǥ ����
        Transform targetPoint = points[currentPointIndex];

        // ���� ��ġ���� ��ǥ �������� �̵�
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        //��ǥ ������ �����ϸ� ���� �������� �ε����� ��ȯ
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
