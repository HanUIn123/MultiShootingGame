using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHP = 100;
    private int currentHP;

    private PlayerHealthUI healthUI; // 로컬 캔버스의 HP바
    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        currentHP = maxHP;

        //// 내 소유 플레이어일 때만 내 폰의 UI를 잡음
        //if (pv.IsMine)
        //{
        //    healthUI = Object.FindAnyObjectByType<PlayerHealthUI>();
        //    if (healthUI != null)
        //        healthUI.SetHP(currentHP);
        //}
    }

    public void AssignUI(PlayerHealthUI ui)
    {
        healthUI = ui;
        //healthUI.SetHP(currentHP); // 이 타이밍에만 SetHP 호출
    }

    // 오너 클라이언트에서만 체력을 깎는 RPC
    [PunRPC]
    void RPC_TakeDamage(int dmg)
    {
        if (!pv.IsMine) return; // 오너만 변경

        currentHP = Mathf.Clamp(currentHP - dmg, 0, maxHP);
        if (healthUI != null)
            healthUI.SetHP(currentHP);

        if (currentHP <= 0)
        {
            Debug.Log("Player Dead");
            // TODO: 사망 처리
        }
    }
}
