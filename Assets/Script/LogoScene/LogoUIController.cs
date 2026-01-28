using System.Collections;
using UnityEngine;

public class LogoUIController : MonoBehaviour
{
    [Header("UI Groups")]
    [SerializeField] private CanvasGroup m_pLogoGroup = null;    
    [SerializeField] private CanvasGroup m_pCreateGroup = null;
    [SerializeField] private GameObject m_pStatusText = null;

    [Header("Settings")]
    [SerializeField] private float m_fFadeDuration = 1.0f;    
    [SerializeField] private float m_fLogoDelay = 0.5f;
    [SerializeField] private float m_fButtonsDelay = 1.0f;

    private void Start()
    {
        if(m_pLogoGroup == null || m_pCreateGroup == null || m_pStatusText == null)
        {
            Debug.LogError("m_pLogoGroup,m_pCreateGroup ,m_pStatusText is Null");
            return;
        }

        ReadyUIStates();
        StartCoroutine(ShowInitialMenu());
    }

    private void ReadyUIStates()
    {
        m_pLogoGroup.alpha = 0f;
        m_pCreateGroup.alpha = 0f;
        m_pStatusText.SetActive(false);
    }

    IEnumerator ShowInitialMenu()
    {
        yield return new WaitForSeconds(m_fLogoDelay);
        yield return StartCoroutine(Fade_In(m_pLogoGroup));

        yield return new WaitForSeconds(m_fButtonsDelay);
        m_pStatusText.SetActive(true);
        yield return StartCoroutine(Fade_In(m_pCreateGroup));
    }

    IEnumerator Fade_In(CanvasGroup _pTargetGroup)
    {
        float fTimer = 0f;

        while (fTimer < m_fFadeDuration)
        {
            fTimer += Time.deltaTime;

            float fRatio = fTimer / m_fFadeDuration;
            _pTargetGroup.alpha = fRatio;

            yield return null;
        }

        _pTargetGroup.alpha = 1.0f;
    }
}