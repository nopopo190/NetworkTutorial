using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyID : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_lobbyIdText;

    private void Start()
    {
        m_lobbyIdText.text = SteamLobby.Instance.LobbyID.ToString();
    }

    public void CopyID()
    {
        GUIUtility.systemCopyBuffer = m_lobbyIdText.text;
    }
}
