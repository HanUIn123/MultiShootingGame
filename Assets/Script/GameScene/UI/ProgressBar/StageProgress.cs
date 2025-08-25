using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    [Header("진행도 UI")]
    public Image progressFill;              // SP_Fill 이미지
    public float stageDuration = 30f;       // 스테이지 총 소요시간(초)

    [Header("Warning UI")]
    public GameObject warningSign;          // Inspector에 WarningSign 오브젝트 드래그
    public float warningDuration = 3f;      // 반짝이는 시간
    public float blinkInterval = 0.3f;      // 반짝이는 속도

    [Header("보스 소환")]
    public Vector3 bossSpawnPosition = new Vector3(0f, 6.5f, 0f); // 충분히 위쪽
    public string bossPrefabName = "Boss/Boss";   // Resources/Boss 경로에 있는 프리팹 이름

    private float t = 0.95f * 30f;                    // 진행 시간
    private bool warningTriggered = false;

    void Update()
    {
        // MasterClient만 시간 진행 및 이벤트 실행
        if (!PhotonNetwork.IsMasterClient)
            return;

        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / stageDuration);

        // 모든 클라이언트에 진행도 UI 동기화
        photonView.RPC(nameof(UpdateFill), RpcTarget.All, p);

        // 경고 한번만 실행
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

        //  모든 MonsterSpawner 찾아서 스폰 멈춤 시킴 
        if (PhotonNetwork.IsMasterClient)
        {
            MonsterSpawner[] spawners = Object.FindObjectsByType<MonsterSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                spawner.StopSpawning();
            }

            // 그리고 보스 소환
            PhotonNetwork.Instantiate(bossPrefabName, bossSpawnPosition, Quaternion.identity);
        }
    }


}
