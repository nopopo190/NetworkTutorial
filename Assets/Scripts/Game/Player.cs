using System;
using System.Collections;
using System.Collections.Generic;
using Tutorial3;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

namespace Tutorial3
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] float m_moveSpeed = 1;
        private Rigidbody m_rigidBody;
        private Vector2 m_moveInput = Vector2.zero;

        //コインのプレハブ
        [SerializeField] private GameObject m_coinCountPrefabs;
        private CoinCount m_coinCount;

        //コイン取得数
        private NetworkVariable<int> m_coinNum;

        void Awake()
        {
            m_coinNum = new NetworkVariable<int>(0);
        }

        void Start()
        {
            // Rigidbody を取得
            m_rigidBody = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            //コイン取得数変化通知
            m_coinNum.OnValueChanged += OnCoinNumChanged;

            //UI表示
            var canvas = GameObject.Find("Canvas").transform;
            m_coinCount = Instantiate(m_coinCountPrefabs, canvas).GetComponent<CoinCount>();
            m_coinCount.SetTarget(transform);
            m_coinCount.SetNumber(m_coinNum.Value);
        }

        //コインUI更新
        void OnCoinNumChanged(int prevValue, int newValue)
        {
            m_coinCount.SetNumber(newValue);
        }

        private void Update()
        {
            //ownerの場合
            if (IsOwner)
            {
                // 移動入力を設定
                SetMoveInputServerRpc(
                        Input.GetAxisRaw("Horizontal"),
                        Input.GetAxisRaw("Vertical"));
            }

            //サーバー（ホスト）の場合
            if (IsServer)
            {
                ServerUpdate();
            }
        }

        //=================================================================
        //RPC
        //=================================================================
        /// <summary>
        /// 移動入力をセットするRPC
        /// </summary>
        [ServerRpc]
        private void SetMoveInputServerRpc(float x, float y)
        {
            m_moveInput = new Vector2(x, y);
        }

        //=================================================================
        //サーバー側で行う処理
        //=================================================================
        /// <summary>
        /// サーバー側で呼ばれるUpdate
        /// </summary>
        private void ServerUpdate()
        {
            //移動
            var velocity = Vector3.zero;
            velocity.x = m_moveSpeed * m_moveInput.normalized.x;
            velocity.z = m_moveSpeed * m_moveInput.normalized.y;
            //移動処理
            m_rigidBody.AddForce(velocity * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsServer == false) { return; }
            if (other.gameObject.CompareTag("Coin"))
            {
                //取得処理
                m_coinNum.Value += 1;
                //コイン削除処理（CoinManagerの処理を呼ぶ）
                CoinManager.Instance.DeleteCoin(other.gameObject);
            }
        }
    }
}