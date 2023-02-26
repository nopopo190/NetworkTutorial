using TMPro;
using UnityEngine;

public class LobbyID : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_lobbyIdText;

    private void Start()
    {
        m_lobbyIdText.text = SteamLobby.Instance.LobbyID.ToString();
    }
}
