using UnityEngine;
using Photon.Pun; // 상단에 추가
using ExitGames.Client.Photon; // 해시테이블 사용을 위해 추가
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 

public class CharacterSelector : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterData
    {
        public string                                       m_szCharName;
        public RuntimeAnimatorController                    m_pAnimatorController; 
        public string                                       m_szDesc;
    }

    [Header("UI References")]
    [SerializeField] private Animator                       m_pEnlargeAnimator = null;    // EnlargeProfileImage의 애니메이터
    [SerializeField] private RectTransform                  m_pSelectBorder = null;       // SelectCheckBorder

    // 새로 추가된 텍스트 참조
    [SerializeField] private TextMeshProUGUI                m_pTextCharName = null;   // 사진 3의 이름 (PILOT GREEN)
    [SerializeField] private TextMeshProUGUI                m_pTextAttackType = null; // 사진 4의 공격 방식 (AUTO ATTACK)

    [Header("Character Data")]
    [SerializeField] private CharacterData[]                m_pShipDatas; 

    private int                                             m_nSelectedShipIndex = 0;



    // 버튼에서 호출할 함수 (Index: 0, 1, 2)
    public void OnClickCharacter(int nIndex)
    {
        m_nSelectedShipIndex = nIndex;

        if (nIndex < 0 || nIndex >= m_pShipDatas.Length)
            return;

        if (m_pEnlargeAnimator != null)
        {
            RuntimeAnimatorController pNewController = m_pShipDatas[nIndex].m_pAnimatorController;

            m_pEnlargeAnimator.runtimeAnimatorController = null;
            m_pEnlargeAnimator.runtimeAnimatorController = pNewController;

            m_pEnlargeAnimator.enabled = true;
            m_pEnlargeAnimator.Play(0, -1, 0f);
            m_pEnlargeAnimator.Update(0);
        }

        // 2. 텍스트 변경 로직 (추가)
        if (m_pTextCharName != null)
        {
            m_pTextCharName.text = m_pShipDatas[nIndex].m_szCharName;
        }

        if (m_pTextAttackType != null)
        {
            // BLUE처럼 텍스트가 없는 경우 빈 칸 처리
            m_pTextAttackType.text = m_pShipDatas[nIndex].m_szDesc;
        }


        /*
            일단 선택한 UI를 가져오는 실질적인 코드는
            EventSystem.current.currentSelectedGameObject
            using UnityEngine.EventSystems;
        */

        // 현재 선택한 오브젝트 
        GameObject SelectButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (SelectButtonObj != null && m_pSelectBorder != null)
        {
            m_pSelectBorder.position = SelectButtonObj.transform.position;
        }
    }

    public void OnClickGameStart()
    {
        // 선택 캐릭터 인덱스를 SelectedShipIndex 에 저장한다.
        PlayerPrefs.SetInt("SelectedShipIndex", m_nSelectedShipIndex);

        // 포톤 네트워크에 내 캐릭터 정보 등록 (다른 플레이어들이 볼 수 있도록)
        Hashtable props = new Hashtable();
        props.Add("ShipIdx", m_nSelectedShipIndex); // "ShipIdx"라는 키로 저장
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 게임 시작 버튼을 누르면 동기화 시작한다.
        Photon.Pun.PhotonNetwork.AutomaticallySyncScene = true;

        if (SceneTransitionManager.Instance != null)
        {
            // 조리개 연출하면서 + 게임신으로 이동 
            SceneTransitionManager.Instance.ChangeSceneWithMask("GameScene");
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}