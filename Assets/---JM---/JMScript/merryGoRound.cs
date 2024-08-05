using UnityEngine;

public class MerryGoRoundController : MonoBehaviour
{
    public Transform merryGoRound; // 놀이기구의 Transform
    public float rotationSpeed = 10f; // 회전 속도

    void Update()
    {
        // 놀이기구 회전
        merryGoRound.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
    }
}
