using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : MonoBehaviour
{
    public bool isOpen = false; //문이 열려 있는 상태를 나타내는 변수
    private Animator animator;
    public bool lastOpenForward = true; //마지막 문이 정방향으로 열렸는지 여부

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    bool IsPlayerInFront(Transform player)
    {
        //플레이어와 문 사이의 벡터값 계산
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //문이 향하는 방향과 플레이어 방향을 비교(내적 연산합니다.)
        float doProduct = Vector3.Dot(toPlayer, transform.position);
        //doProduct > 0 이면 플레이어가 문 앞에 있음
        return doProduct > 0;
    }
    public bool Open(Transform player)
    {
        if (!isOpen)
        {
            StudySoundManager.Instance.PlaySFX("DoorOpen",transform.position);
            //플레이어가 앞에 있으면 정방향 애니메이션 재생, 뒤에 있으면 역방향 애니메이션 재생
            if (IsPlayerInFront(player))
            {
                animator.SetTrigger("OpenForward"); //정방향 애니메이션
                lastOpenForward = true;
            }
            else
            {
                animator.SetTrigger("OpenBackward"); //역방향 애니메이션
                lastOpenForward = false;
            }
            isOpen = true;
            return true;
        }
        return false;
    }
    public void CloseForward(Transform player)
    {
        if(isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseForward");
        }
    }
    public void CloseBackward(Transform player)
    {
        if (isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseBackward");
        }
    }
}
