using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameProc : NetworkBehaviour
{
    //コインのプレハブ
    [SerializeField] private NetworkObject m_coinPrefab;
    //コインのプレハブ
    [SerializeField] private NetworkObject m_playerPrefab;

    //コイン管理
    private List<GameObject> m_coinObjects;

    private void Awake()
    {
        m_coinObjects = new List<GameObject>();
    }

    public override void OnNetworkSpawn()
    {
        //ホストの場合
        if (IsHost)
        {
            //コイン生成
            for (int x = 0; x < 10; x++)
            {
                NetworkObject coin = Instantiate(m_coinPrefab);
                coin.transform.position = new Vector3(UnityEngine.Random.Range(0, 10) - 5, 0, UnityEngine.Random.Range(0, 10) - 5);
                coin.Spawn();
                m_coinObjects.Add(coin.gameObject);
            }

            //クライアント接続時
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            //すでに存在するクライアント用に関数呼び出す
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
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

    public void DeleteCoin(GameObject coinObj)
    {
        var network = coinObj.GetComponent<NetworkObject>();
        network.Despawn();
        m_coinObjects.Remove(coinObj);
        if(m_coinObjects.Count == 0)
        {
            //コイン生成
            for (int x = 0; x < 10; x++)
            {
                NetworkObject coin = Instantiate(m_coinPrefab);
                coin.transform.position = new Vector3(UnityEngine.Random.Range(0, 10) - 5, 0, UnityEngine.Random.Range(0, 10) - 5);
                coin.Spawn();
                m_coinObjects.Add(coin.gameObject);
            }
        }
    }

    //簡易的なシングルトン
    private static GameProc instance;
    public static GameProc Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (GameProc)FindObjectOfType(typeof(GameProc));
            }

            return instance;
        }
    }
}
