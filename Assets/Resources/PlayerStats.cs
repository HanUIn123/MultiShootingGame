//using UnityEngine;
//using Photon.Pun;

//public class PlayerStats : MonoBehaviourPun
//{
//    [Header("레벨 및 게이지")]
//    public float m_fCurrentAttackLevel = 0f;
//    public float m_fCurrentSpeedLevel = 0f;
//    public float m_fCurrentGauge = 0f;
//    public float m_fChargeSpeed = 1.5f;

//    public bool m_bIsCharging = false;
//    public bool m_bIsFevertime = false;

//    [Header("궁극기 설정")]
//    [SerializeField] private string m_strLaserPath = "UltimateLaser";
//    [SerializeField] private Transform trLaserSpawn;
//    public UltimateUIManager ultimateUI;

//    public void ProcessUltimate()
//    {
//        if (m_bIsCharging && !m_bIsFevertime)
//        {
//            m_fCurrentGauge = Mathf.Min(m_fCurrentGauge + Time.deltaTime * m_fChargeSpeed, 1f);
//            if (ultimateUI) ultimateUI.UpdateGauge(m_fCurrentGauge);
//            if (m_fCurrentGauge >= 1f) TriggerFevertime();
//        }
//    }

//    private void TriggerFevertime()
//    {
//        m_bIsFevertime = true;
//        m_bIsCharging = false;
//        if (ultimateUI)
//        {
//            ultimateUI.onCutInFinished = FinishFevertimeAndFire;
//            ultimateUI.PlayCutIn();
//        }
//        else FinishFevertimeAndFire();
//    }

//    private void FinishFevertimeAndFire()
//    {
//        m_bIsFevertime = false;
//        m_fCurrentGauge = 0f;
//        if (ultimateUI) ultimateUI.UpdateGauge(0f);

//        if (trLaserSpawn && photonView.IsMine)
//        {
//            PhotonNetwork.Instantiate(m_strLaserPath, trLaserSpawn.position, trLaserSpawn.rotation);
//            if (SoundManager.Instance) SoundManager.Instance.PlaySFX("FireSound");
//        }
//    }

//    public void InitLaserSpawn(Transform tr)
//    {
//        trLaserSpawn = tr;
//    }

//    public void ResetGauge()
//    {
//        if (m_bIsFevertime) return;
//        m_fCurrentGauge = 0f;
//        if (ultimateUI) ultimateUI.UpdateGauge(0f);
//    }
//}