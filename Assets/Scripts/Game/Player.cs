using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] float m_moveSpeed;
    //コインのプレハブ
    [SerializeField] private GameObject m_coinCountPrefabs;

    private Rigidbody m_rigidBody;
    private Vector2 m_moveInput;

    private NetworkVariable<int> m_coinNum;

    //頭上に表示するUI
    private CoinCount m_coinCount;

    void Awake()
    {
        m_moveInput = Vector2.zero;
        m_coinNum = new NetworkVariable<int>(0);
    }

    public override void OnNetworkSpawn()
    {
        //コイン取得数変化通知
        m_coinNum.OnValueChanged += OnCoinNumChanged;
    }

    void Start()
    {
        // Rigidbody を取得
        m_rigidBody = GetComponent<Rigidbody>();

        //UI表示
        var canvas = GameObject.Find("Canvas").transform;
        m_coinCount = Instantiate(m_coinCountPrefabs, canvas).GetComponent<CoinCount>();
        m_coinCount.SetTarget(transform);
        m_coinCount.SetNumber(m_coinNum.Value);
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

    //コインUI更新
    void OnCoinNumChanged(int prevValue, int newValue)
    {
        m_coinCount.SetNumber(newValue);
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
        m_rigidBody.AddForce(velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsServer == false) { return; }
        if (other.gameObject.CompareTag("Coin"))
        {
            //取得処理
            m_coinNum.Value += 1;
            //コイン削除処理（GameManager側の処理を呼ぶ）
            GameProc.Instance.DeleteCoin(other.gameObject);
        }
    }
}