using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyWeaponManager : MonoBehaviour
{
    public static StudyWeaponManager Instance;

    [Serializable]
    public class WeaponSpawnPoint
    {
        public Weapon.WeaponType weaponType; //����Ÿ��
        public Transform spawnPoint; //�ش� ������ ���� ��ġ
    }

    public List<WeaponSpawnPoint> spawnPoints = new List<WeaponSpawnPoint>(); //�ν����Ϳ��� ������ ���� ����Ʈ
    private Dictionary<Weapon.WeaponType, Transform> weaponSpawnPoints = new Dictionary<Weapon.WeaponType, Transform>();

    public List<GameObject> weaponPrefebs; //���� ������ ����Ʈ
    private Dictionary<Weapon.WeaponType, GameObject> weaponInventory = new Dictionary<Weapon.WeaponType, GameObject>();

    private GameObject currentWeapon; //���� ������ ����
    private Weapon.WeaponType currentWeaponType = Weapon.WeaponType.None; //���� ���� Ÿ��
    private Weapon currentWeaponComponent; //���� ������ Weapon ������Ʈ(EffectPos�� �������� ���� ���)


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        foreach(var spawnPoint in spawnPoints)
        {
            if(!weaponSpawnPoints.ContainsKey(spawnPoint.weaponType))
            {
                weaponSpawnPoints.Add(spawnPoint.weaponType,spawnPoint.spawnPoint);
            }
        }
    }
    public void EquipWeapon(Weapon.WeaponType weaponType) //���⸦ �����ϴ� �Լ�
    {
        if(!weaponInventory.ContainsKey(weaponType)) //����ó��
        {
            Debug.Log("���Ⱑ �κ��丮�� �����ϴ�.");
        }

        foreach(Transform child in weaponSpawnPoints[weaponType])
        {
            Destroy(child.gameObject);
        }
        GameObject newWeapon = Instantiate(weaponInventory[weaponType], weaponSpawnPoints[weaponType]);

        newWeapon.transform.localPosition = Vector3.zero;

        currentWeapon = newWeapon;
        currentWeaponType = weaponType;

        currentWeaponComponent = newWeapon.GetComponent<Weapon>();

        currentWeapon.SetActive(true);
        Debug.Log($"{weaponType} ���� ����");
    }
    public void AddWeapon(GameObject weapon) //���� ȹ�� �Լ�
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        SphereCollider sphereCollider = weaponComponent.GetComponent<SphereCollider>();
        sphereCollider.enabled = false;

        if (weaponComponent != null & !weaponInventory.ContainsKey(weaponComponent.weaponType))
        {
            weaponInventory.Add(weaponComponent.weaponType, weapon);
            Debug.Log($"{weaponComponent.weaponType} ���� ȹ��");
        }
    }
    public Weapon.WeaponType GetCurrentWeaponType()
    {
        return currentWeaponType;
    }
    public Weapon GetCurrentWeaponComponent()
    {
        return currentWeaponComponent;
    }
}
