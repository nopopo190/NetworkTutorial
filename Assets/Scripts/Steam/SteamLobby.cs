using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamLobby : MonoBehaviour
{
    //ロビー作成コールバック
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    //ロビー入出コールバック
    private Callback<LobbyEnter_t> m_lobbyEnter;

    //ロビーデータ設定用キー
    private const string s_HostAddressKey = "HostAddress";

    public ulong LobbyID { get; private set; }

    public void Start()
    {
        //SteamManagerの初期化が完了していたら
        if (SteamManager.Initialized)
        {
            m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
            m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    /// <summary>
    /// ロビー作成（ゲームをホスト）
    /// </summary>
    /// <param name="lobbyType"></param>
    /// <param name="cMaxMembers"></param>
    public void CreateLobby()
    {
        SteamAPICall_t hCreateLobby = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_crLobbyCreated.Set(hCreateLobby);
    }

    //ロビー作成完了コールバック
    private void OnCreateLobby(LobbyCreated_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_eResult != EResult.k_EResultOK || bIOFailure)
        {
            return;
        }

        //ホストのアドレス（SteamID）を登録
        SteamMatchmaking.SetLobbyData(
            new CSteamID(pCallback.m_ulSteamIDLobby),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        //ロビーID保存
        LobbyID = pCallback.m_ulSteamIDLobby;

        //サーバー開始コールバック
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        //ホスト開始
        NetworkManager.Singleton.StartHost();
        //シーンを切り替え
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    /// <summary>
    /// ロビー入出
    /// </summary>
    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    /// <summary>
    /// ゲームの招待を受けた時のコールバック
    /// </summary>
    /// <param name="callback"></param>
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /// <summary>
    /// ロビー入出コールバック
    /// </summary>
    /// <param name="callback"></param>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            return;
        }

        //ホストのSteamIDを取得
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            s_HostAddressKey);

        //ロビーID保存
        LobbyID = callback.m_ulSteamIDLobby;

        //ホスト（CreateLobbyした本人）もここを通るのでクライアント接続しないようにリターン
        if (hostAddress == SteamUser.GetSteamID().ToString()) { return; }

        //Netcodeでクライアント接続
        var stp = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        stp.ConnectToSteamID = ulong.Parse(hostAddress);

        //ホストに接続
        bool result = NetworkManager.Singleton.StartClient();
        //切断時
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        Debug.Log($"SteamID{hostAddress}の部屋に接続");
    }

    /// <summary>
    /// 接続承認
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 追加の承認手順が必要な場合は、追加の手順が完了するまでこれを true に設定します
        // true から false に遷移すると、接続承認応答が処理されます。
        response.Pending = true;

        //最大
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        //ここからは接続成功クライアントに向けた処理
        response.Approved = true;//接続を許可

        //PlayerObjectを生成するかどうか
        response.CreatePlayerObject = false;
        //生成するPlayerObjectのPrefabハッシュ値。nullの場合NetworkManagerに登録したプレハブが使用される
        response.PlayerPrefabHash = null;

        //PlayerObjectをスポーンする位置(nullの場合Vector3.zero)
        response.Position = Vector3.zero;
        //PlayerObjectをスポーン時の回転 (nullの場合Quaternion.identity)
        response.Rotation = Quaternion.identity;

        response.Pending = false;
    }

    /// <summary>
    /// クライアントが切断したとき
    /// </summary>
    private void OnClientDisconnect(ulong clientId)
    {
        //クライアント切断コールバック
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        //ネットワークマネージャーを破棄（これで新しくNetworkManagerを作る（使う）ことができる）
        NetworkManager.Singleton.Shutdown();
        //メインシーンに戻る
        SceneManager.LoadScene("Title");
    }

    //簡易的なシングルトン
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
