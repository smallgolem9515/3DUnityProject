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
        Vector3 direction = targetCamera.transform.position - UIImage.position; //카메라와의 방향 계산
        direction.y = 0; //Y축 회전을 고정하여 UI가 위아래로 기울어지지 않도록 함
        Quaternion rotation = Quaternion.LookRotation(-direction); //UI가 카메라를 바라보도록 회전
        UIImage.rotation = rotation; //UIImage 회전 적용
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            UIImage.gameObject.SetActive(true);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            UIImage.gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            UIImage.gameObject.SetActive(false);
        }
    }
}
