using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class MultiplayerChat : NetworkBehaviour
{
    public TextMeshProUGUI _messages;
    public TextMeshProUGUI input;
    public TextMeshProUGUI usernameInput;
    public string username = "Default";

    public void SetUsername()
    {
        username = usernameInput.text;
    }

    public void CallMessageRPC()
    {
        string message = input.text;
        RPC_SendMessage(username, message);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(string username, string message, RpcInfo rpcInfo = default)
    {
        _messages.text += $"{username}: {message}\n";
    }
}
