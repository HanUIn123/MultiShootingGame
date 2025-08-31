using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    [Header("진행도 UI")]
    public Image progressFill;
    public float stageDuration = 60f;

    [Header("Warning UI")]
    public GameObject warningSign;
    public float warningDuration = 3f;
    public float blinkInterval = 0.3f;

    [Header("보스 소환")]
    public Vector3 bossSpawnPosition = new Vector3(0f, 6.5f, 0f);
    public string bossPrefabName = "Boss/BossPrefab";

    [Header("보스 체력바 UI")]
    public GameObject bossHpPanel;
    public Image bossHpFillImage;

    [Header("몬스터 스포너 연결")]
    [SerializeField] private MonsterSpawner monsterSpawner;

    private double startTime = -1;
    private bool warningTriggered = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            photonView.RPC(nameof(SetStartTime), RpcTarget.AllBuffered, startTime);
        }
    }

    [PunRPC]
    void SetStartTime(double time)
    {
        startTime = time;
    }

    void Update()
    {
        if (startTime < 0)
            return;

        double elapsed = PhotonNetwork.Time - startTime;
        float p = Mathf.Clamp01((float)(elapsed / stageDuration));
        UpdateFill(p);

        if (PhotonNetwork.IsMasterClient && !warningTriggered && p >= 1f)
        {
            warningTriggered = true;
            photonView.RPC(nameof(ShowWarning), RpcTarget.AllBuffered);
        }
    }

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

        if (PhotonNetwork.IsMasterClient)
        {
            GameObject boss = PhotonNetwork.Instantiate(bossPrefabName, bossSpawnPosition, Quaternion.identity);
            BossController bc = boss.GetComponent<BossController>();

            bc.hpFillImage = bossHpFillImage;
            bc.bossHpPanel = bossHpPanel;

            bc.photonView.RPC("InitBossUI", RpcTarget.AllBuffered);
            bc.StartBossBattle();

            if (monsterSpawner == null)
                monsterSpawner = FindFirstObjectByType<MonsterSpawner>();

            if (monsterSpawner != null)
                monsterSpawner.StopSpawning();
        }
    }
}
