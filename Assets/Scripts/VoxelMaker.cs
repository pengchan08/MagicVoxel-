using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMaker : MonoBehaviour
{
    public GameObject voxelFactory;
    public int voxelPoolSize = 200;

    // 오브젝트 풀
    public static List<GameObject> voxelPool = new List<GameObject>();

    // 생성 시간
    public float createTime = 0.001f;
    // 경과 시간
    float currentTime = 0f;

    // 크로스헤어 변수
    public Transform crosshair;

    public Color[] paintColors = {
        Color.red,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.blue,
        new Color(0.5f, 0f, 1f),
        Color.white,
        Color.black
    };
    private int colorIndex = 0;

    public float[] paintSizes = { 0.02f, 0.05f, 0.10f, 0.18f, 0.30f };
    private int sizeIndex = 1;

    private bool prevYButton = false;
    private bool prevXButton = false;

    private Vector3 lastPaintPosition = Vector3.positiveInfinity;
    public float minDistanceRatio = 0f;
    private float lastHitDistance = 3.0f;
    public float defaultPaintDistance = 3.0f;


    void Start()
    {
        lastHitDistance = defaultPaintDistance;

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

        bool yButton = ARAVRInput.Get(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch);
        if (yButton && !prevYButton)
        {
            colorIndex = (colorIndex + 1) % paintColors.Length;
            Debug.Log($"[VoxelMaker] Color → {paintColors[colorIndex]}");
        }
        prevYButton = yButton;

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
        if (ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.RTouch))
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
                    spawnPos = hitInfo.point + hitInfo.normal * (paintSizes[sizeIndex] * 0.5f);
                    lastHitDistance = hitInfo.distance;
                }
                else
                {
                    spawnPos = ray.origin + ray.direction * lastHitDistance;
                }

                float minDist = paintSizes[sizeIndex] * minDistanceRatio;
                if (Vector3.Distance(spawnPos, lastPaintPosition) < minDist)
                    return;

                if (voxelPool.Count == 0)
                {
                    GameObject newVoxel = Instantiate(voxelFactory);
                    newVoxel.SetActive(false);
                    voxelPool.Add(newVoxel);
                }

                if (voxelPool.Count > 0)
                {
                    currentTime = 0f;
                    GameObject voxel = voxelPool[0];
                    voxel.SetActive(true);
                    voxel.transform.position = spawnPos;
                    voxelPool.RemoveAt(0);

                    lastPaintPosition = spawnPos;

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
            currentTime = createTime;
            lastPaintPosition = Vector3.positiveInfinity;
        }
    }
}