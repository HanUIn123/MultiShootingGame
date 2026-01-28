using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{   
    [Header("UI Groups - Create")]
    [SerializeField] private GameObject m_pCreateInputFieldObj = null;
    [SerializeField] private GameObject m_pCreateSubmitButtonObj = null;
    [SerializeField] private TMP_InputField m_pRoomNameInput = null;

    [Header("UI Groups - Join")]
    [SerializeField] private GameObject m_pJoinInputFieldObj = null;
    [SerializeField] private GameObject m_pJoinSubmitButtonObj = null;
    [SerializeField] private TMP_InputField m_pJoinRoomNameInput = null;

    [Header("UI Groups - Status")]
    [SerializeField] private TextMeshProUGUI m_pStatusText = null;

    [Header("Transition UI")]
    [SerializeField] private Animator m_pMaskAnimator = null;

    private void Start()
    {
        if (m_pCreateInputFieldObj == null || m_pJoinInputFieldObj == null || m_pStatusText == null)
        {
            Debug.LogError("m_pCreateInputFieldObj, m_pJoinInputFieldObj, m_pStatusText is null");

            return;
        }

        // 플레이이 닉네임(채팅창에서 보이는) 설정
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "PLAYER#" + Random.Range(1000, 9999);
        }

        // 시작하자마자 조리개를 여는 연출 (tOpen 실행)
        if (m_pMaskAnimator != null)
        {
            m_pMaskAnimator.transform.root.gameObject.SetActive(true);
            m_pMaskAnimator.SetTrigger("tOpen");
        }

        // 서버 접속 및 상태 업데이트
        PhotonNetwork.ConnectUsingSettings();
        m_pStatusText.text = "SERVER CONNECTING..";

        // 초기 로비씬 UI들 상테 셋팅
        Ready_Network_UI();

        StartCoroutine(Play_Start_BGM());
    }

    private void Ready_Network_UI()
    {
        m_pCreateInputFieldObj.SetActive(false);
        m_pCreateSubmitButtonObj.SetActive(false);
        m_pJoinInputFieldObj.SetActive(false);
        m_pJoinSubmitButtonObj.SetActive(false);
    }

    private IEnumerator Play_Start_BGM()
    {
        yield return new WaitForSeconds(1f);

        SoundManager.Instance.PlayBGM("BGM");
    }


    // --- 포톤 서버 콜백 함수들 ---
    public override void OnConnectedToMaster()
    {
        m_pStatusText.text = "SERVER LOGGIN SUCCESS! ENTERING LOBBY..";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        m_pStatusText.text = "SERVER LOGGIN COMPLETE..";
    }

    public void ShowCreateUI()
    {
        m_pCreateInputFieldObj.SetActive(true);
        m_pCreateSubmitButtonObj.SetActive(true);
        m_pRoomNameInput.text = "";
        m_pRoomNameInput.Select();
    }

    public void SubmitCreateRoom()
    {
        string szRoomName = m_pRoomNameInput.text;

        if (string.IsNullOrEmpty(szRoomName))
        {
            m_pStatusText.text = "방 이름을 입력하세요!";

            return;
        }

        RoomOptions pRoomOptions = new RoomOptions() { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(szRoomName, pRoomOptions);
        m_pStatusText.text = $"'{szRoomName}' CREATING SERVER...";
    }

    public void ShowJoinUI()
    {
        m_pJoinInputFieldObj.SetActive(true);
        m_pJoinSubmitButtonObj.SetActive(true);
        m_pJoinRoomNameInput.text = "";
        m_pJoinRoomNameInput.Select();
    }

    public void SubmitJoinRoom()
    {
        string szRoomName = m_pJoinRoomNameInput.text;

        if (string.IsNullOrEmpty(szRoomName))
        {
            m_pStatusText.text = "입장할 방 이름을 입력하세요!";

            return;
        }

        PhotonNetwork.JoinRoom(szRoomName);
        m_pStatusText.text = $"'{szRoomName}' ENTERING ROOM...";
    }

    public override void OnJoinedRoom()
    {
        m_pStatusText.text = "ENTERING ROOM SUCCESS..";

        PhotonNetwork.AutomaticallySyncScene = false;

        SceneTransitionManager.Instance.ChangeSceneWithMask("SelectScene");
    }

    private IEnumerator DeferredSyncEnable()
    {
        yield return new WaitForSeconds(2.0f); // 0.5초 더 늘림
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("<color=green>[Network]</color> 씬 동기화 준비 완료!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        m_pStatusText.text = $"CREATE ROOM FAIL: {message}";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        m_pStatusText.text = $"ENTERING ROOM FAIL: {message}"; 
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        m_pStatusText.text = $"MISCONNECT WITH SERVER..: {cause}";
    }
}