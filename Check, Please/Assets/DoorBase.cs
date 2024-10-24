using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : MonoBehaviour
{
    public bool isOpen = false; //���� ���� �ִ� ���¸� ��Ÿ���� ����
    private Animator animator;
    public bool lastOpenForward = true; //������ ���� ���������� ���ȴ��� ����

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    bool IsPlayerInFront(Transform player)
    {
        //�÷��̾�� �� ������ ���Ͱ� ���
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //���� ���ϴ� ����� �÷��̾� ������ ��(���� �����մϴ�.)
        float doProduct = Vector3.Dot(toPlayer, transform.position);
        //doProduct > 0 �̸� �÷��̾ �� �տ� ����
        return doProduct > 0;
    }
    public bool Open(Transform player)
    {
        if (!isOpen)
        {
            StudySoundManager.Instance.PlaySFX("DoorOpen",transform.position);
            //�÷��̾ �տ� ������ ������ �ִϸ��̼� ���, �ڿ� ������ ������ �ִϸ��̼� ���
            if (IsPlayerInFront(player))
            {
                animator.SetTrigger("OpenForward"); //������ �ִϸ��̼�
                lastOpenForward = true;
            }
            else
            {
                animator.SetTrigger("OpenBackward"); //������ �ִϸ��̼�
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
