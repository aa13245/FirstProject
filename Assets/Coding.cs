using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coding : MonoBehaviour
{
    // ���� �ΰ��� �޾Ƽ� ���� ���� ��ȯ�ϴ� �Լ�
    // ��ȯ�ڷ��� �Լ��̸�(�Ű�����, ...)
    int Sum(int number1, int number2)
    {
        // �ΰ��� �޾Ƽ� ���Ѵ�.
        int sum = number1 + number2;
        // ����� ��ȯ�ϰ� �ʹ�.
        return sum;
    } 

    // Start is called before the first frame update
    void Start()
    {
        // ���� �ΰ��� �޾Ƽ� ���� ���� ȭ�鿡 ����ϰ� �ʹ�.
        // 1. ���� �ΰ��� ���� ����� �ʿ��ϴ�.
        int number1 = 200;
        int number2 = 5;
        // 2. ȭ�鿡 ����� ����ϰ� �ʹ�.
        print("sum : " + Sum(number1, number2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
