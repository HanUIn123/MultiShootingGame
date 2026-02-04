using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    // 전역에서 접근 가능한 몬스터 매니저
    public static List<Transform> AllMonsters = new List<Transform>();

    private void Awake()
    {
        // 씬이 바뀔 때 몬스터 리스트 정보 초기화
        AllMonsters.Clear();
    }
}
