using UnityEngine;
using Photon.Pun;

public class BossMovement : MonoBehaviourPunCallbacks
{
    public float                m_fMoveSpeed = 2.0f;
    public float                m_fMoveRangeX = 2.5f;
    private Vector3             m_v3TargetPos;

    void Start()
    {
        m_v3TargetPos = transform.position;
    }

    void Update()
    {
        // 모든 클라이언트가 타겟 포지션으로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, m_v3TargetPos, Time.deltaTime * m_fMoveSpeed);
    }

    // 방장이 새로운 목적지를 정할 때 호출
    public void SetNewRandomTarget()
    {
        if (!PhotonNetwork.IsMasterClient) 
            return;

        float fRandomX = Random.Range(-m_fMoveRangeX, m_fMoveRangeX);

        Vector3 v3NextPos = new Vector3(fRandomX, transform.position.y, 0f);

        photonView.RPC("RPC_UpdateTargetPos", RpcTarget.All, v3NextPos);
    }

    [PunRPC]
    void RPC_UpdateTargetPos(Vector3 v3TargetPos)
    {
        m_v3TargetPos = v3TargetPos;
    }
}