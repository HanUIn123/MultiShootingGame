using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private Animator m_pMaskAnimator = null;
    [SerializeField] private float m_fTransitionTime = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
          
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeSceneWithMask(string szSceneName)
    {
        if (m_pMaskAnimator == null)
        {
            m_pMaskAnimator.gameObject.transform.root.gameObject.SetActive(true);
        }
        StartCoroutine(TransitionRoutine(szSceneName));
    }

    private IEnumerator TransitionRoutine(string szSceneName)
    {
        // 조리개 닫기
        m_pMaskAnimator.SetTrigger("tClose");
        yield return new WaitForSeconds(m_fTransitionTime);

        //// 씬 로드
        //AsyncOperation pAsyncLoad = SceneManager.LoadSceneAsync(szSceneName);
        //while (!pAsyncLoad.isDone) yield return null;

        //// 새 씬의 카메라를 찾아서 조리개 캔버스에 넣어줌.
        //Canvas pCanvas = m_pMaskAnimator.GetComponentInParent<Canvas>();

        //if (pCanvas != null)
        //{
        //    pCanvas.worldCamera = Camera.main; // 새 씬의 메인 카메라를 찾고,
        //    pCanvas.planeDistance = 1;         // 카메라 바로 앞에 붙이기.
        //}

        //// 조리개 열기
        //m_pMaskAnimator.SetTrigger("tOpen");

        // --- 예외 처리 구간 ---
        // 이동할 씬 이름이 있을 때만 유니티 싱글용 씬 로드를 실행
        if (!string.IsNullOrEmpty(szSceneName))
        {
            AsyncOperation pAsyncLoad = SceneManager.LoadSceneAsync(szSceneName);
            while (!pAsyncLoad.isDone) yield return null;

            // 새 씬 카메라 연결
            Canvas pCanvas = m_pMaskAnimator.GetComponentInParent<Canvas>();
            if (pCanvas != null)
            {
                pCanvas.worldCamera = Camera.main;
                pCanvas.planeDistance = 1;
            }

            // 조리개 열기 (이동한 씬에서)
            m_pMaskAnimator.SetTrigger("tOpen");
        }
        else
        {
            // 씬 이름이 없다면? 아무것도 안 하고 여기서 코루틴 종료.
            // (조리개가 닫힌 상태로 유지됨 -> 이후 GameSceneManager에서 LoadLevel 실행)
            Debug.Log("Scene name is empty. Just closing the mask.");
        }
    }

    public void PlayOpenTransition()
    {
        if (m_pMaskAnimator != null)
            m_pMaskAnimator.SetTrigger("tOpen");
    }
}