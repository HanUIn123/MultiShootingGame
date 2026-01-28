using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text m_textMesh;

    private float m_fAlpha = 1f;    
    [SerializeField] private float m_fMoveSpeed = 3.0f; // 숫자가 너무 느리면 인스펙터에서 키우세요
    private float m_fDuration = 0.8f;

    void Awake()
    {
        if (m_textMesh == null)
            m_textMesh = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(float fAmount)
    {
        if (m_textMesh != null)
            m_textMesh.text = Mathf.FloorToInt(fAmount).ToString();

        Destroy(gameObject, m_fDuration);
    }

    void Update()
    {
        if (m_textMesh == null) return;

        // 위로 이동 (월드 좌표 기준)
        transform.position += Vector3.up * m_fMoveSpeed * Time.deltaTime;

        // 알파값 조절
        m_fAlpha -= Time.deltaTime / m_fDuration;
        Color c = m_textMesh.color;
        m_textMesh.color = new Color(c.r, c.g, c.b, Mathf.Max(0, m_fAlpha));
    }
}