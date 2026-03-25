using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // 현재 각도
    Vector3 angle;
    // 마우스 감도
    public float sensitivity = 200f;

    void Start()
    {
        // 시작할 때 현재 카메라의 각도를 적용한다.
        angle.x = -Camera.main.transform.eulerAngles.x;
        angle.y = Camera.main.transform.eulerAngles.y;
        angle.z = Camera.main.transform.eulerAngles.z;    
    }

    void Update()
    {
        // 마우스 입력에 따라 카메라를 회전시키고 싶다.
        // 1. 사용자의 마우스 입력을 얻어오기
        // 마우스의 좌우 입력 받기
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        // 2. 방향
        // 이동 공식에 대입해 각 속성 별로 회전 값을 누적
        angle.x += x * sensitivity * Time.deltaTime;
        angle.y += y * sensitivity * Time.deltaTime;

        // 3. 회전
        // 카메라의 회전 값에 새로 만들어진 회전 값을 할당
        transform.eulerAngles = new Vector3(-angle.y, angle.x, transform.eulerAngles.z);    
    }
}
