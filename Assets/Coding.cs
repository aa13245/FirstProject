using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coding : MonoBehaviour
{
    // 숫자 두개를 받아서 더한 값을 반환하는 함수
    // 반환자료형 함수이름(매개변수, ...)
    int Sum(int number1, int number2)
    {
        // 두개를 받아서 더한다.
        int sum = number1 + number2;
        // 결과를 반환하고 싶다.
        return sum;
    } 

    // Start is called before the first frame update
    void Start()
    {
        // 숫자 두개를 받아서 더한 값을 화면에 출력하고 싶다.
        // 1. 숫자 두개를 더한 결과가 필요하다.
        int number1 = 200;
        int number2 = 5;
        // 2. 화면에 결과를 출력하고 싶다.
        print("sum : " + Sum(number1, number2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
