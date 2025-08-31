using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltimateUIManager : MonoBehaviour
{
    public Image ultimateGauge;
    public RectTransform cutInRect; // �̹��� �� RectTransform
    public float slideDuration = 0.3f;
    public float displayDuration = 0.8f;

    public Vector2 startOffset = new Vector2(800f, 400f); // ���� ������
    public Vector2 endPosition = Vector2.zero; // �߾� ����
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

        // ����� ���� ��ġ�� ����
        originalAnchoredPos = cutInRect.anchoredPosition;

        StopAllCoroutines();
        StartCoroutine(PlayCutInRoutine());
    }

    private IEnumerator PlayCutInRoutine()
    {
        cutInRect.gameObject.SetActive(true);

        // ���� �ٱ����� ����
        cutInRect.anchoredPosition = endPosition + startOffset;

        // ���� ����: �����̵� ��
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            cutInRect.anchoredPosition = Vector2.Lerp(endPosition + startOffset, endPosition, t / slideDuration);
            yield return null;
        }

        // ��ġ ����
        cutInRect.anchoredPosition = endPosition;

        // ���ÿ� ���� �ִϸ��̼�
        StartCoroutine(ShakeImage(cutInRect, displayDuration, 3f));

        // ��� ���
        yield return new WaitForSeconds(displayDuration);

        // ���� ����: �����̵� �ƿ�
        t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            cutInRect.anchoredPosition = Vector2.Lerp(endPosition, endPosition - startOffset, t / slideDuration);
            yield return null;
        }

        cutInRect.gameObject.SetActive(false);

        // �ñر� �߻�
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
