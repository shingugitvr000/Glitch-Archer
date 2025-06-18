using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Stage_", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 정보")]
    public string stageName = "스테이지 1";
    public string stageDescription = "첫 번째 스테이지입니다.";
    public Sprite stageIcon;

    [Header("목표 설정")]
    public StageObjective objective = StageObjective.KillAllEnemies;
    public int targetCount = 5;                     // 처치할 적 수 / 수집할 아이템 수
    public float timeLimit = 300f;                  // 제한 시간 (초)

    [Header("적 설정")]
    public List<EnemySpawnInfo> enemySpawns = new List<EnemySpawnInfo>();
    public int maxEnemiesAtOnce = 5;                // 동시에 존재할 수 있는 최대 적 수

    [Header("보상")]
    public int baseExpReward = 100;
    public int goldReward = 50;
    public List<string> possibleItemRewards = new List<string>(); // 아이템 ID들

    [Header("난이도")]
    [Range(1, 10)]
    public int difficultyLevel = 1;
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;

    [Header("특수 조건")]
    public bool allowPlayerRespawn = true;
    public bool hasTimeBonus = true;                // 빠른 클리어 시 보너스
    public float timeBonusThreshold = 120f;         // 보너스를 받을 수 있는 시간
}

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int spawnCount = 1;
    public float spawnDelay = 0f;                   // 스테이지 시작 후 몇 초 뒤 스폰
    public Vector3 spawnPosition;
    public bool useSpawnPoint = true;               // SpawnPoint 사용 여부
}

public enum StageObjective
{
    KillAllEnemies,     // 모든 적 처치
    KillSpecificCount,  // 특정 수만큼 적 처치
    Survival,           // 시간 버티기
    CollectItems,       // 아이템 수집
    ReachGoal,          // 목적지 도달
    DefendBase          // 기지 방어
}