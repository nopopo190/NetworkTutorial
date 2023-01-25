using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] float m_moveSpeed;
    //�R�C���̃v���n�u
    [SerializeField] private GameObject m_coinCountPrefabs;

    private Rigidbody m_rigidBody;
    private Vector2 m_moveInput;

    private NetworkVariable<int> m_coinNum;

    //����ɕ\������UI
    private CoinCount m_coinCount;

    void Awake()
    {
        m_moveInput = Vector2.zero;
        m_coinNum = new NetworkVariable<int>(0);
    }

    public override void OnNetworkSpawn()
    {
        //�R�C���擾���ω��ʒm
        m_coinNum.OnValueChanged += OnCoinNumChanged;
    }

    void Start()
    {
        // Rigidbody ���擾
        m_rigidBody = GetComponent<Rigidbody>();

        //UI�\��
        var canvas = GameObject.Find("Canvas").transform;
        m_coinCount = Instantiate(m_coinCountPrefabs, canvas).GetComponent<CoinCount>();
        m_coinCount.SetTarget(transform);
        m_coinCount.SetNumber(m_coinNum.Value);
    }

    private void Update()
    {
        //owner�̏ꍇ
        if (IsOwner)
        {
            // �ړ����͂�ݒ�
            SetMoveInputServerRpc(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical"));
        }

        //�T�[�o�[�i�z�X�g�j�̏ꍇ
        if (IsServer)
        {
            ServerUpdate();
        }
    }

    //�R�C��UI�X�V
    void OnCoinNumChanged(int prevValue, int newValue)
    {
        m_coinCount.SetNumber(newValue);
    }

    //=================================================================
    //RPC
    //=================================================================
    /// <summary>
    /// �ړ����͂��Z�b�g����RPC
    /// </summary>
    [ServerRpc]
    private void SetMoveInputServerRpc(float x, float y)
    {
        m_moveInput = new Vector2(x, y);
    }

    //=================================================================
    //�T�[�o�[���ōs������
    //=================================================================
    /// <summary>
    /// �T�[�o�[���ŌĂ΂��Update
    /// </summary>
    private void ServerUpdate()
    {
        //�ړ�
        var velocity = Vector3.zero;
        velocity.x = m_moveSpeed * m_moveInput.normalized.x;
        velocity.z = m_moveSpeed * m_moveInput.normalized.y;
        //�ړ�����
        m_rigidBody.AddForce(velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsServer == false) { return; }
        if (other.gameObject.CompareTag("Coin"))
        {
            //�擾����
            m_coinNum.Value += 1;
            //�R�C���폜�����iGameManager���̏������Ăԁj
            GameProc.Instance.DeleteCoin(other.gameObject);
        }
    }
}