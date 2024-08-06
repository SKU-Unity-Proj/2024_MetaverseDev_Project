using UnityEngine;
using System.Collections;

public class RollerCoasterController : MonoBehaviour
{
    public Transform[] waypoints; // 경로의 포인트들
    public float baseSpeed = 5f; // 초기 속도
    private float currentSpeed; // 현재 속도
    private float t = 0f; // 경로의 위치를 나타내는 매개변수
    private int currentWaypointIndex = 0; // 현재 목표 포인트의 인덱스
    private bool isStopped = false; // 차량이 정지 상태인지 여부
    private bool returningToStart = false; // 시작점으로 돌아가는 상태인지 여부

    void Start()
    {
        // 시작 위치와 방향 설정
        if (waypoints.Length >= 2)
        {
            transform.position = waypoints[0].position;
            Vector3 initialDirection = waypoints[1].position - waypoints[0].position;
            if (initialDirection != Vector3.zero)
            {
                Quaternion initialRotation = Quaternion.LookRotation(initialDirection);
                transform.rotation = initialRotation * Quaternion.Euler(0, -90, 0); // Y축을 기준으로 -90도 회전
            }
        }

        currentSpeed = baseSpeed; // 초기 속도로 설정
        StartCoroutine(RollerCoasterRoutine());
    }

    IEnumerator RollerCoasterRoutine()
    {
        while (true)
        {
            // 속도 설정
            SetSpeed();

            if (!isStopped)
            {
                if (returningToStart)
                {
                    // 마지막 웨이포인트에서 첫 번째 웨이포인트로 이동 처리
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, waypoints[0].position, currentSpeed * Time.deltaTime);
                    transform.position = newPosition;

                    Vector3 targetDirection = waypoints[0].position - transform.position;
                    if (targetDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                        transform.rotation = targetRotation * Quaternion.Euler(0, -90, 0); // Y축을 기준으로 -90도 회전
                    }

                    if (Vector3.Distance(transform.position, waypoints[0].position) < 0.1f)
                    {
                        returningToStart = false;
                        currentWaypointIndex = 0;
                        t = 0f;
                    }
                }
                else
                {
                    // 현재 세그먼트의 포인트들 선택
                    Vector3 p0 = waypoints[GetWrappedIndex(currentWaypointIndex - 1)].position;
                    Vector3 p1 = waypoints[currentWaypointIndex].position;
                    Vector3 p2 = waypoints[GetWrappedIndex(currentWaypointIndex + 1)].position;
                    Vector3 p3 = waypoints[GetWrappedIndex(currentWaypointIndex + 2)].position;

                    // Catmull-Rom Spline을 사용하여 곡선 계산
                    Vector3 newPosition = CatmullRomSpline(p0, p1, p2, p3, t);

                    // 차량 이동
                    transform.position = newPosition;

                    // 차량 회전
                    Vector3 targetDirection = CatmullRomSpline(p0, p1, p2, p3, t + 0.01f) - newPosition;
                    if (targetDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                        transform.rotation = targetRotation * Quaternion.Euler(0, -90, 0); // Y축을 기준으로 -90도 회전
                    }

                    // t 값 업데이트
                    t += currentSpeed * Time.deltaTime / Vector3.Distance(p1, p2);

                    // t 값이 1 이상이면 다음 세그먼트로 이동
                    if (t >= 1f)
                    {
                        t = 0f;
                        currentWaypointIndex = GetWrappedIndex(currentWaypointIndex + 1);
                    }

                    // 마지막 웨이포인트에 도달하면 정지
                    if (currentWaypointIndex == waypoints.Length - 1)
                    {
                        isStopped = true;
                        yield return new WaitForSeconds(5f); // 5초 정지
                        returningToStart = true;
                        isStopped = false;
                    }
                }
            }
            yield return null;
        }
    }

    //균일하게 속도 줄임
    void SetSpeed()
    {
        if (returningToStart)
        {
            currentSpeed = baseSpeed;
            return;
        }

        if (currentWaypointIndex <= 15)
        {
            currentSpeed = Mathf.Lerp(5f, 15f, currentWaypointIndex / 15f);
        }
        else if (currentWaypointIndex <= 27)
        {
            currentSpeed = Mathf.Lerp(15f, 30f, (currentWaypointIndex - 15) / 12f);
        }
        else if (currentWaypointIndex <= 34)
        {
            currentSpeed = Mathf.Lerp(30f, 5f, (currentWaypointIndex - 27) / 7f);
        }
        else if (currentWaypointIndex <= 38)
        {
            currentSpeed = Mathf.Lerp(5f, 25f, (currentWaypointIndex - 34) / 4f);
        }
        else if (currentWaypointIndex <= 48)
        {
            currentSpeed = Mathf.Lerp(25f, 45f, (currentWaypointIndex - 38) / 10f);
        }
        else if (currentWaypointIndex <= 56)
        {
            currentSpeed = Mathf.Lerp(45f, 0f, (currentWaypointIndex - 48) / 8f);
        }
    }

    int GetWrappedIndex(int index)
    {
        // 인덱스를 0과 배열 길이 사이로 맞춤
        return (index + waypoints.Length) % waypoints.Length;
    }

    Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom Spline을 사용하여 곡선 계산
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}
