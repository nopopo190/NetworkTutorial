using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameProc : NetworkBehaviour
{
    //�R�C���̃v���n�u
    [SerializeField] private NetworkObject m_coinPrefab;
    //�R�C���̃v���n�u
    [SerializeField] private NetworkObject m_playerPrefab;

    //�R�C���Ǘ�
    private List<GameObject> m_coinObjects;

    private void Awake()
    {
        m_coinObjects = new List<GameObject>();
    }

    public override void OnNetworkSpawn()
    {
        //�z�X�g�̏ꍇ
        if (IsHost)
        {
            //�R�C������
            for (int x = 0; x < 10; x++)
            {
                NetworkObject coin = Instantiate(m_coinPrefab);
                coin.transform.position = new Vector3(UnityEngine.Random.Range(0, 10) - 5, 0, UnityEngine.Random.Range(0, 10) - 5);
                coin.Spawn();
                m_coinObjects.Add(coin.gameObject);
            }

            //�N���C�A���g�ڑ���
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            //���łɑ��݂���N���C�A���g�p�Ɋ֐��Ăяo��
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                OnClientConnected(client.ClientId);
            }
        }
    }

    public void OnClientConnected(ulong clientId)
    {
        //�v���C���[�I�u�W�F�N�g����
        var generatePos = new Vector3(0, 1, -8);
        generatePos.x = -5 + 5 * (NetworkManager.Singleton.ConnectedClients.Count % 3);
        NetworkObject playerObject = Instantiate(m_playerPrefab, generatePos, Quaternion.identity);
        playerObject.SpawnAsPlayerObject(clientId);//�ڑ��N���C�A���g��Owner�ɂ���PlayerObject�Ƃ��ăX�|�[��
    }

    public void DeleteCoin(GameObject coinObj)
    {
        var network = coinObj.GetComponent<NetworkObject>();
        network.Despawn();
        m_coinObjects.Remove(coinObj);
        if(m_coinObjects.Count == 0)
        {
            //�R�C������
            for (int x = 0; x < 10; x++)
            {
                NetworkObject coin = Instantiate(m_coinPrefab);
                coin.transform.position = new Vector3(UnityEngine.Random.Range(0, 10) - 5, 0, UnityEngine.Random.Range(0, 10) - 5);
                coin.Spawn();
                m_coinObjects.Add(coin.gameObject);
            }
        }
    }

    //�ȈՓI�ȃV���O���g��
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
