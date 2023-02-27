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
        //�R�C���̃v���n�u
        [SerializeField] private NetworkObject m_coinPrefab;
        //�R�C���Ǘ�
        private List<GameObject> m_coinObjects = new List<GameObject>();

        public override void OnNetworkSpawn()
        {
            //�z�X�g�̏ꍇ
            if (IsHost)
            {
                GenerateCoin();
            }
        }

        /// <summary>
        /// �R�C������
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
        /// �R�C���폜�i�z�X�g����Ăяo�����j
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

        //�ȈՓI�ȃV���O���g��
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