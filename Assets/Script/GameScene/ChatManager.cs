using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviourPunCallbacks
{
    [Header("Chat UI Settings")]
    [SerializeField] private TMP_InputField                 m_pChatInputField = null;
    [SerializeField] private TMP_Text                       m_pChatLogText = null;
    [SerializeField] private ScrollRect                     m_pScrollRect = null;

    void Start()
    {
        m_pChatInputField.text = "";
    }

    public void OnClickSend()
    {
        string strMessage = m_pChatInputField.text;

        if (string.IsNullOrEmpty(strMessage)) 
            return;

        string strFullMessage = $"{PhotonNetwork.NickName}: {strMessage}";

        photonView.RPC("ReceiveChatMessage", RpcTarget.All, strFullMessage);

        m_pChatInputField.text = "";

        EventSystem.current.SetSelectedGameObject(null); 
    }

    /*
     RPC 함수는 문자열("ReceiveChatMessage")로 넘겨서 
    포톤 시스템이 뒤에서 몰래 호출하기 때문에 에디터가 인식을 못 하는 겁
     */
    [PunRPC]
    void ReceiveChatMessage(string strMessage)
    {
        m_pChatLogText.text += strMessage + "\n";
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; 
        Canvas.ForceUpdateCanvases();
        m_pScrollRect.verticalNormalizedPosition = 0f;
    }

    public bool IsChatInputFocused()
    {
        return EventSystem.current.currentSelectedGameObject == m_pChatInputField.gameObject;
    }

}
