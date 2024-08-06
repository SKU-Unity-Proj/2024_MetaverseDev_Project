using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    private TextMeshProUGUI playerName;

    public SendName sendName;

    void Start()
    {
        // TextMeshProUGUI ������Ʈ�� �Ҵ���� �ʾҴٸ� �ڵ����� ã��
        if (playerName == null)
        {
            playerName = GetComponent<TextMeshProUGUI>();
        }

        // A��ũ��Ʈ�� �Ҵ���� �ʾҴٸ� �ڵ����� ã��
        if (sendName == null)
        {
            sendName = FindObjectOfType<SendName>();
        }
    }

    void Update()
    {
        // TextMeshPro �ؽ�Ʈ ���� ��������� A��ũ��Ʈ�� ���� ������
        if (string.IsNullOrEmpty(playerName.text))
        {
            playerName.text = sendName.playerName;
        }
    }
}
