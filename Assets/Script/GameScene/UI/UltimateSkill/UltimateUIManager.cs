using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltimateUIManager : MonoBehaviour
{
    public Image ultimateGauge;
    public RectTransform cutInRect; 
    public float slideDuration = 0.3f;
    public float displayDuration = 0.8f;

    public Vector2 startOffset = new Vector2(800f, 400f); 
    public Vector2 endPosition = Vector2.zero; 
    public System.Action onCutInFinished;

    private Vector2 originalAnchoredPos;

    public void UpdateGauge(float value)
    {
        if (ultimateGauge != null)
            ultimateGauge.fillAmount = value;
    }

    public void PlayCutIn()
    {
        if (cutInRect == null) 
            return;

        originalAnchoredPos = cutInRect.anchoredPosition;

        StopAllCoroutines();

        StartCoroutine(PlayCutInRoutine());
    }

    private IEnumerator PlayCutInRoutine()
    {
        cutInRect.gameObject.SetActive(true);

        // 우상단 바깥에서 시작하자, 화면 중앙값 + 시작 위치 값 더해서, 
        cutInRect.anchoredPosition = endPosition + startOffset;

        float fTime = 0f;
        while (fTime < slideDuration)
        {
            fTime += Time.deltaTime;

            cutInRect.anchoredPosition = Vector2.Lerp(endPosition + startOffset, endPosition, fTime / slideDuration);

            yield return null;
        }

        // 이제 여기서 cutInRect는, 화면 중앙이 되버림.
        cutInRect.anchoredPosition = endPosition;

        // 동시에 진동 애니메이션
        StartCoroutine(ShakeImage(cutInRect, displayDuration, 3f));

        // 잠시 대기
        yield return new WaitForSeconds(displayDuration);

        // 화면 중앙에서 이제 startOffset 을 빼면 반대방향으로 가겟지?
        fTime = 0f;
        while (fTime < slideDuration)
        {
            fTime += Time.deltaTime;

            cutInRect.anchoredPosition = Vector2.Lerp(endPosition, endPosition - startOffset, fTime / slideDuration);

            yield return null;
        }

        cutInRect.gameObject.SetActive(false);

        onCutInFinished?.Invoke();
    }

    private IEnumerator ShakeImage(RectTransform target, float fDuration, float fMagnitude)
    {
        Vector3 originalPos = target.localPosition;

        float fElapsed = 0f;

        while (fElapsed < fDuration)
        {
            float fOffsetX = Random.Range(-1f, 1f) * fMagnitude;
            float fOffsetY = Random.Range(-1f, 1f) * fMagnitude;
            target.localPosition = originalPos + new Vector3(fOffsetX, fOffsetY, 0f);

            fElapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
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
