using System.Collections;
using UnityEngine;

public class LogoUIController : MonoBehaviour
{
    public GameObject logoPanel;          // 로고 배경
    public GameObject statusText;         // 상태 텍스트

    public GameObject createRoomButton;   // "방 생성하기" 큰 버튼
    public GameObject joinRoomButton;     // "방 참가하기" 큰 버튼

    public float delayTime = 1f;

    private void Start()
    {
        StartCoroutine(ShowInitialMenu());
    }

    IEnumerator ShowInitialMenu()
    {
        // 초기 UI 전부 끄기
        statusText.SetActive(false);
        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);

        yield return new WaitForSeconds(delayTime);

        // 로고는 유지
        // 버튼들만 등장
        statusText.SetActive(true);
        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);
    }
}
