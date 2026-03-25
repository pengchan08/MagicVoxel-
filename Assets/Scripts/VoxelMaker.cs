using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMaker : MonoBehaviour
{
    public GameObject voxelFactory;
    public int voxelPoolSize = 20;

    // 오브젝트 풀
    public static List<GameObject> voxelPool = new List<GameObject>();

    // 생성 시간
    public float createTime = 0.1f;
    // 경과 시간
    float currentTime = 0f;

    // 크로스헤어 변수
    public Transform crosshair;


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
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(ray, out hitInfo))
                {
                    // 복셀 오브젝트 풀 이용하기
                    if (voxelPool.Count > 0)
                    {
                        currentTime = 0f;   
                        GameObject voxel = voxelPool[0];
                        voxel.SetActive(true);
                        voxel.transform.position = hitInfo.point;
                        voxelPool.RemoveAt(0);
                    }
                }
            }
        }
    }
}
