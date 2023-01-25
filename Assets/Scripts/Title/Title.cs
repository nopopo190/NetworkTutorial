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

    private int playerCount = 0;

    public void StartHost()
    {
        SteamLobby.Instance.CreateLobby();
        //playerCount = 0;
        ////�T�[�o�[�J�n�R�[���o�b�N
        //NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        ////�z�X�g�J�n
        //NetworkManager.Singleton.StartHost();
        ////�V�[����؂�ւ�
        //NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        SteamLobby.Instance.JoinLobby((CSteamID)ulong.Parse(m_joinLobbyID.text));
        ////�z�X�g�ɐڑ�
        //bool result = NetworkManager.Singleton.StartClient();
        ////�ؒf��
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    /// <summary>
    /// �ڑ����F
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // �ǉ��̏��F�菇���K�v�ȏꍇ�́A�ǉ��̎菇����������܂ł���� true �ɐݒ肵�܂�
        // true ���� false �ɑJ�ڂ���ƁA�ڑ����F��������������܂��B
        response.Pending = true;
        Debug.Log(playerCount);
        //�ő�
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        //�ڑ��J�E���g
        playerCount++;

        //��������͐ڑ������N���C�A���g�Ɍ���������
        response.Approved = true;//�ڑ�������

        //PlayerObject�𐶐����邩�ǂ���
        response.CreatePlayerObject = false;
        //��������PlayerObject��Prefab�n�b�V���l�Bnull�̏ꍇNetworkManager�ɓo�^�����v���n�u���g�p�����
        response.PlayerPrefabHash = null;

        //PlayerObject���X�|�[������ʒu(null�̏ꍇVector3.zero)
        response.Position = Vector3.zero;
        //PlayerObject���X�|�[�����̉�] (null�̏ꍇQuaternion.identity)
        response.Rotation = Quaternion.identity;

        response.Pending = false;
    }

    /// <summary>
    /// �N���C�A���g���ؒf�����Ƃ�
    /// </summary>
    private void OnClientDisconnect(ulong clientId)
    {
        //�N���C�A���g�ؒf�R�[���o�b�N
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        //�l�b�g���[�N�}�l�[�W���[��j���i����ŐV����NetworkManager�����i�g���j���Ƃ��ł���j
        NetworkManager.Singleton.Shutdown();
        //���C���V�[���ɖ߂�
        SceneManager.LoadScene("Title");
    }
}
