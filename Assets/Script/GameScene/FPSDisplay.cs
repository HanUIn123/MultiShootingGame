using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    //float deltaTime = 0.0f;

    //void Update()
    //{
    //    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    //}

    //void OnGUI()
    //{
    //    int w = Screen.width, h = Screen.height;
    //    GUIStyle style = new GUIStyle();

    //    style.alignment = TextAnchor.UpperLeft;
    //    style.fontSize = h / 40;
    //    style.normal.textColor = Color.white;

    //    float msec = deltaTime * 1000.0f;
    //    float fps = 1.0f / deltaTime;
    //    string text = $"{msec:0.0} ms ({fps:0.} fps)";

    //    Rect rect = new Rect(10, 10, w, h / 20);
    //    GUI.Label(rect, text, style);
    //}

    [Header("표시 설정")]
    [SerializeField] private Color m_pTextColor = Color.cyan;
    [SerializeField] private int m_iFontSize = 25;

    private float m_fDeltaTime = 0.0f;

    private void Awake()
    {
        var objs = FindObjectsByType<FPSDisplay>(FindObjectsSortMode.None);
        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 프레임 간격 계산 (보정치 적용)
        m_fDeltaTime += (Time.unscaledDeltaTime - m_fDeltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int nW = Screen.width;
        int nH = Screen.height;

        GUIStyle pStyle = new GUIStyle();

        // 스타일 설정 
        pStyle.alignment = TextAnchor.UpperLeft;
        pStyle.fontSize = m_iFontSize; 
        pStyle.normal.textColor = m_pTextColor;

        float fMsec = m_fDeltaTime * 1000.0f;
        float fFps = 1.0f / m_fDeltaTime;

        // 문자열 포맷 최적화
        string strText = string.Format("{0:0.0} ms ({1:0.} fps)", fMsec, fFps);

        // 여백 10px 주고 화면에 출력
        Rect pRect = new Rect(15, 15, nW, nH / 20);
        GUI.Label(pRect, strText, pStyle);
    }
}
