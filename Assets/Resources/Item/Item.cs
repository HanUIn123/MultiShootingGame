using UnityEngine;
using Photon.Pun;

public class Item : MonoBehaviourPun
{
    public enum ItemType { PowerUp, SpeedUp, Gold, Health }

    [Header("아이템 설정")]
    [SerializeField] private ItemType                   m_eItemType;

    [Header("이동 설정")]
    [SerializeField] private float                      m_fItemFallingSpeed = 1.5f; 

    private void Update()
    {
        transform.Translate(Vector2.down * m_fItemFallingSpeed * Time.deltaTime);

        // 화면 밖에 나가면 방장 입장에서 처리 해서 팀원에게 알려줌. 
        if (PhotonNetwork.IsMasterClient && transform.position.y < -6.0f)
        {
            DestroySelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null && playerPV.IsMine)
            {
                ApplyItemEffect();

                if (PhotonNetwork.IsMasterClient) 
                    DestroySelf();
                else 
                    photonView.RPC("RPC_DestroyItem", RpcTarget.MasterClient);
            }
        }
    }

    private void ApplyItemEffect()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController myPlayer = null;

        foreach (var player in allPlayers)
        {
            // 진짜 내 입장의 플레이어로.
            if (player.photonView != null && player.photonView.IsMine)
            {
                myPlayer = player;
                break;
            }
        }

        if (myPlayer != null)
        {
            switch (m_eItemType)
            {
                case ItemType.PowerUp:
                    myPlayer.AddAttackPower(0.1f);
                    break;

                case ItemType.SpeedUp:
                    myPlayer.AddSpeedPower(0.05f);
                    break;
            }
        }
    }

    [PunRPC]
    private void RPC_DestroyItem()
    {
        if (PhotonNetwork.IsMasterClient) DestroySelf();
    }

    private void DestroySelf()
    {
        if (photonView != null && photonView.InstantiationId > 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}