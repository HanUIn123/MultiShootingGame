using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager                           Instance;

    [Header("능력치 게이지 UI")]
    [SerializeField] private Image                          m_imgAttackGauge;
    [SerializeField] private Image                          m_imgSpeedGauge;

    private void Awake()
    {
        Instance = this;
    }

    // 공격력 게이지 UI 갱신
    public void UpdateAttackGauge(float fillAmount)
    {
        if (m_imgAttackGauge != null)
            m_imgAttackGauge.fillAmount = fillAmount;
    }

    // 스피드 게이지 UI 갱신 
    public void UpdateSpeedGauge(float fillAmount)
    {
        if (m_imgSpeedGauge != null)
            m_imgSpeedGauge.fillAmount = fillAmount;
    }
}