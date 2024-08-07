using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffGpt : MonoBehaviour
{
    public GameObject gptCanvas;

    private bool cursorVisible = true; // 초기 커서 상태

    void Update()
    {
        // G키가 눌렸을 때 UI 요소의 활성 상태를 토글
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (gptCanvas != null)
            {
                bool isActive = !gptCanvas.activeSelf;
                LockCursor(!isActive);
                gptCanvas.SetActive(!gptCanvas.activeSelf);
            }
            else
            {
                Debug.LogError("UI 요소가 지정되지 않았습니다.");
            }

            void LockCursor(bool isLocked)
            {
                Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isLocked;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            cursorVisible = !cursorVisible;
            Cursor.visible = cursorVisible;
            Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
