using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("�� ���� UI")]
    public GameObject createRoomInputFieldObj;             // RoomName_InputField ������Ʈ
    public GameObject createRoomSubmitButtonObj;           // Check_CreateInputButton ������Ʈ
    public TMP_InputField roomNameInput;                   // �Է� �ʵ�

    [Header("���� ��� �ؽ�Ʈ")]
    public TextMeshProUGUI statusText;

    [Header("�� ���� UI")]
    public GameObject joinRoomInputFieldObj;
    public GameObject joinRoomSubmitButtonObj;
    public TMP_InputField joinRoomNameInput;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "���� ���� ��...";

        // ó���� �Է�â/��ư�� ���α�
        createRoomInputFieldObj.SetActive(false);
        createRoomSubmitButtonObj.SetActive(false);
        joinRoomInputFieldObj.SetActive(false);
        joinRoomSubmitButtonObj.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "���� ���� ����! �κ� ���� ��� ��...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "�κ� ���� �Ϸ�!";
    }

    // ------------------------------
    // �� �� ���� UI ����
    // ------------------------------
    public void ShowCreateUI()
    {
        createRoomInputFieldObj.SetActive(true);
        createRoomSubmitButtonObj.SetActive(true);
        roomNameInput.text = "";
        roomNameInput.Select();
    }

    // ------------------------------
    // �� �� ���� �õ�
    // ------------------------------
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

    // ------------------------------
    // �� ���� UI ����
    // ------------------------------
    public void ShowJoinUI()
    {
        joinRoomInputFieldObj.SetActive(true);
        joinRoomSubmitButtonObj.SetActive(true);
        joinRoomNameInput.text = "";
        joinRoomNameInput.Select();
    }

    // ------------------------------
    // �� ���� �õ�
    // ------------------------------
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

    // ------------------------------
    // �� �ݹ�
    // ------------------------------
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
        statusText.text = $"�� '{PhotonNetwork.CurrentRoom.Name}' ���� �Ϸ�!";
        PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"���� ������ ������ϴ�: {cause}";
    }
}
