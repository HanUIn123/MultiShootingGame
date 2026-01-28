using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunObjectPool : MonoBehaviour, IPunPrefabPool
{
    private Dictionary<string, Stack<GameObject>> poolDict = new Dictionary<string, Stack<GameObject>>();

    private void Start()
    {
        PhotonNetwork.PrefabPool = this;
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        // 플레이어는 풀링에서 제외한다. 
        if (prefabId.Equals("TinyShip1") || prefabId.Equals("TinyShip2") || prefabId.Equals("TinyShip3"))
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            GameObject playerObj = Object.Instantiate(prefab, position, rotation);

            return playerObj;
        }

        // 풀링 대상은 총알과 몬스터 ( 일단은.. )
        // 위에서 걸러진 prefabId 가 있으면 얘네들을 풀 딕셔너리에 담기.
        if (!poolDict.ContainsKey(prefabId)) 
            poolDict[prefabId] = new Stack<GameObject>();

        GameObject gameObject = null;

        if (poolDict[prefabId].Count > 0)
        {
            // 풀에 있는 프리팹이 0 보다 크면, 꺼내기 시작.
            gameObject = poolDict[prefabId].Pop();
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;

            // 포톤 에러 방지로서, 반드시 꺼진 상태(false)로 return 해야 경고문이 안뜸
            gameObject.SetActive(false);
        }
        else
        {
            // 풀에 없으면 일단 처음으로 최초 생성
            GameObject prefab = Resources.Load<GameObject>(prefabId);

            gameObject = Object.Instantiate(prefab, position, rotation);

            // 새로 만들 때도 포톤이 직접 켜게, 꺼서 준다.
            gameObject.SetActive(false);
        }

        return gameObject;
    }

    public void Destroy(GameObject gameObject)
    {
        string cleanName = gameObject.name.Replace("(Clone)", "").Trim();

        if (cleanName.Equals("TinyShip1") || cleanName.Equals("TinyShip2") || cleanName.Equals("TinyShip3"))
        {
            Object.Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);

        string prefabId = cleanName;

        if (cleanName.StartsWith("TinyShip") && !cleanName.Equals("TinyShip1") && !cleanName.Equals("TinyShip2") && !cleanName.Equals("TinyShip3"))
        {
            prefabId = "Monster/" + cleanName;
        }
        else if (cleanName.Equals("EnemyBullet"))
        {
            prefabId = "Monster/" + cleanName;
        }

        if (!poolDict.ContainsKey(prefabId)) 
            poolDict[prefabId] = new Stack<GameObject>();

        poolDict[prefabId].Push(gameObject);
    }
}