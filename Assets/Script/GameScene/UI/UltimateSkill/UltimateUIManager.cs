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

        // ���� �ٱ����� ��������, ȭ�� �߾Ӱ� + ���� ��ġ �� ���ؼ�, 
        cutInRect.anchoredPosition = endPosition + startOffset;

        float fTime = 0f;
        while (fTime < slideDuration)
        {
            fTime += Time.deltaTime;

            cutInRect.anchoredPosition = Vector2.Lerp(endPosition + startOffset, endPosition, fTime / slideDuration);

            yield return null;
        }

        // ���� ���⼭ cutInRect��, ȭ�� �߾��� �ǹ���.
        cutInRect.anchoredPosition = endPosition;

        // ���ÿ� ���� �ִϸ��̼�
        StartCoroutine(ShakeImage(cutInRect, displayDuration, 3f));

        // ��� ���
        yield return new WaitForSeconds(displayDuration);

        // ȭ�� �߾ӿ��� ���� startOffset �� ���� �ݴ�������� ������?
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
