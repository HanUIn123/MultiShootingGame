using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image                              m_pFrontFill = null; 
    [SerializeField] private Image                              m_pBackFill = null;  

    [SerializeField] private float                              m_fLerpSpeed = 0.05f;
    private Coroutine                                           m_pCoroutine = null;

    public void SetHP(int iCurrentHp, int iMaxHp)
    {
        float fTargetFill = (float)iCurrentHp / (float)iMaxHp;

        if (m_pFrontFill != null) 
            m_pFrontFill.fillAmount = fTargetFill;

        if (m_pCoroutine != null) 
            StopCoroutine(m_pCoroutine);

        m_pCoroutine = StartCoroutine(SmoothHpEffect(fTargetFill));
    }

    private IEnumerator SmoothHpEffect(float fTarget)
    {
        if (m_pBackFill == null) 
            yield break;

        while (m_pBackFill.fillAmount > fTarget)
        {
            m_pBackFill.fillAmount = Mathf.Lerp(m_pBackFill.fillAmount, fTarget, m_fLerpSpeed);

            if (m_pBackFill.fillAmount - fTarget < 0.001f)
            {
                m_pBackFill.fillAmount = fTarget;
                break;
            }
            yield return null;
        }
    }
}