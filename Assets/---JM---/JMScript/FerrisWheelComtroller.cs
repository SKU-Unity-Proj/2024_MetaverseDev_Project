using UnityEngine;

public class FerrisWheelController : MonoBehaviour
{
    public Transform ferrisWheel; // 관람차의 Transform
    public Transform centerPivot; // 부스 회전을 위한 중심 축 Transform
    public Transform[] cabins;    // 부스들의 Transform 배열
    public float rotationSpeed = 10f; // 회전 속도

    private Vector3[] initialCabinPositions;

    void Start()
    {
        // 각 부스의 초기 위치를 저장합니다.
        initialCabinPositions = new Vector3[cabins.Length];
        for (int i = 0; i < cabins.Length; i++)
        {
            // 부스의 초기 위치를 중심 축으로부터 상대적인 위치로 저장
            initialCabinPositions[i] = centerPivot.InverseTransformPoint(cabins[i].position);
        }
    }

    void Update()
    {
        // 관람차 회전
        ferrisWheel.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // 각 부스를 회전시키는 로직
        for (int i = 0; i < cabins.Length; i++)
        {
            Transform cabin = cabins[i];

            // 부스의 초기 위치에서 중심 축을 기준으로 회전
            Vector3 rotatedPosition = Quaternion.Euler(0, 0, rotationSpeed * Time.deltaTime) * initialCabinPositions[i];

            // 새로운 위치 설정
            cabin.position = centerPivot.TransformPoint(rotatedPosition);

            // 부스가 항상 수직을 유지하도록 회전
            Vector3 directionToCenter = centerPivot.position - cabin.position;
            cabin.rotation = Quaternion.LookRotation(Vector3.forward, directionToCenter);
        }
    }
}
