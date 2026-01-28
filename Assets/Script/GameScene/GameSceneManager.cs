using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // TMP 사용을 위해 필수
using System.Collections;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    public static GameSceneManager Instance;

    void Awake()
    {
        if (Instance == null) 
            Instance = this;
    }

    // 오브젝트 숨기기 로직 
    public static void CheckAndHideObject(MonoBehaviour Targets)
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            if (Targets.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                spriteRenderer.enabled = false;

            // 물리 연산 강제 스탑
            if (Targets.TryGetComponent<Rigidbody2D>(out var rigidbody))
                rigidbody.simulated = false;

            // 충돌체 강제 off 
            if (Targets.TryGetComponent<Collider2D>(out var collider2D))
                collider2D.enabled = false;

            foreach (Transform child in Targets.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    // --- 스테이지 클리어 연출 관련 ---
    // 보스 죽고나서.
    public void StartClearSequence()
    {
        // 모든 클라이언트에서 연출이 시작되도록 RPC 호출
        photonView.RPC("RPC_PlayStageClear", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayStageClear()
    {
        StartCoroutine(StageClearRoutine());
    }

    IEnumerator StageClearRoutine()
    {
        // UI 요소 찾기
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) 
            yield break;

        Transform trStageClearPanel = canvasObj.transform.Find("StageClear_Panel");
        if (trStageClearPanel == null) 
            yield break;

        GameObject clearPanelObj = trStageClearPanel.gameObject;

        // TextMeshProUGUI 컴포넌트 
        TextMeshProUGUI clearText = trStageClearPanel.Find("StageClearTEXT")?.GetComponent<TextMeshProUGUI>();

        if (clearText != null)
        {
            // 초기 세팅 -> 크기 10 
            clearPanelObj.SetActive(true);
            clearText.fontSize = 10f;

            // 투명도 조절 -> 발가잊게.
            Color textColor = clearText.color;
            textColor.a = 0f;
            clearText.color = textColor;

            // 190으로 크기 변화 ㄱㄱ 
            float fDuration= 0.6f; 
            float fElapsed = 0f;

            while (fElapsed < fDuration)
            {
                fElapsed += Time.deltaTime;

                float fPerncent= fElapsed / fDuration;

                // 폰트 크기와 투명도를 부드럽게 보간
                clearText.fontSize = Mathf.Lerp(10f, 190f, fPerncent);
                textColor.a = Mathf.Lerp(0f, 1f, fPerncent);
                clearText.color = textColor;

                yield return null;
            }

            // 마지막 값 고정
            clearText.fontSize = 190f;
            textColor.a = 1f;
            clearText.color = textColor;
        }

        // --- 조리개 연출 시작 ---
        // 텍스트 다 나오고 2초 스탑.
        yield return new WaitForSeconds(2.0f);

        // tclose 실행.
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ChangeSceneWithMask("");
        }

        // tclose 동안 대기.
        yield return new WaitForSeconds(1.0f);


        // 유니티의 시간 스케일을 0으로 만들어 모든 물리, Update, 코루틴(일부)을 멈춤
        Time.timeScale = 0f;
    }
}