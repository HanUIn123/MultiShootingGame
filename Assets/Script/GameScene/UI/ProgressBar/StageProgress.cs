using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    public Image progressFill;              // SP_Fill 이미지
    public float stageDuration = 30f;       // 스테이지 총 소요시간(초)

    [Header("Warning UI")]
    public GameObject warningSign;          // Inspector에 WarningSign 오브젝트 드래그
    public float warningDuration = 3f;      // 반짝이는 시간
    public float blinkInterval = 0.3f;      // 반짝이는 속도

    private float t = 0.55f * 30f; // 또는 stageDuration * 0.95f
    //private float t = 0f; // 또는 stageDuration * 0.95f
    private bool warningTriggered = false;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / stageDuration);

        // UI Fill 업데이트
        photonView.RPC(nameof(UpdateFill), RpcTarget.All, p);

        if (!warningTriggered && p >= 1f)
        {
            warningTriggered = true;

            // 모든 클라에 Warning 실행
            photonView.RPC(nameof(ShowWarning), RpcTarget.All);
        }
    }

    [PunRPC]
    void UpdateFill(float p)
    {
        if (progressFill != null)
            progressFill.fillAmount = p;
    }

    [PunRPC]
    void ShowWarning()
    {
        if (warningSign != null)
        {
            warningSign.SetActive(true);
            StartCoroutine(BlinkWarning());
        }
    }

    IEnumerator BlinkWarning()
    {
        float elapsed = 0f;
        Image img = warningSign.GetComponent<Image>();

        while (elapsed < warningDuration)
        {
            img.enabled = !img.enabled; // 반짝임
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        img.enabled = true; // 마지막엔 켜둠
    }
}
