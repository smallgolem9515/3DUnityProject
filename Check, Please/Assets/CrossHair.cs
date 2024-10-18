using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
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
        if (Input.GetMouseButtonDown(0)) //총을 쏘면에 대한 조건
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairMaxSize, Time.deltaTime * crossHairSpeed);
        }
        else
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossDefaltSize, Time.deltaTime * 2);
        }

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize); //크기 적용
    }
}
