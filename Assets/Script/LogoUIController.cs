using System.Collections;
using UnityEngine;

public class LogoUIController : MonoBehaviour
{
    public GameObject logoPanel;          // �ΰ� ���
    public GameObject statusText;         // ���� �ؽ�Ʈ

    public GameObject createRoomButton;   // "�� �����ϱ�" ū ��ư
    public GameObject joinRoomButton;     // "�� �����ϱ�" ū ��ư

    public float delayTime = 1f;

    private void Start()
    {
        StartCoroutine(ShowInitialMenu());
    }

    IEnumerator ShowInitialMenu()
    {
        // �ʱ� UI ���� ����
        statusText.SetActive(false);
        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);

        yield return new WaitForSeconds(delayTime);

        // �ΰ�� ����
        // ��ư�鸸 ����
        statusText.SetActive(true);
        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);
    }
}
