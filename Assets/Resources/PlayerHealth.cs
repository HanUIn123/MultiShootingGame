using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHP = 100;
    private int currentHP;

    private PlayerHealthUI healthUI; // ���� ĵ������ HP��
    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        currentHP = maxHP;

        //// �� ���� �÷��̾��� ���� �� ���� UI�� ����
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
        //healthUI.SetHP(currentHP); // �� Ÿ�ֿ̹��� SetHP ȣ��
    }

    // ���� Ŭ���̾�Ʈ������ ü���� ��� RPC
    [PunRPC]
    void RPC_TakeDamage(int dmg)
    {
        if (!pv.IsMine) return; // ���ʸ� ����

        currentHP = Mathf.Clamp(currentHP - dmg, 0, maxHP);
        if (healthUI != null)
            healthUI.SetHP(currentHP);

        if (currentHP <= 0)
        {
            Debug.Log("Player Dead");
            // TODO: ��� ó��
        }
    }
}
