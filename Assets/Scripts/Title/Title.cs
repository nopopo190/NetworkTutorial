using Steamworks;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField]
    TMP_InputField m_joinLobbyID;

    public void StartHost()
    {
        SteamLobby.Instance.CreateLobby();
    }

    public void StartClient()
    {
        SteamLobby.Instance.JoinLobby((CSteamID)ulong.Parse(m_joinLobbyID.text));
    }
}
