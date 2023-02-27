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
        //�v���C���[�̃v���n�u
        [SerializeField] private NetworkObject m_playerPrefab;

        public override void OnNetworkSpawn()
        {
            //�z�X�g�̏ꍇ
            if (IsHost)
            {
                //�N���C�A���g�ڑ���
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                //���łɑ��݂���N���C�A���g�p�Ɋ֐��Ăяo��
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
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
    }
}