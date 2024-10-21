using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        None,Pistol, ShotGun, Rifle, SMG
    }
    public WeaponType weaponType;

    public Camera targetCamera;
    public Transform UIImage;

    public Transform effectPos;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        UIImage.gameObject.SetActive(false);
        if(effectPos == null)
        {
            effectPos = transform.GetChild(1);
        }
    }
    void Update()
    {
        Vector3 direction = targetCamera.transform.position - UIImage.position; //ī�޶���� ���� ���
        direction.y = 0; //Y�� ȸ���� �����Ͽ� UI�� ���Ʒ��� �������� �ʵ��� ��
        Quaternion rotation = Quaternion.LookRotation(-direction); //UI�� ī�޶� �ٶ󺸵��� ȸ��
        UIImage.rotation = rotation; //UIImage ȸ�� ����
    }
    private void OnTriggerEnter(Collider other)
    {
        UIImage.gameObject.SetActive(true);
    }
    private void OnTriggerStay(Collider other)
    {
        UIImage.gameObject.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        UIImage.gameObject.SetActive(false);
    }
}
