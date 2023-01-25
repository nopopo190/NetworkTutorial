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
        ////サーバー開始コールバック
        //NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        ////ホスト開始
        //NetworkManager.Singleton.StartHost();
        ////シーンを切り替え
        //NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        SteamLobby.Instance.JoinLobby((CSteamID)ulong.Parse(m_joinLobbyID.text));
        ////ホストに接続
        //bool result = NetworkManager.Singleton.StartClient();
        ////切断時
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    /// <summary>
    /// 接続承認
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 追加の承認手順が必要な場合は、追加の手順が完了するまでこれを true に設定します
        // true から false に遷移すると、接続承認応答が処理されます。
        response.Pending = true;
        Debug.Log(playerCount);
        //最大
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        //接続カウント
        playerCount++;

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
}
