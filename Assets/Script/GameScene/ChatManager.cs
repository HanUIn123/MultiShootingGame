using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField chatInputField;
    public TMP_Text chatLogText;
    public ScrollRect scrollRect;

    void Start()
    {
        chatInputField.text = "";
    }

    public void OnClickSend()
    {
        string strMessage = chatInputField.text;

        if (string.IsNullOrEmpty(strMessage)) 
            return;

        string strFullMessage = $"{PhotonNetwork.NickName}: {strMessage}";

        photonView.RPC("ReceiveChatMessage", RpcTarget.All, strFullMessage);

        chatInputField.text = "";

        EventSystem.current.SetSelectedGameObject(null); 
    }

    [PunRPC]
    void ReceiveChatMessage(string strMessage)
    {
        chatLogText.text += strMessage + "\n";
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; 
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // 클래스 안에 이 함수 추가해
    public bool IsChatInputFocused()
    {
        return EventSystem.current.currentSelectedGameObject == chatInputField.gameObject;
    }

}
