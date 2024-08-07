using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffGpt : MonoBehaviour
{
    public GameObject gptCanvas;

    private bool cursorVisible = true; // �ʱ� Ŀ�� ����

    void Update()
    {
        // GŰ�� ������ �� UI ����� Ȱ�� ���¸� ���
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
                Debug.LogError("UI ��Ұ� �������� �ʾҽ��ϴ�.");
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
