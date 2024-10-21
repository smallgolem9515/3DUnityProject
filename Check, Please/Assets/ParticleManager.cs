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

    private Dictionary<ParticleType,Queue<GameObject>> particlePools = new Dictionary<ParticleType,Queue<GameObject>>();

    public GameObject pistolEffect;
    public GameObject shotGunEffect;
    public GameObject rifleEffect;
    public GameObject SMGEffect;
    public GameObject bloodEffect;
    public GameObject RockImpactEffect;
    public GameObject brickImpactEffect;

    GameObject particleObj;
    public int poolSize = 20;

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
    private void Start()
    {
        foreach(var particleType in particleDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>(); //Queue : FIFO �����͸� ó���ϴ� �ڷᱸ��
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleDic[particleType]);
                obj.SetActive(false);
                pool.Enqueue(obj); //Enqueue : Queue�� �߰��ϴ� �Լ�
            }
            particlePools.Add(particleType, pool);
        }
    }
    public void PlayParticle(ParticleType type, Vector3 position)
    {
        if(particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if (particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if(particleSystem == null)
                {
                    return;
                }
                if(particleSystem.isPlaying)
                {
                    particleSystem.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                    //��ƼŬ �ý��� ������ �����ϰ�, ������ ����� ��� ��ƼŬ�� �����մϴ�.
                }
                particleObj.SetActive(true);
                particleSystem.Play();

                StartCoroutine(ParticleEnd(type, particleObj, particleSystem));
            }

            ////�̱������� �÷��̾��� ��ġ�� ������
            //Transform playerTransform = StudyPlayerManager.Instance.transform;

            ////�÷��̾� ������ �������� ��ƼŬ�� ȸ���ϵ��� ����
            //Vector3 directionToPlayer = playerTransform.position - position;
            //Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

            //if(particleDic.ContainsKey(type))
            //{
            //    particleObj = Instantiate(particleDic[type],position,rotation);
            //    StartCoroutine(ParticleEnd(particleObj));
        }
    }
    IEnumerator ParticleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while(particleSystem.isPlaying)
        {
            yield return null;
        }
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj); //EnQueue : �����͸� Queue�� �߰��ϴ� �Լ� ���ο� ��Ҹ� ���� �߰�
        //particle.SetActive(true);
        //yield return new WaitForSeconds(1.0f);

        //Destroy(particle);
    }
}
