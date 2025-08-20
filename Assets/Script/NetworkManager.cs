using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("방 생성 UI")]
    public GameObject createRoomInputFieldObj;             // RoomName_InputField 오브젝트
    public GameObject createRoomSubmitButtonObj;           // Check_CreateInputButton 오브젝트
    public TMP_InputField roomNameInput;                   // 입력 필드

    [Header("상태 출력 텍스트")]
    public TextMeshProUGUI statusText;

    [Header("방 참가 UI")]
    public GameObject joinRoomInputFieldObj;
    public GameObject joinRoomSubmitButtonObj;
    public TMP_InputField joinRoomNameInput;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "서버 연결 중...";

        // 처음엔 입력창/버튼들 꺼두기
        createRoomInputFieldObj.SetActive(false);
        createRoomSubmitButtonObj.SetActive(false);
        joinRoomInputFieldObj.SetActive(false);
        joinRoomSubmitButtonObj.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "서버 연결 성공! 로비 입장 대기 중...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "로비 입장 완료!";
    }

    // ------------------------------
    // ▶ 방 생성 UI 열기
    // ------------------------------
    public void ShowCreateUI()
    {
        createRoomInputFieldObj.SetActive(true);
        createRoomSubmitButtonObj.SetActive(true);
        roomNameInput.text = "";
        roomNameInput.Select();
    }

    // ------------------------------
    // ▶ 방 생성 시도
    // ------------------------------
    public void SubmitCreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "방 이름을 입력하세요!";
            return;
        }

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        statusText.text = $"'{roomName}' 방 생성 시도 중...";
    }

    // ------------------------------
    // ▶ 참가 UI 열기
    // ------------------------------
    public void ShowJoinUI()
    {
        joinRoomInputFieldObj.SetActive(true);
        joinRoomSubmitButtonObj.SetActive(true);
        joinRoomNameInput.text = "";
        joinRoomNameInput.Select();
    }

    // ------------------------------
    // ▶ 참가 시도
    // ------------------------------
    public void SubmitJoinRoom()
    {
        string roomName = joinRoomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "입장할 방 이름을 입력하세요!";
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        statusText.text = $"'{roomName}' 방 입장 시도 중...";
    }

    // ------------------------------
    // ▶ 콜백
    // ------------------------------
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"방 생성 실패: {message}";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"방 입장 실패: {message}";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"방 '{PhotonNetwork.CurrentRoom.Name}' 입장 완료!";
        PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"서버 연결이 끊겼습니다: {cause}";
    }
}
