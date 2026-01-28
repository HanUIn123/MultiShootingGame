using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine.UI; // Image 사용을 위해 추가
using Photon.Realtime;
using UnityEngine;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    [Header("PlayerPrefab Names")]
    [SerializeField]
    private string[]                                        strPlayerPrefabNames = { "TinyShip1", "TinyShip2", "TinyShip3" };

    [Header("PlayerHealth UI & UltimateUIManager")]
    [SerializeField] private PlayerHealthUI                 m_PlayerHealthUI;
    [SerializeField] private UltimateUIManager              m_UltimateUIManager;

    [Header("Player Profile UI")]
    [SerializeField] private Image                          m_pProfileImages; 
    [SerializeField] private Sprite[]                       m_pShipSprites; 

    private void Start()
    {
        // 씬 로딩 이후 안정화 되기 까지 코루틴으로 대기하자.
        StartCoroutine(CoSpawnPlayer());

        RefreshProfileUI(); // 시작할 때 한 번 갱신
    }


    // 누군가 들어오거나 정보가 바뀌면 자동으로 UI 갱신 (별도 스크립트 역할 통합)
    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        RefreshProfileUI(); 
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("ShipIdx")) 
            RefreshProfileUI();
    }

    private IEnumerator CoSpawnPlayer()
    {
        while (!PhotonNetwork.InRoom) 
            yield return null;

        yield return new WaitForSeconds(0.5f);

        // 플레이어들이 DontDestroyOnLoad 로 살아있는지 확인하고..
        // 살아있다면 위치 잡아주기.
        PlayerController[] existingPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        bool bAlreadySpawned = false;

        foreach (var players in existingPlayers)
        {
            if (players.photonView.IsMine) 
            { 
                bAlreadySpawned = true; 
                break; 
            }
        }

        if (!bAlreadySpawned)
        {
            int iSelectedPlayerIndex = PlayerPrefs.GetInt("SelectedShipIndex", 0);
            string strSpawnNames = strPlayerPrefabNames[iSelectedPlayerIndex];


            // 내 로컬 정보를 네트워크(CustomProperties)에 한 번 더 등록
            // 이렇게 해야 상대방 화면에서도 내 프로필이 바뀜.
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props.Add("ShipIdx", iSelectedPlayerIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);



            float fXoffset = PhotonNetwork.IsMasterClient ? -2f : 2f;
            Vector3 v3SpawnPosition = new Vector3(fXoffset, -3.5f, 0f);

            GameObject PlayerObj = PhotonNetwork.Instantiate(strSpawnNames, v3SpawnPosition, Quaternion.identity);
            SetupPlayerComponents(PlayerObj);


            RefreshProfileUI(); // 생성 후 다시 한 번 갱신
        }
    }

    public void RefreshProfileUI()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("ShipIdx", out object myIdx))
        {
            int shipIdx = (int)myIdx;

            if (m_pProfileImages != null)
            {
                m_pProfileImages.sprite = m_pShipSprites[shipIdx];
                m_pProfileImages.gameObject.SetActive(true);
            }
        }
    }

    private void SetupPlayerComponents(GameObject PlayerObj)
    {
        var pView = PlayerObj.GetComponent<PhotonView>();

        if (pView == null) 
            return;

        // PlayerHealth 가져오기.
        var playerHealth = PlayerObj.GetComponent<PlayerHealth>();

        // PlayerController 가져오기.
        var playerController = PlayerObj.GetComponent<PlayerController>();

        // 내 플레이어일 때만 화면의 메인 UI 연동.
        if (pView.IsMine)
        {
            if (playerHealth != null)
            {
                playerHealth.AssignUI(m_PlayerHealthUI);
            }

            if (playerController != null)
            {
                playerController.ultimateUI = m_UltimateUIManager;

                var firePoint = PlayerObj.transform.Find("FirePoint");

                if (firePoint) 
                    playerController.InitLaserSpawn(firePoint);
            }
        }
        else
        {
            
        }
    }
}
