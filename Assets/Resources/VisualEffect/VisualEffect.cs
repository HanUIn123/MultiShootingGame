using UnityEngine;
using System.Collections;

public class VisualEffect : MonoBehaviour
{
    private SpriteRenderer                              compSpriteRenderer;
    private Material                                    m_matMonster; // 실제 조절할 메테리얼

    private static readonly int                         DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int                         WhiteAmountID = Shader.PropertyToID("_WhiteAmount");

    [SerializeField] private Material                   m_pDissolveMaterial;

    private Coroutine                                   m_coHitFlash;

    private void Awake()
    {
        // Monster에 붙은 SpriteRenderer를 찾아옴.
        compSpriteRenderer = GetComponent<SpriteRenderer>();

        // 셰이더 그래프가 적용된 메테리얼을 개별 인스턴스로 가져옵니다.
        // 이렇게 해야 같은 몬스터라도 자기만 반짝이고 자기만 사라집니다.
        if (compSpriteRenderer != null)
        {
            m_matMonster = compSpriteRenderer.material;
        }
    }


    // 피격 시 하얗게 반짝이는 함수
    public void StartHitFlash()
    {
        if (m_coHitFlash != null) StopCoroutine(m_coHitFlash);
        m_coHitFlash = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        m_matMonster.SetFloat(WhiteAmountID, 1f); // 흰색 ON
        yield return new WaitForSeconds(0.08f);   // 찰나의 시간
        m_matMonster.SetFloat(WhiteAmountID, 0f); // 흰색 OFF
        m_coHitFlash = null;
    }

    // 외부에서 디졸브 호출하게 끔, 
    public void PlayDissolve(float fDuration, System.Action onComplete = null)
    {
        StopAllCoroutines();

        StartCoroutine(DissolveRoutine(fDuration, onComplete));
    }

    private IEnumerator DissolveRoutine(float fDuration, System.Action onComplete)
    {
        float fElapsed = 0f;

        while (fElapsed < fDuration)
        {
            fElapsed += Time.deltaTime;
            float fAmount = Mathf.Lerp(0f, 1f, fElapsed / fDuration);
            m_matMonster.SetFloat(DissolveAmountID, fAmount);
            yield return null;
        }

        onComplete?.Invoke();
    }

    // 다시 풀에서 나올 때를 위해 리셋 함수
    public void ResetVisuals()
    {
        if (m_matMonster != null)
        {
            m_matMonster.SetFloat(WhiteAmountID, 0f);
            m_matMonster.SetFloat(DissolveAmountID, 0f);
        }
    }
}