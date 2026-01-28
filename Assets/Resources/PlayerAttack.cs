//using UnityEngine;
//using Photon.Pun;
//using System.Collections.Generic;

//public class PlayerAttack : MonoBehaviourPun
//{
//    [Header("공격 설정")]
//    [SerializeField] private Transform trFirePoint;
//    private float m_fFireCooldown = 0.1f;
//    private float m_fMinFireCooldown = 0.05f;
//    private float m_fLastFireTime;

//    private List<Transform> m_targetList = new List<Transform>();
//    private Animator m_animator;

//    private void Awake() => m_animator = GetComponent<Animator>();

//    public void SetCooldown(float cd) => m_fFireCooldown = Mathf.Max(m_fMinFireCooldown, cd);
//    public float GetCooldown() => m_fFireCooldown;

//    public void HandleAutoFire(int charType, float attackLevel, float speedLevel, bool isFever)
//    {
//        if (isFever) return;

//        if (Time.time - m_fLastFireTime >= m_fFireCooldown)
//        {
//            m_fLastFireTime = Time.time;
//            float fDmg = 50f + (attackLevel * 100f);

//            if (charType == 2) // 레이저 타입
//            {
//                float fRange = 4f + (speedLevel * 4f);
//                Transform[] targets = GetNearestEnemies(fRange);
//                if (targets.Length > 0) FireLaserMulti(targets, attackLevel, fDmg);
//            }
//            else // 일반 기체
//            {
//                if (m_animator) m_animator.SetTrigger("tAttack");
//                FireBullet(attackLevel, fDmg);
//            }
//        }
//    }

//    private Transform[] GetNearestEnemies(float fRange)
//    {
//        m_targetList.Clear();
//        Vector3 myPos = transform.position;
//        float fRangeSq = fRange * fRange;
//        var allMonsters = MonsterManager.AllMonsters;

//        for (int i = 0; i < allMonsters.Count; i++)
//        {
//            if (allMonsters[i] == null) continue;
//            float fDiffY = allMonsters[i].position.y - myPos.y;
//            if (fDiffY < -0.5f || fDiffY > fRange) continue;

//            if (((Vector2)allMonsters[i].position - (Vector2)myPos).sqrMagnitude <= fRangeSq)
//                m_targetList.Add(allMonsters[i]);
//        }
//        m_targetList.Sort((a, b) => (a.position - myPos).sqrMagnitude.CompareTo((b.position - myPos).sqrMagnitude));
//        return m_targetList.ToArray();
//    }

//    private void FireLaserMulti(Transform[] targets, float attackLevel, float damage)
//    {
//        if (!trFirePoint) return;
//        BendLaser[] allLasers = FindObjectsByType<BendLaser>(FindObjectsSortMode.None);
//        int myCount = 0;
//        foreach (var l in allLasers) if (l.photonView.IsMine) myCount++;

//        int max = attackLevel >= 0.7f ? 3 : (attackLevel >= 0.3f ? 2 : 1);
//        int spawnLimit = Mathf.Min(max - myCount, targets.Length);

//        for (int i = 0; i < spawnLimit; i++)
//        {
//            GameObject obj = PhotonNetwork.Instantiate("BendLaserPrefab", trFirePoint.position, Quaternion.identity);
//            var lPV = obj.GetComponent<PhotonView>();
//            var tPV = targets[i].GetComponent<PhotonView>();
//            if (lPV && tPV) lPV.RPC("RPC_SetupLaser", RpcTarget.All, photonView.ViewID, tPV.ViewID, damage);
//        }
//    }

//    private void FireBullet(float attackLevel, float damage)
//    {
//        if (!trFirePoint) return;
//        if (attackLevel < 0.3f) CreateBullet(trFirePoint.position, trFirePoint.rotation, damage);
//        else if (attackLevel < 0.7f)
//        {
//            CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, 15f), damage);
//            CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, -15f), damage);
//        }
//        else
//        {
//            CreateBullet(trFirePoint.position, trFirePoint.rotation, damage);
//            CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, 25f), damage);
//            CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, -25f), damage);
//        }
//        if (SoundManager.Instance) SoundManager.Instance.PlaySFX("BulletSound");
//    }

//    private void CreateBullet(Vector3 pos, Quaternion rot, float dmg)
//    {
//        GameObject obj = PhotonNetwork.Instantiate("BulletPrefab", pos, rot);
//        Bullet b = obj.GetComponent<Bullet>();
//        if (b != null) b.SetDamage(dmg);
//    }
//}