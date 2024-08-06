using UnityEngine;
using TMPro;
using Fusion;

public class PlayerName : NetworkBehaviour
{
    public TextMeshProUGUI nicknameText;

    [Networked] public string Nickname { get; private set; }

    void Start()
    {
        // TextMeshProUGUI ������Ʈ�� �Ҵ���� �ʾҴٸ� �ڵ����� ã��
        if (nicknameText == null)
        {
            nicknameText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void SetNickname(string nickname)
    {
        Nickname = nickname;
        UpdateNicknameUI();
    }

    private void UpdateNicknameUI()
    {
        // �г����� ǥ���ϴ� UI�� ������Ʈ�ϴ� �ڵ� �߰�
        if (nicknameText != null)
        {
            nicknameText.text = Nickname;
        }
    }
}
