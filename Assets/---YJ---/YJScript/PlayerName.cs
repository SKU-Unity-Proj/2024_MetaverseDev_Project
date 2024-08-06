using UnityEngine;
using TMPro;
using Fusion;

public class PlayerName : NetworkBehaviour
{
    public TextMeshProUGUI nicknameText;

    [Networked] public string Nickname { get; private set; }

    void Start()
    {
        // TextMeshProUGUI 컴포넌트가 할당되지 않았다면 자동으로 찾기
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
        // 닉네임을 표시하는 UI를 업데이트하는 코드 추가
        if (nicknameText != null)
        {
            nicknameText.text = Nickname;
        }
    }
}
