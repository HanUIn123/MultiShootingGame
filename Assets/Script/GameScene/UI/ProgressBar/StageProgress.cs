using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    public Image progressFill;              // SP_Fill �̹���
    public float stageDuration = 30f;       // �������� �� �ҿ�ð�(��)

    [Header("Warning UI")]
    public GameObject warningSign;          // Inspector�� WarningSign ������Ʈ �巡��
    public float warningDuration = 3f;      // ��¦�̴� �ð�
    public float blinkInterval = 0.3f;      // ��¦�̴� �ӵ�

    private float t = 0.55f * 30f; // �Ǵ� stageDuration * 0.95f
    //private float t = 0f; // �Ǵ� stageDuration * 0.95f
    private bool warningTriggered = false;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / stageDuration);

        // UI Fill ������Ʈ
        photonView.RPC(nameof(UpdateFill), RpcTarget.All, p);

        if (!warningTriggered && p >= 1f)
        {
            warningTriggered = true;

            // ��� Ŭ�� Warning ����
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
            img.enabled = !img.enabled; // ��¦��
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        img.enabled = true; // �������� �ѵ�
    }
}
