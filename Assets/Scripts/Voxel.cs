using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 1. 복셀은 랜덤한 방향으로 날아가는 운동을 한다.
// 필요속성: 날아갈 속도
public class Voxel : MonoBehaviour
{
    // 1. 복셀이 날아갈 속도 속성
    public float speed = 5f;
    // 복셀을 제거할 시간
    public float destroyTime = 3.0f;
    // 경과 시간
    private float currentTime;

    void OnEnable()
    {
        currentTime = 0f;
        // 2. 랜덤한 방향 찾기
        Vector3 direction = Random.insideUnitSphere;
        // 3. 랜덤한 방향으로 날아가는 속도 부여
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = direction * speed;    
    }

    void Update()
    {
        // 일정 시간이 지나면 복셀 제거
        // 1. 시간 경과
        currentTime += Time.deltaTime;
        // 2. 제거 시간 도달
        if (currentTime > destroyTime)
        {
            // 3. Voxel을 비활성화 시킴
            gameObject.SetActive(false);
            // 4. 오브젝트 풀에 반환
            VoxelMaker.voxelPool.Add(gameObject);
        }    
    }
}
