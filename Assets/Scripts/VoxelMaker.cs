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
    private float lastHitDistance;
    public float defaultPaintDistance = 3.0f;
    public float rayMinDistance = 0.15f;

    // 레이 시각화용 LineRenderer
    private LineRenderer lineRenderer;
    public float rayVisualLength = 5.0f;


    void Start()
    {
        lastHitDistance = defaultPaintDistance;

        for (int i = 0; i < voxelPoolSize; i++)
        {
            GameObject voxel = Instantiate(voxelFactory);
            voxel.SetActive(false);
            voxelPool.Add(voxel);
        }

        // LineRenderer 자동 생성
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth   = 0.002f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor   = new Color(1f, 1f, 1f, 0f);
        lineRenderer.material   = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        // 크로스헤어 그리기
        ARAVRInput.DrawCrosshair(crosshair);

        // 왼쪽 Y버튼 → 색상 순환
        bool yButton = ARAVRInput.Get(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch);
        if (yButton && !prevYButton)
        {
            colorIndex = (colorIndex + 1) % paintColors.Length;
            Debug.Log($"[VoxelMaker] Color → {paintColors[colorIndex]}");
        }
        prevYButton = yButton;

        // 왼쪽 X버튼 → 크기 순환
        bool xButton = ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);
        if (xButton && !prevXButton)
        {
            sizeIndex = (sizeIndex + 1) % paintSizes.Length;
            Debug.Log($"[VoxelMaker] Size → {paintSizes[sizeIndex]}");
        }
        prevXButton = xButton;

        Vector3 handPos = ARAVRInput.RHandPosition;
        Vector3 handDir = ARAVRInput.RHandDirection;
        Ray ray = new Ray(handPos, handDir);

        // 레이 시각화
        UpdateRayVisual(ray);

        // 오른쪽 A버튼(RTouch)으로 페인팅
        if (ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.RTouch))
        {
            currentTime += Time.deltaTime;
            if (currentTime > createTime)
            {
                Vector3 spawnPos;

                Vector3 raycastOrigin = handPos + handDir * rayMinDistance;
                Ray offsetRay = new Ray(raycastOrigin, handDir);
                RaycastHit hitInfo;

                if (Physics.Raycast(offsetRay, out hitInfo))
                {
                    spawnPos = hitInfo.point + hitInfo.normal * (paintSizes[sizeIndex] * 0.5f);
                    // 실제 거리 = offset 거리 + raycast 거리
                    lastHitDistance = rayMinDistance + hitInfo.distance;
                }
                else
                {
                    spawnPos = handPos + handDir * lastHitDistance;
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

    void UpdateRayVisual(Ray ray)
    {
        if (lineRenderer == null) return;

        Vector3 endPos;
        RaycastHit hit;
        Vector3 raycastOrigin = ray.origin + ray.direction * rayMinDistance;

        if (Physics.Raycast(new Ray(raycastOrigin, ray.direction), out hit, rayVisualLength))
            endPos = hit.point;
        else
            endPos = ray.origin + ray.direction * rayVisualLength;

        lineRenderer.SetPosition(0, ray.origin);
        lineRenderer.SetPosition(1, endPos);

        lineRenderer.startColor = paintColors[colorIndex];
        lineRenderer.endColor   = new Color(
            paintColors[colorIndex].r,
            paintColors[colorIndex].g,
            paintColors[colorIndex].b,
            0f);
    }
}