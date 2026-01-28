//using UnityEngine;
//using Photon.Pun;

//public class PlayerMovement : MonoBehaviourPun
//{
//    private float m_fMoveSpeed = 6f;
//    private Vector2 v2MoveInput;
//    private ChatManager m_compChatManager;

//    public void SetSpeed(float speed) => m_fMoveSpeed = speed;
//    public float GetSpeed() => m_fMoveSpeed;

//    private void Awake()
//    {
//        m_compChatManager = FindFirstObjectByType<ChatManager>();
//    }

//    public void SetMoveInput(Vector2 input)
//    {
//        // 채팅 중이면 이동 멈춤
//        if (m_compChatManager != null && m_compChatManager.IsChatInputFocused())
//        {
//            v2MoveInput = Vector2.zero;
//            return;
//        }
//        v2MoveInput = input;
//    }

//    public void ProcessMovement()
//    {
//        if (!photonView.IsMine) return;

//        Vector3 v3NextPos = transform.position + new Vector3(v2MoveInput.x, v2MoveInput.y, 0f) * m_fMoveSpeed * Time.deltaTime;

//        // 화면 밖으로 못 나가게 가두기
//        v3NextPos.x = Mathf.Clamp(v3NextPos.x, -3.5f, 3.5f);
//        v3NextPos.y = Mathf.Clamp(v3NextPos.y, -4.6f, 4.6f);

//        transform.position = v3NextPos;
//    }
//}