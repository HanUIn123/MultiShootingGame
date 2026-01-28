using UnityEngine;
using Photon.Pun;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    private void Awake()
    {
        Instance = this;
    }
  
    public void ManageDropItem(Vector3 v3Position, float fPowerPercent, float fSpeedPercent)
    {
        if (!PhotonNetwork.IsMasterClient) 
            return;

        float fRandRange = Random.Range(0f, 100f);

        // 파워 아이템 
        if (fRandRange <= fPowerPercent)
        {
            PhotonNetwork.Instantiate("Item/PowerItem", v3Position, Quaternion.identity);
        }
        // 스피드 아이템 
        else if (fRandRange <= (fPowerPercent + fSpeedPercent))
        {
            PhotonNetwork.Instantiate("Item/SpeedItem", v3Position, Quaternion.identity);
        }
        else
        {
        }
    }
}