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
        string msg = chatInputField.text;
        if (string.IsNullOrEmpty(msg)) return;

        string fullMsg = $"{PhotonNetwork.NickName}: {msg}";
        photonView.RPC("ReceiveChatMessage", RpcTarget.All, fullMsg);
        chatInputField.text = "";

        EventSystem.current.SetSelectedGameObject(null); // 포커스 해제
    }

    [PunRPC]
    void ReceiveChatMessage(string msg)
    {
        chatLogText.text += msg + "\n";
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;  // 한 프레임 기다림
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // 클래스 안에 이 함수 추가해
    public bool IsChatInputFocused()
    {
        return EventSystem.current.currentSelectedGameObject == chatInputField.gameObject;
    }

}
