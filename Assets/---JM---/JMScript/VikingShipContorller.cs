using UnityEngine;

public class VikingShipController : MonoBehaviour
{
    public Transform pivot; // 바이킹 중심축 Transform
    public float amplitude = 30f; // 최대 흔들림 각도
    public float speed = 1f; // 흔들림 속도

    void Update()
    {
        // 사인 함수를 사용하여 각도를 계산
        float angle = amplitude * Mathf.Sin(Time.time * speed);

        // 중심축을 기준으로 회전
        pivot.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
