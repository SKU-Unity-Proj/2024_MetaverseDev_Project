using Cinemachine;
using UnityEngine;

public class DynamicCameraController : MonoBehaviour
{
    public CinemachineStateDrivenCamera stateDrivenCamera;
    public string defaultCameraName = "Default Follow"; // 'default' 상태의 Virtual Camera 이름

    private CinemachineVirtualCamera currentVirtualCamera;

    public float waveFrequency = 1.0f; // 울렁거림의 주파수
    public float waveAmplitude = 0.5f; // 울렁거림의 크기

    private float originalYPosition;
    private float timer;

    void Start()
    {
        // 초기 Y 위치를 저장합니다.
        originalYPosition = stateDrivenCamera.transform.position.y;
    }
    void Update()
    {
        // 현재 활성화된 Virtual Camera 얻기
        currentVirtualCamera = stateDrivenCamera.LiveChild as CinemachineVirtualCamera;

        // 'default' 상태의 Virtual Camera인지 확인
        if (currentVirtualCamera != null && currentVirtualCamera.Name == defaultCameraName)
        {
            // 타이머를 업데이트합니다.
            timer += Time.deltaTime;

            // 사인 함수를 사용하여 Y 축에 대한 울렁거림을 계산합니다.
            float newYPosition = originalYPosition + Mathf.Sin(timer * waveFrequency) * waveAmplitude;

            // 카메라의 위치를 업데이트합니다.
            Vector3 newPosition = stateDrivenCamera.transform.position;
            newPosition.y = newYPosition;
            stateDrivenCamera.transform.position = newPosition;
        }
        else
        {
            // 다른 상태에서는 타이머를 리셋합니다.
            timer = 0;
        }
    }
}