using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltimateUIManager : MonoBehaviour
{
    public Image ultimateGauge;
    public RectTransform cutInRect; // 이미지 → RectTransform
    public float slideDuration = 0.3f;
    public float displayDuration = 0.8f;

    public Vector2 startOffset = new Vector2(800f, 400f); // 우상단 오프셋
    public Vector2 endPosition = Vector2.zero; // 중앙 고정
    public System.Action onCutInFinished;

    private Vector2 originalAnchoredPos;

    public void UpdateGauge(float value)
    {
        if (ultimateGauge != null)
            ultimateGauge.fillAmount = value;
    }

    public void PlayCutIn()
    {
        if (cutInRect == null) return;

        // 저장된 원래 위치로 설정
        originalAnchoredPos = cutInRect.anchoredPosition;

        StopAllCoroutines();
        StartCoroutine(PlayCutInRoutine());
    }

    private IEnumerator PlayCutInRoutine()
    {
        cutInRect.gameObject.SetActive(true);

        // 우상단 바깥에서 시작
        cutInRect.anchoredPosition = endPosition + startOffset;

        // 등장 연출: 슬라이드 인
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            cutInRect.anchoredPosition = Vector2.Lerp(endPosition + startOffset, endPosition, t / slideDuration);
            yield return null;
        }

        // 위치 보정
        cutInRect.anchoredPosition = endPosition;

        // 동시에 진동 애니메이션
        StartCoroutine(ShakeImage(cutInRect, displayDuration, 3f));

        // 잠시 대기
        yield return new WaitForSeconds(displayDuration);

        // 퇴장 연출: 슬라이드 아웃
        t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            cutInRect.anchoredPosition = Vector2.Lerp(endPosition, endPosition - startOffset, t / slideDuration);
            yield return null;
        }

        cutInRect.gameObject.SetActive(false);

        // 궁극기 발사
        onCutInFinished?.Invoke();
    }

    private IEnumerator ShakeImage(RectTransform target, float duration, float magnitude)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    public void OnChargeButtonPressed()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.OnChargeButtonPressed();
    }

    public void OnChargeButtonReleased()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.OnChargeButtonReleased();
    }
}
