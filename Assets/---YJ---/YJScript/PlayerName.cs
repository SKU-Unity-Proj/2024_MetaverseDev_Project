using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    private TextMeshProUGUI playerName;

    public SendName sendName;

    void Start()
    {
        // TextMeshProUGUI 컴포넌트가 할당되지 않았다면 자동으로 찾기
        if (playerName == null)
        {
            playerName = GetComponent<TextMeshProUGUI>();
        }

        // A스크립트가 할당되지 않았다면 자동으로 찾기
        if (sendName == null)
        {
            sendName = FindObjectOfType<SendName>();
        }
    }

    void Update()
    {
        // TextMeshPro 텍스트 값이 비어있으면 A스크립트의 값을 가져옴
        if (string.IsNullOrEmpty(playerName.text))
        {
            playerName.text = sendName.playerName;
        }
    }
}
