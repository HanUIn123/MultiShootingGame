using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    [Header("���൵ UI")]
    public Image progressFill;              // SP_Fill �̹���
    public float stageDuration = 30f;       // �������� �� �ҿ�ð�(��)

    [Header("Warning UI")]
    public GameObject warningSign;          // Inspector�� WarningSign ������Ʈ �巡��
    public float warningDuration = 3f;      // ��¦�̴� �ð�
    public float blinkInterval = 0.3f;      // ��¦�̴� �ӵ�

    [Header("���� ��ȯ")]
    public Vector3 bossSpawnPosition = new Vector3(0f, 6.5f, 0f); // ����� ����
    public string bossPrefabName = "Boss/Boss";   // Resources/Boss ��ο� �ִ� ������ �̸�

    private float t = 0.95f * 30f;                    // ���� �ð�
    private bool warningTriggered = false;

    void Update()
    {
        // MasterClient�� �ð� ���� �� �̺�Ʈ ����
        if (!PhotonNetwork.IsMasterClient)
            return;

        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / stageDuration);

        // ��� Ŭ���̾�Ʈ�� ���൵ UI ����ȭ
        photonView.RPC(nameof(UpdateFill), RpcTarget.All, p);

        // ��� �ѹ��� ����
        if (!warningTriggered && p >= 1f)
        {
            warningTriggered = true;
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
            img.enabled = !img.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        img.enabled = false;
        warningSign.SetActive(false);

        //  ��� MonsterSpawner ã�Ƽ� ���� ���� ��Ŵ 
        if (PhotonNetwork.IsMasterClient)
        {
            MonsterSpawner[] spawners = Object.FindObjectsByType<MonsterSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                spawner.StopSpawning();
            }

            // �׸��� ���� ��ȯ
            PhotonNetwork.Instantiate(bossPrefabName, bossSpawnPosition, Quaternion.identity);
        }
    }


}
