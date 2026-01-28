using UnityEngine;

public class BossExplode : MonoBehaviour
{
    [Header("폭발 유지 시간")]
    public float m_fDestroyTime = 0.6f; 

    void Start()
    {
        Destroy(gameObject, m_fDestroyTime);
    }
}