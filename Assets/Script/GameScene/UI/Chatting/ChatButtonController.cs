using UnityEngine;

public class ChatButtonController : MonoBehaviour
{
    public GameObject                                       m_TargetPanel;

    public void TogglePanel()
    {
        if (m_TargetPanel != null)
        {
            bool isActive = !m_TargetPanel.activeSelf;
            m_TargetPanel.SetActive(isActive);

            // 현재 씬에 있는 '내' 플레이어를 찾아서 이동을 멈추게 합니다.
            StopMyPlayer(isActive);
        }
    }

    private void StopMyPlayer(bool isChatting)
    {
        // 씬 내의 모든 PlayerController 중 내 것(PhotonView.IsMine)을 찾음
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.photonView.IsMine)
            {
                // 채팅 중이면 입력을 비활성화, 아니면 활성화
                // (PlayerController 내부의 m_compInput에 접근할 수 있도록 해당 변수나 함수를 public으로 열어줘야 할 수 있습니다)
                if (isChatting)
                    player.SendMessage("OnDisable"); // 혹은 별도의 가동중지 함수 호출
                else
                    player.SendMessage("OnEnable");
            }
        }
    }
}
