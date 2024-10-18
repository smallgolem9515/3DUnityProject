using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    static public ParticleManager instance;

    public enum ParticleType
    {
        PistolEffect,
        ShotGunEffect,
        RifleEffect,
        SMGEffect,
        BloodEffect,
        RockImpactEffect,
        BrickImpactEffect
    }

    public Dictionary<ParticleType,GameObject> particleDic = new Dictionary<ParticleType,GameObject>();

    public GameObject pistolEffect;
    public GameObject shotGunEffect;
    public GameObject rifleEffect;
    public GameObject SMGEffect;
    public GameObject bloodEffect;
    public GameObject RockImpactEffect;
    public GameObject brickImpactEffect;

    GameObject particleObj;

    private void Awake()
    {
        if(instance == null )
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        particleDic.Add(ParticleType.PistolEffect, pistolEffect);
        particleDic.Add(ParticleType.ShotGunEffect, shotGunEffect);
        particleDic.Add(ParticleType.RifleEffect, rifleEffect);
        particleDic.Add(ParticleType.SMGEffect, SMGEffect);
        particleDic.Add(ParticleType.BloodEffect, bloodEffect);
        particleDic.Add(ParticleType.RockImpactEffect, RockImpactEffect);
        particleDic.Add(ParticleType.BrickImpactEffect,brickImpactEffect);
    }
    public void PlayParticle(ParticleType type, Vector3 position)
    {
        //싱글톤으로 플레이어의 위치를 가져옴
        Transform playerTransform = StudyPlayerManager.Instance.transform;

        //플레이어 방향을 기준으로 파티클이 회전하도록 설정
        Vector3 directionToPlayer = playerTransform.position - position;
        Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

        if(particleDic.ContainsKey(type))
        {
            particleObj = Instantiate(particleDic[type],position,Quaternion.identity);
            StartCoroutine(ParticleEnd(particleObj));
        }
    }
    IEnumerator ParticleEnd(GameObject particle)
    {
        particle.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        Destroy(particle);
    }
}
