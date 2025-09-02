using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHP = 100;
    private int currentHP;

    private PlayerHealthUI healthUI; 
    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        currentHP = maxHP;
    }

    public void AssignUI(PlayerHealthUI ui)
    {
        healthUI = ui;
    }


    [PunRPC]
    void RPC_TakeDamage(int dmg)
    {
        // 내 PhotonView가 내꺼일 때만
        if (!pv.IsMine) 
            return; 

        currentHP = Mathf.Clamp(currentHP - dmg, 0, maxHP);

        if (healthUI != null)
            healthUI.SetHP(currentHP);

        if (currentHP <= 0)
        {
            Debug.Log("Player Dead");
        }
    }
}
