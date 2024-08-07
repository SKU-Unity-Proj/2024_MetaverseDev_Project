using UnityEngine;
using TMPro;
using Fusion;

public class SendName : NetworkBehaviour
{
    public TMP_InputField playerNameInput;
    public string playerNickName = null;
    public MultiplayerChat multiplayerChat;

    private void Awake()
    {
        playerNickName = playerNameInput.GetComponent<TMP_InputField>().text;
    }

    public void InputName()
    {
        playerNickName = playerNameInput.text;
        var playerName = FindObjectOfType<PlayerName>();
        playerName.SetNickname(playerNickName);

        multiplayerChat.username = playerNickName;
    }
}