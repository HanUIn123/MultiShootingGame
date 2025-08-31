using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections;  
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("방 생성 UI")]
    public GameObject createRoomInputFieldObj;
    public GameObject createRoomSubmitButtonObj;
    public TMP_InputField roomNameInput;

    [Header("상태 출력 텍스트")]
    public TextMeshProUGUI statusText;

    [Header("방 참가 UI")]
    public GameObject joinRoomInputFieldObj;
    public GameObject joinRoomSubmitButtonObj;
    public TMP_InputField joinRoomNameInput;

    void Start()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "플레이어#" + Random.Range(1000, 9999);
        }

        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "서버 연결 중...";

        createRoomInputFieldObj.SetActive(false);
        createRoomSubmitButtonObj.SetActive(false);
        joinRoomInputFieldObj.SetActive(false);
        joinRoomSubmitButtonObj.SetActive(false);

        StartCoroutine(PlayStartBGM());
    }

    private IEnumerator PlayStartBGM()
    {
        yield return new WaitForSeconds(1f); // 1초 대기

        //AudioClip clip = SoundManager.Instance.LoadClip("BGM");
        //SoundManager.Instance.PlayBGM(clip);

        SoundManager.Instance.PlayBGM("BGM");
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "서버 연결 성공! 로비 입장 중...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "로비 입장 완료!";
    }

    public void ShowCreateUI()
    {
        createRoomInputFieldObj.SetActive(true);
        createRoomSubmitButtonObj.SetActive(true);
        roomNameInput.text = "";
        roomNameInput.Select();
    }

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

    public void ShowJoinUI()
    {
        joinRoomInputFieldObj.SetActive(true);
        joinRoomSubmitButtonObj.SetActive(true);
        joinRoomNameInput.text = "";
        joinRoomNameInput.Select();
    }

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
        statusText.text = $"'{PhotonNetwork.CurrentRoom.Name}' 방 입장 완료!";

        // 이 오브젝트 씬 전환 후에도 살아있게 유지
        DontDestroyOnLoad(this.gameObject);

        // 씬 로드 전에 동기화 옵션 켜기 (꼭 필요!)
        PhotonNetwork.AutomaticallySyncScene = true;

        // 게임 씬으로 전환
        SceneManager.LoadScene("GameScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"서버 연결 끊김: {cause}";
    }
}
