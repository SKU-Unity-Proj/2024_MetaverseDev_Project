using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rideChangePos : MonoBehaviour
{
    public float interactDistance = 3f; // 상호작용 가능한 최대 거리
    private Transform originalParent; // 원래의 부모 오브젝트 저장
    private Quaternion originalRotation; // 원래의 회전 각도 저장
    private Ride currentRide; // 현재 탑승 중인 놀이기구

    public LayerMask interactLayer; // 상호작용 가능한 레이어 마스크

    private bool isSeated = false; // 플레이어가 놀이기구에 탑승 중인지 여부

    void Start()
    {
        originalParent = transform.parent; // 원래 부모 오브젝트 저장
        originalRotation = transform.rotation; // 원래 회전 각도 저장
    }

    void Update()
    {
        // F 키 입력 체크
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isSeated)
            {
                // 놀이기구에서 내리기
                ExitRide();
            }
            else
            {
                // 놀이기구에 탑승 시도
                TryInteractWithRide();
            }
        }

        // 탑승 중인 경우 플레이어를 seatPosition에 고정
        if (isSeated)
        {
            transform.localPosition = Vector3.zero;
            //transform.localRotation = Quaternion.identity;
        }
    }

    void TryInteractWithRide()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * interactDistance, Color.red, 2f); // 디버그 레이 그리기

        // Raycast를 사용하여 놀이기구와의 상호작용 체크
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {     
            Ride ride = hit.collider.GetComponent<Ride>();
            if (ride != null)
            {
                ///Debug.Log("Interacting with Ride: " + ride.name); // 디버그 메시지 출력

                // 플레이어를 의자 위치로 이동 (월드 포지션 사용)
                transform.position = ride.seatPosition.position;
                transform.rotation = ride.seatPosition.rotation;

                // 플레이어를 놀이기구의 자식 오브젝트로 설정
                transform.SetParent(ride.seatPosition);

                isSeated = true; // 탑승 상태로 설정
                currentRide = ride; // 현재 탑승 중인 놀이기구 설정
                //Debug.Log("Player moved to seat position."); // 디버그 메시지 출력
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any objects."); // 디버그 메시지 출력
        }
    }

    void ExitRide()
    {
        // 플레이어를 원래 부모로 설정
        transform.SetParent(originalParent);

        // 플레이어의 위치를 놀이기구에서 내리는 위치로 설정 (예: 현재 위치 근처)
        transform.position += transform.forward * 2f; // 현재 위치에서 약간 앞으로 이동

        // 원래 회전 각도로 되돌리기
        transform.rotation = originalRotation;

        isSeated = false; // 탑승 상태 해제
        currentRide = null; // 현재 탑승 중인 놀이기구 초기화
        //Debug.Log("Player exited the ride."); // 디버그 메시지 출력
    }
}
