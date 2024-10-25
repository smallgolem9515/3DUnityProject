using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    static public CrossHair instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private RectTransform crossHair;

    private float crossHairSize = 10; //CrossHairSize
    private float crossDefaltSize = 10; //CrossHair 기본 사이즈
    private float crossHairSpeed = 100; //CrossHair 확대 속도
    public float crossHairMaxSize = 500; //변경할 최대사이즈

    void Start()
    {
        crossHair = GetComponent<RectTransform>();
    }

    void Update()
    {
        crossHairSize = Mathf.Lerp(crossHairSize, crossDefaltSize, Time.deltaTime * 2);

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize); //크기 적용
    }
    public void WeaponCrossSpeed()
    {
        if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
        {
            crossHairMaxSize = 100;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
        {
            crossHairMaxSize = 200;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
        {
            crossHairMaxSize = 150;
        }
        else if (StudyWeaponManager.Instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        {
            crossHairMaxSize = 50;
        }
        crossHairSize = Mathf.Lerp(crossHairSize, crossHairMaxSize, Time.deltaTime * crossHairSpeed);
    }
}
