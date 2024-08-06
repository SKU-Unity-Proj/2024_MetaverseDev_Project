using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingController : MonoBehaviour
{
    public Transform merryGoRound; // 놀이기구의 Transform
    public float rotationSpeed = 30f; // 회전 속도

    void Update()
    {
        // 놀이기구 회전
        merryGoRound.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
