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
            //���r�[�쐬
            SteamLobby.Instance.CreateLobby();
        }

        public void StartClient()
        {
            //���r�[����
            SteamLobby.Instance.JoinLobby((CSteamID)ulong.Parse(m_joinLobbyID.text));
        }
    }

}