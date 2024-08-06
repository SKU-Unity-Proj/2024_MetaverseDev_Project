using UnityEngine;
using TMPro;
using Fusion;

public class SendName : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public string playerNickName = null;

    private void Awake()
    {
        playerNickName = playerNameInput.GetComponent<TMP_InputField>().text;
    }

    public void InputName()
    {
        playerNickName = playerNameInput.text;
        var playerName = FindObjectOfType<PlayerName>();
        playerName.SetNickname(playerNickName);
    }
}