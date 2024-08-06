using UnityEngine;

public class TrainController : MonoBehaviour
{
    public Transform[] waypoints; // 이동할 포인트들의 Transform 배열
    public float speed = 5f; // 기차의 이동 속도
    private float t = 0f; // 현재 경로의 위치를 나타내는 매개변수
    private int currentWaypointIndex = 0; // 현재 목표로 하는 포인트의 인덱스

    void Update()
    {
        if (waypoints.Length < 4) return;

        // 현재 세그먼트의 포인트들 선택
        Vector3 p0 = waypoints[GetWrappedIndex(currentWaypointIndex - 1)].position;
        Vector3 p1 = waypoints[currentWaypointIndex].position;
        Vector3 p2 = waypoints[GetWrappedIndex(currentWaypointIndex + 1)].position;
        Vector3 p3 = waypoints[GetWrappedIndex(currentWaypointIndex + 2)].position;

        // Catmull-Rom 스플라인을 사용하여 곡선 계산
        Vector3 newPosition = CatmullRomSpline(p0, p1, p2, p3, t);

        // 기차 이동
        transform.position = newPosition;

        // 기차 회전
        Vector3 targetDirection = CatmullRomSpline(p0, p1, p2, p3, t + 0.01f) - newPosition;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }

        // t 값 업데이트
        t += speed * Time.deltaTime / Vector3.Distance(p1, p2);

        // t 값이 1 이상이면 다음 세그먼트로 이동
        if (t >= 1f)
        {
            t = 0f;
            currentWaypointIndex = GetWrappedIndex(currentWaypointIndex + 1);
        }
    }

    int GetWrappedIndex(int index)
    {
        // 인덱스를 0과 배열 길이 사이로 맞춤
        return (index + waypoints.Length) % waypoints.Length;
    }

    Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom 스플라인을 사용하여 곡선 계산
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}
