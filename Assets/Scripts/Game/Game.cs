using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Tutorial3
{
    public class Game : NetworkBehaviour
    {
        //プレイヤーのプレハブ
        [SerializeField] private NetworkObject m_playerPrefab;

        public override void OnNetworkSpawn()
        {
            //ホストの場合
            if (IsHost)
            {
                //クライアント接続時
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                //すでに存在するクライアント用に関数呼び出す
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    OnClientConnected(client.ClientId);
                }
            }
        }

        public void OnClientConnected(ulong clientId)
        {
            //プレイヤーオブジェクト生成
            var generatePos = new Vector3(0, 1, -8);
            generatePos.x = -5 + 5 * (NetworkManager.Singleton.ConnectedClients.Count % 3);
            NetworkObject playerObject = Instantiate(m_playerPrefab, generatePos, Quaternion.identity);
            playerObject.SpawnAsPlayerObject(clientId);//接続クライアントをOwnerにしてPlayerObjectとしてスポーン
        }
    }
}