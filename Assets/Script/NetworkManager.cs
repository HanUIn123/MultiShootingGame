using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections;  
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("�� ���� UI")]
    public GameObject createRoomInputFieldObj;
    public GameObject createRoomSubmitButtonObj;
    public TMP_InputField roomNameInput;

    [Header("���� ��� �ؽ�Ʈ")]
    public TextMeshProUGUI statusText;

    [Header("�� ���� UI")]
    public GameObject joinRoomInputFieldObj;
    public GameObject joinRoomSubmitButtonObj;
    public TMP_InputField joinRoomNameInput;

    void Start()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "�÷��̾�#" + Random.Range(1000, 9999);
        }

        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "���� ���� ��...";

        createRoomInputFieldObj.SetActive(false);
        createRoomSubmitButtonObj.SetActive(false);
        joinRoomInputFieldObj.SetActive(false);
        joinRoomSubmitButtonObj.SetActive(false);

        StartCoroutine(PlayStartBGM());
    }

    private IEnumerator PlayStartBGM()
    {
        yield return new WaitForSeconds(1f); // 1�� ���

        //AudioClip clip = SoundManager.Instance.LoadClip("BGM");
        //SoundManager.Instance.PlayBGM(clip);

        SoundManager.Instance.PlayBGM("BGM");
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "���� ���� ����! �κ� ���� ��...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "�κ� ���� �Ϸ�!";
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
            statusText.text = "�� �̸��� �Է��ϼ���!";
            return;
        }

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        statusText.text = $"'{roomName}' �� ���� �õ� ��...";
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
            statusText.text = "������ �� �̸��� �Է��ϼ���!";
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        statusText.text = $"'{roomName}' �� ���� �õ� ��...";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"�� ���� ����: {message}";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"�� ���� ����: {message}";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"'{PhotonNetwork.CurrentRoom.Name}' �� ���� �Ϸ�!";

        // �� ������Ʈ �� ��ȯ �Ŀ��� ����ְ� ����
        DontDestroyOnLoad(this.gameObject);

        // �� �ε� ���� ����ȭ �ɼ� �ѱ� (�� �ʿ�!)
        PhotonNetwork.AutomaticallySyncScene = true;

        // ���� ������ ��ȯ
        SceneManager.LoadScene("GameScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"���� ���� ����: {cause}";
    }
}
