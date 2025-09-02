using System.Collections;
using UnityEngine;

public class LogoUIController : MonoBehaviour
{
    public GameObject logoPanel;         
    public GameObject statusText;         

    public GameObject createRoomButton;   
    public GameObject joinRoomButton;     

    public float delayTime = 1f;

    private void Start()
    {
        StartCoroutine(ShowInitialMenu());
    }

    IEnumerator ShowInitialMenu()
    {
        statusText.SetActive(false);
        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);

        yield return new WaitForSeconds(delayTime);

        statusText.SetActive(true);
        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);
    }
}
