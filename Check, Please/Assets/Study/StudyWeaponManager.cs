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
        public Weapon.WeaponType weaponType; //무기타입
        public Transform spawnPoint; //해당 무기의 생성 위치
    }

    public List<WeaponSpawnPoint> spawnPoints = new List<WeaponSpawnPoint>(); //인스펙터에서 관리할 스폰 리스트
    private Dictionary<Weapon.WeaponType, Transform> weaponSpawnPoints = new Dictionary<Weapon.WeaponType, Transform>();

    public List<GameObject> weaponPrefebs; //무기 프리펩 리스트
    private Dictionary<Weapon.WeaponType, GameObject> weaponInventory = new Dictionary<Weapon.WeaponType, GameObject>();

    private GameObject currentWeapon; //현재 장착된 무기
    private Weapon.WeaponType currentWeaponType = Weapon.WeaponType.None; //현재 무기 타입
    private Weapon currentWeaponComponent; //현재 무기의 Weapon 컴포넌트(EffectPos를 가져오기 위해 사용)


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
    public void EquipWeapon(Weapon.WeaponType weaponType) //무기를 장착하는 함수
    {
        if(!weaponInventory.ContainsKey(weaponType)) //예외처리
        {
            Debug.Log("무기가 인벤토리에 없습니다.");
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
        Debug.Log($"{weaponType} 무기 장착");
    }
    public void AddWeapon(GameObject weapon) //무기 획득 함수
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        SphereCollider sphereCollider = weaponComponent.GetComponent<SphereCollider>();
        sphereCollider.enabled = false;

        if (weaponComponent != null & !weaponInventory.ContainsKey(weaponComponent.weaponType))
        {
            weaponInventory.Add(weaponComponent.weaponType, weapon);
            Debug.Log($"{weaponComponent.weaponType} 무기 획득");
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
