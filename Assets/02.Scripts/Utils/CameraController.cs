using Cinemachine;
using Photon.Pun;
using System;
using UnityEngine;

public class CameraController : MonoBehaviourPun
{
    public RaycastHit target;
    public Vector3 screenCenter;
    public Transform aimTarget;
    private Cinemachine3rdPersonFollow cameraDistance;
    private float setDistance;
    private float curDistance;
    private float zoomDelay;
    private float maxZoomDelay;
    private float time;

    public Vector3 cameraOriginalPos;
    public float shakeDuration;

    public Vector3 fireTarget;
    void Start()
    {
        cameraDistance = GetComponent<CinemachineVirtualCamera>()
            .GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        maxZoomDelay = 0.3f;
        cameraOriginalPos = transform.position;
        
    }

    void Update()
    {
        CenterRay();
        Zoom();
    }
    private void Zoom()
    {
        if (curDistance != setDistance)
        {
            // 선형보간
            time += Time.deltaTime;
            cameraDistance.CameraDistance = 
                ZoomLerp(curDistance, setDistance, time);
        }
    }
    private void CenterRay()
    {
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out target))
        {
            fireTarget = target.point;
            aimTarget.position = target.point;
        }
    }
    private float ZoomLerp(float a, float b, float t)
    {
        return a + (b - a) * Mathf.Clamp01(t / maxZoomDelay);
    }
    public void SetDistance(float distance)
    {
        curDistance = cameraDistance.CameraDistance;
        setDistance = distance;
        time = 0;
    }
}
