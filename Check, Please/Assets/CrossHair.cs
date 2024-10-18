using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    private RectTransform crossHair;

    private float crossHairSize = 10; //CrossHairSize
    private float crossDefaltSize = 10; //CrossHair �⺻ ������
    private float crossHairSpeed = 100; //CrossHair Ȯ�� �ӵ�
    public float crossHairMaxSize = 500; //������ �ִ������

    void Start()
    {
        crossHair = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //���� ��鿡 ���� ����
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairMaxSize, Time.deltaTime * crossHairSpeed);
        }
        else
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossDefaltSize, Time.deltaTime * 2);
        }

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize); //ũ�� ����
    }
}
