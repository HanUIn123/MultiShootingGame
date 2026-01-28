using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BossAttack : MonoBehaviourPunCallbacks
{
    public string                   m_strBulletPrefabPath = "Boss/BossBulletPrefab";
    public Transform                m_trFirePoint;

    // 원 패턴
    public IEnumerator Pattern_Circle()
    {
        // FirePoint or 보스 중심점 
        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

        int iBulletCount = 20;

        float fPatternAngle = 360f / iBulletCount;

        for (int i = 0; i < iBulletCount; i++)
        {
            float fAngle = i * fPatternAngle * Mathf.Deg2Rad;

            Vector3 v3Dir = new Vector3(Mathf.Cos(fAngle), Mathf.Sin(fAngle), 0f);

            FireBossBullet(v3SpawnPos, v3Dir);
        }
        yield return null;
    }

    // 나선 패턴
    public IEnumerator Pattern_Spiral()
    {
        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

        int iBulletCount = 36;

        float fStartAngle = -90f;

        for (int i = 0; i < iBulletCount; i++)
        {
            float fAngle = fStartAngle + i * 10f;

            float fRadian = fAngle * Mathf.Deg2Rad;

            Vector3 v3Dir = new Vector3(Mathf.Cos(fRadian), Mathf.Sin(fRadian), 0f);

            FireBossBullet(v3SpawnPos, v3Dir);

            yield return new WaitForSeconds(0.05f);
        }
    }

    // 샷건 탄환 패턴 
    public IEnumerator Pattern_Shotgun()
    {
        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

        int iBulletCount = 10;

        float fSpreadAngle = 45f;

        float fBaseAngle = -fSpreadAngle / 2f;

        for (int i = 0; i < iBulletCount; i++)
        {
            float fAngle = fBaseAngle + i * (fSpreadAngle / (iBulletCount - 1));

            float fRadian = fAngle * Mathf.Deg2Rad;

            Vector3 v3Dir = new Vector3(Mathf.Sin(fRadian), -Mathf.Cos(fRadian), 0f);

            FireBossBullet(v3SpawnPos, v3Dir);
        }
        yield return null;
    }

    private void FireBossBullet(Vector3 v3SpawnPos, Vector3 v3Dir)
    {
        if (!PhotonNetwork.IsMasterClient) 
            return;

        GameObject bulletObj = PhotonNetwork.Instantiate(m_strBulletPrefabPath, v3SpawnPos, Quaternion.identity);

        bulletObj.GetComponent<BossBullet>()?.SetDirection(v3Dir);
    }
}