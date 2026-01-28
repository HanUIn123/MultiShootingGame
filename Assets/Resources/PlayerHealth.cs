using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    [Header("체력 세팅")]
    public int                                          m_iMaxHP = 100;
    private int                                         m_iCurrentHP;

    private PlayerHealthUI                              m_pHealthUI;
    private PhotonView                                  m_pPv;

    private void Awake()
    {
        m_pPv = GetComponent<PhotonView>();

        m_iCurrentHP = m_iMaxHP;
    }

    private void Start()
    {
      
    }

    public void AssignUI(PlayerHealthUI pUi)
    {
        m_pHealthUI = pUi;

        if (m_pHealthUI != null)
        {
            m_pHealthUI.SetHP(m_iCurrentHP, m_iMaxHP);
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(float fDmg)
    {
        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP - (int)fDmg, 0, m_iMaxHP);

        // 팀원 화면에서 팀원 본인 피가 깎이게 하기 위해, IsMine 체크.
        if (photonView.IsMine)
        {
            if (m_pHealthUI != null)
            {
                m_pHealthUI.SetHP(m_iCurrentHP, m_iMaxHP);
            }
            else
            {
                // 만약 UI가 연결이 안 됐다면 강제로 찾아 넣기.
                m_pHealthUI = FindFirstObjectByType<PlayerHealthUI>();

                if (m_pHealthUI != null) 
                    m_pHealthUI.SetHP(m_iCurrentHP, m_iMaxHP);
            }
        }
    }

}