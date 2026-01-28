using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltimateUIManager : MonoBehaviour
{
    public Image                                                        ultimateGaugeImg;
    public RectTransform                                                cutInRect; 
    public float                                                        m_fSlideDuration = 0.1f;
    public float                                                        m_fDisplayDuration = 0.3f;

    public Vector2                                                      m_v2StartOffset = new Vector2(800f, 400f); 
    public Vector2                                                      m_v2EndPosition = Vector2.zero; 
    public System.Action                                                onCutInFinished;

    public void UpdateGauge(float fAmount)
    {
        if (ultimateGaugeImg != null)
        {
            ultimateGaugeImg.fillAmount = fAmount;

            if (fAmount <= 0f)
            {
                //게이지 이미지만 끄기
                ultimateGaugeImg.gameObject.SetActive(false);
            }
            else
            {
                // 차징 시작하면 다시 보이기
                ultimateGaugeImg.gameObject.SetActive(true);
            }
        }
    }

    public void PlayCutIn()
    {
        if (cutInRect == null) 
            return;

        StopAllCoroutines();

        StartCoroutine(PlayCutInRoutine());
    }

    private IEnumerator PlayCutInRoutine()
    {
        cutInRect.gameObject.SetActive(true);

        // 우상단 -> 중앙 진입
        float fTime = 0f;
        Vector2 v2StartPos = m_v2EndPosition + m_v2StartOffset;

        while (fTime < m_fSlideDuration)
        {
            fTime += Time.deltaTime;
            float t = fTime / m_fSlideDuration;

            float tEntry = 1f - Mathf.Pow(1f - t, 4f);

            cutInRect.anchoredPosition = Vector2.Lerp(v2StartPos, m_v2EndPosition, tEntry);
            yield return null;
        }

        cutInRect.anchoredPosition = m_v2EndPosition;

        // 중앙에 도착해서 대기 후, 진동 
        yield return StartCoroutine(ShakeImage(cutInRect, m_fDisplayDuration, 15f));

        // 중앙 -> 좌하단 이동.
        fTime = 0f;
        Vector2 v2ExitPos = m_v2EndPosition - m_v2StartOffset;

        while (fTime < m_fSlideDuration)
        {
            fTime += Time.deltaTime;
            float t = fTime / m_fSlideDuration;

            // 나가는 것도 3제곱(t^3)으로 맞추면 들어올 때와 속도 밸런스를 맞춘다.
            float tExit = t * t * t;

            cutInRect.anchoredPosition = Vector2.Lerp(m_v2EndPosition, v2ExitPos, tExit);
            yield return null;
        }

        cutInRect.gameObject.SetActive(false);
        onCutInFinished?.Invoke();
    }

    private IEnumerator ShakeImage(RectTransform targetRect, float fDuration, float fMagnitude)
    {
        Vector2 v2CenterPos = targetRect.anchoredPosition;
        float fElapsed = 0f;

        while (fElapsed < fDuration)
        {
            fElapsed += Time.deltaTime;

            float fOffsetX = Random.Range(-1f, 1f) * fMagnitude;
            float fOffsetY = Random.Range(-1f, 1f) * fMagnitude;
            targetRect.anchoredPosition = v2CenterPos + new Vector2(fOffsetX, fOffsetY);

            yield return null;
        }

        targetRect.anchoredPosition = v2CenterPos;
    }

    public void OnChargeButtonPressed()
    {
        var playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
            playerController.OnChargeButtonPressed();
    }

    public void OnChargeButtonReleased()
    {
        var playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
            playerController.OnChargeButtonReleased();
    }
}
