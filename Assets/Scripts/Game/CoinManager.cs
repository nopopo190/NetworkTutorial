using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tutorial3
{
    public class CoinManager : NetworkBehaviour
    {
        //コインのプレハブ
        [SerializeField] private NetworkObject m_coinPrefab;
        //コイン管理
        private List<GameObject> m_coinObjects = new List<GameObject>();

        public override void OnNetworkSpawn()
        {
            //ホストの場合
            if (IsHost)
            {
                GenerateCoin();
            }
        }

        /// <summary>
        /// コイン生成
        /// </summary>
        public void GenerateCoin()
        {
            for (int x = 0; x < 10; x++)
            {
                NetworkObject coin = Instantiate(m_coinPrefab);
                int posX = UnityEngine.Random.Range(0, 10) - 5;
                int posZ = UnityEngine.Random.Range(0, 10) - 5;
                coin.transform.position = new Vector3(posX, 0, posZ);
                coin.Spawn();
                m_coinObjects.Add(coin.gameObject);
            }
        }

        /// <summary>
        /// コイン削除（ホストから呼び出される）
        /// </summary>
        /// <param name="coinObj"></param>
        public void DeleteCoin(GameObject coinObj)
        {
            var network = coinObj.GetComponent<NetworkObject>();
            network.Despawn();
            m_coinObjects.Remove(coinObj);
            if (m_coinObjects.Count == 0)
            {
                GenerateCoin();
            }
        }

        //簡易的なシングルトン
        private static CoinManager instance;
        public static CoinManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (CoinManager)FindObjectOfType(typeof(CoinManager));
                }

                return instance;
            }
        }
    }
}