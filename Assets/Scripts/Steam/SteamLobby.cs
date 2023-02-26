using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamLobby : MonoBehaviour
{
    //���r�[�쐬�R�[���o�b�N
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    //���r�[���o�R�[���o�b�N
    private Callback<LobbyEnter_t> m_lobbyEnter;

    //���r�[�f�[�^�ݒ�p�L�[
    private const string s_HostAddressKey = "HostAddress";

    public ulong LobbyID { get; private set; }

    public void Start()
    {
        //SteamManager�̏��������������Ă�����
        if (SteamManager.Initialized)
        {
            m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
            m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    /// <summary>
    /// ���r�[�쐬�i�Q�[�����z�X�g�j
    /// </summary>
    /// <param name="lobbyType"></param>
    /// <param name="cMaxMembers"></param>
    public void CreateLobby()
    {
        SteamAPICall_t hCreateLobby = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_crLobbyCreated.Set(hCreateLobby);
    }

    //���r�[�쐬�����R�[���o�b�N
    private void OnCreateLobby(LobbyCreated_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_eResult != EResult.k_EResultOK || bIOFailure)
        {
            return;
        }

        //�z�X�g�̃A�h���X�iSteamID�j��o�^
        SteamMatchmaking.SetLobbyData(
            new CSteamID(pCallback.m_ulSteamIDLobby),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        //���r�[ID�ۑ�
        LobbyID = pCallback.m_ulSteamIDLobby;

        //�T�[�o�[�J�n�R�[���o�b�N
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        //�z�X�g�J�n
        NetworkManager.Singleton.StartHost();
        //�V�[����؂�ւ�
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    /// <summary>
    /// ���r�[���o
    /// </summary>
    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    /// <summary>
    /// �Q�[���̏��҂��󂯂����̃R�[���o�b�N
    /// </summary>
    /// <param name="callback"></param>
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /// <summary>
    /// ���r�[���o�R�[���o�b�N
    /// </summary>
    /// <param name="callback"></param>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            return;
        }

        //�z�X�g��SteamID���擾
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            s_HostAddressKey);

        //���r�[ID�ۑ�
        LobbyID = callback.m_ulSteamIDLobby;

        //�z�X�g�iCreateLobby�����{�l�j��������ʂ�̂ŃN���C�A���g�ڑ����Ȃ��悤�Ƀ��^�[��
        if (hostAddress == SteamUser.GetSteamID().ToString()) { return; }

        //Netcode�ŃN���C�A���g�ڑ�
        var stp = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        stp.ConnectToSteamID = ulong.Parse(hostAddress);

        //�z�X�g�ɐڑ�
        bool result = NetworkManager.Singleton.StartClient();
        //�ؒf��
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        Debug.Log($"SteamID{hostAddress}�̕����ɐڑ�");
    }

    /// <summary>
    /// �ڑ����F
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // �ǉ��̏��F�菇���K�v�ȏꍇ�́A�ǉ��̎菇����������܂ł���� true �ɐݒ肵�܂�
        // true ���� false �ɑJ�ڂ���ƁA�ڑ����F��������������܂��B
        response.Pending = true;

        //�ő�
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

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

    //�ȈՓI�ȃV���O���g��
    private static SteamLobby instance;
    public static SteamLobby Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SteamLobby)FindObjectOfType(typeof(SteamLobby));
            }

            return instance;
        }
    }
}
