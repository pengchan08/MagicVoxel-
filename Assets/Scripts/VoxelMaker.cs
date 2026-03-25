using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMaker : MonoBehaviour
{
    public GameObject voxelFactory;
    public int voxelPoolSize = 200;           // [변경] 그림을 그리려면 풀이 충분해야 함

    // 오브젝트 풀
    public static List<GameObject> voxelPool = new List<GameObject>();

    // 생성 시간
    public float createTime = 0.00008f;         // [변경] 0.02 → 0.008: 더 촘촘하게
    // 경과 시간
    float currentTime = 0f;

    // 크로스헤어 변수
    public Transform crosshair;

    // ── [추가] 색상 목록 (Y버튼으로 순환) ──────────────────────
    public Color[] paintColors = {
        Color.red,
        new Color(1f, 0.5f, 0f),  // Orange
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.blue,
        new Color(0.5f, 0f, 1f),  // Violet
        Color.white,
        Color.black
    };
    private int colorIndex = 0;

    // ── [추가] 크기 목록 (X버튼으로 순환) ──────────────────────
    public float[] paintSizes = { 0.02f, 0.05f, 0.10f, 0.18f, 0.30f };
    private int sizeIndex = 1;

    // ── [추가] 버튼 엣지 감지용 이전 상태 ──────────────────────
    private bool prevYButton = false;
    private bool prevXButton = false;

    // ── [추가] 거리 기반 촘촘함: 마지막 생성 위치 ──────────────
    private Vector3 lastPaintPosition = Vector3.positiveInfinity;
    // 이전 점과 최소 이 거리 이상 떨어져야 새 점 생성 (브러시 크기에 비례)
    public float minDistanceRatio = 0.6f;

    // ── [추가] 하늘 페인팅: Raycast miss 시 배치할 거리 ─────────
    public float skyPaintDistance = 3.0f;


    void Start()
    {
        for (int i = 0; i < voxelPoolSize; i++)
        {
            GameObject voxel = Instantiate(voxelFactory);
            voxel.SetActive(false);
            voxelPool.Add(voxel);
        }
    }

    void Update()
    {
        // 크로스헤어 그리기
        ARAVRInput.DrawCrosshair(crosshair);

        // ── [추가] 왼쪽 Y버튼 → 색상 순환 ─────────────────────
        bool yButton = ARAVRInput.Get(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch);
        if (yButton && !prevYButton)
        {
            colorIndex = (colorIndex + 1) % paintColors.Length;
            Debug.Log($"[VoxelMaker] Color → {paintColors[colorIndex]}");
        }
        prevYButton = yButton;

        // ── [추가] 왼쪽 X버튼 → 크기 순환 ─────────────────────
        bool xButton = ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);
        if (xButton && !prevXButton)
        {
            sizeIndex = (sizeIndex + 1) % paintSizes.Length;
            Debug.Log($"[VoxelMaker] Size → {paintSizes[sizeIndex]}");
        }
        prevXButton = xButton;

        // 사용자가 마우스를 클릭한 지점에 복셀 1개 생성
        // 1. 사용자 마우스 클릭
        //if (Input.GetButtonDown("Fire1"))
        if (ARAVRInput.Get(ARAVRInput.Button.One))
        {
            currentTime += Time.deltaTime;
            if (currentTime > createTime)
            {
                // 2. 바닥 위치 확인
                // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // 2) 컨트롤러가 향하는 방향으로 시선 만들기
                Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
                RaycastHit hitInfo;

                Vector3 spawnPos;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    // 표면에 닿았을 때: 표면 위에 살짝 띄워서 배치 (z-fighting 방지)
                    spawnPos = hitInfo.point + hitInfo.normal * (paintSizes[sizeIndex] * 0.5f);
                }
                else
                {
                    // ── [추가] 하늘/허공: Raycast miss → 컨트롤러 앞 일정 거리에 배치 ──
                    spawnPos = ray.origin + ray.direction * skyPaintDistance;
                }

                // ── [추가] 거리 기반 필터: 너무 가까우면 생성 스킵 (촘촘하되 중복 방지) ──
                float minDist = paintSizes[sizeIndex] * minDistanceRatio;
                if (Vector3.Distance(spawnPos, lastPaintPosition) < minDist)
                    return;

                // ── [추가] 풀이 비면 새로 생성 (오브젝트 풀 미사용 허용) ──
                if (voxelPool.Count == 0)
                {
                    GameObject newVoxel = Instantiate(voxelFactory);
                    newVoxel.SetActive(false);
                    voxelPool.Add(newVoxel);
                }

                // 복셀 오브젝트 풀 이용하기
                if (voxelPool.Count > 0)
                {
                    currentTime = 0f;
                    GameObject voxel = voxelPool[0];
                    voxel.SetActive(true);
                    voxel.transform.position = spawnPos;
                    voxelPool.RemoveAt(0);

                    lastPaintPosition = spawnPos;   // [추가] 마지막 위치 갱신

                    // ── [추가] 색상 및 크기 적용 ──────────
                    Voxel voxelScript = voxel.GetComponent<Voxel>();
                    if (voxelScript != null)
                    {
                        voxelScript.SetColorAndSize(paintColors[colorIndex], paintSizes[sizeIndex]);
                    }
                }
            }
        }
        else
        {
            // 버튼을 떼면 타이머 리셋 → 다음 획 시작 즉시 반응
            currentTime = createTime;
            // ── [추가] 획이 끊기면 거리 필터도 리셋 ──────────────
            lastPaintPosition = Vector3.positiveInfinity;
        }
    }
}