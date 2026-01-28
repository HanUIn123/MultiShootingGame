using UnityEngine;

public class SimpleBackgroundMove : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float m_fScrollSpeed = 100f;
    [SerializeField] private float m_fChangeInterval = 5f;
    [SerializeField] private float m_fMaxDistance = 500f; 

    private RectTransform m_RectTransform;
    private float fSwitchTimer = 0.0f;
    private Vector2 m_MoveDirection = Vector2.down;

    void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        fSwitchTimer += Time.deltaTime;

        if (fSwitchTimer >= m_fChangeInterval)
        {
            fSwitchTimer = 0.0f;
            m_MoveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        Vector2 pos = m_RectTransform.anchoredPosition;

        pos += m_MoveDirection * m_fScrollSpeed * Time.deltaTime;

        if (Mathf.Abs(pos.x) > m_fMaxDistance) 
            m_MoveDirection.x *= -1f;

        if (Mathf.Abs(pos.y) > m_fMaxDistance) 
            m_MoveDirection.y *= -1f;

        m_RectTransform.anchoredPosition = pos;
    }
}