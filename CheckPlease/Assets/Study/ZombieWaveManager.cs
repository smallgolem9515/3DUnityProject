using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieWaveManager : MonoBehaviour
{
    public GameObject[] zombiePrefebs;
    public GameObject[] zombieWave1 = new GameObject[5];
    public GameObject objClear1;

    private void Start()
    {
        StartCoroutine(WaveClear());
    }
    IEnumerator WaveClear()
    {
        while (true)
        {
            for (int i = 0; i < zombieWave1.Length; i++)
            {
                if (zombieWave1[i].activeSelf == true)
                {
                    break;
                }
                StudySoundManager.Instance.PlaySFX("OpenCarGo", objClear1.transform.position);
                objClear1.SetActive(false);
                
            }
            yield return new WaitForSeconds(1);
            if(objClear1.activeSelf == false)
            {
                break;
            }
        }
        
    }
}
