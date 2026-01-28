using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    // 전역에서 접근 가능한 몬스터 명단 (공장 직명부)
    // static이라서 어떤 스크립트에서도 MonsterManager.AllMonsters로 바로 접근 가능합니다.
    public static List<Transform> AllMonsters = new List<Transform>();

    private void Awake()
    {
        // 씬이 바뀔 때 리스트가 꼬이지 않도록 초기화
        AllMonsters.Clear();
    }
}