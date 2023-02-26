using Steamworks;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Tutorial3
{
    public class Title : MonoBehaviour
    {
        [SerializeField]
        TMP_InputField m_joinLobbyID;

        public void StartHost()
        {
            //ロビー作成
            SteamLobby.Instance.CreateLobby();
        }

        public void StartClient()
        {
            //ロビー入室
            SteamLobby.Instance.JoinLobby((CSteamID)ulong.Parse(m_joinLobbyID.text));
        }
    }

}