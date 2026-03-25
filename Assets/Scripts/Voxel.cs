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

    // ── [추가] 3D Painter 모드 플래그 ──────────────────────────
    // true : 생성 위치에 고정 (그림 그리기 모드)
    // false: 기존처럼 랜덤 방향으로 날아감
    public bool painterMode = true;

    void OnEnable()
    {
        currentTime = 0f;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (painterMode)
            {
                // ── [수정] velocity/angularVelocity는 isKinematic = true 이전에 설정해야 오류 없음 ──
                rb.isKinematic     = false;   // 잠깐 false로 열어서 velocity 초기화 허용
                rb.velocity        = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic     = true;    // 이후 kinematic으로 완전 고정
            }
            else
            {
                // 기존: 랜덤한 방향으로 날아가는 속도 부여
                // 2. 랜덤한 방향 찾기
                Vector3 direction = Random.insideUnitSphere;
                // 3. 랜덤한 방향으로 날아가는 속도 부여
                rb.isKinematic = false;
                rb.velocity    = direction * speed;
            }
        }
    }

    void Update()
    {
        // ── [추가] 3D Painter 모드: 자동 제거 하지 않음 ──────────
        if (painterMode) return;

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

    // ── [추가] 색상과 크기를 외부에서 설정하는 메서드 ──────────
    public void SetColorAndSize(Color color, float size)
    {
        transform.localScale = Vector3.one * size;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = rend.material;
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
        }
    }
}