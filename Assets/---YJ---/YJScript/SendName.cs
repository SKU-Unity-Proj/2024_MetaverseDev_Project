using UnityEngine;
using TMPro;

public class SendName : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public string playerName = null;

    private void Awake()
    {
        playerName = playerNameInput.GetComponent<TMP_InputField>().text;
    }

    public void InputName()
    {
        playerName = playerNameInput.text;
    }
}
