using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (필요시)

// EnemyType enum은 이 파일 또는 다른 공용 파일에 정의되어 있다고 가정합니다.
// public enum EnemyType { Normal, Elite, Boss }

public class EnemySpawner : MonoBehaviour
{
    [Header("적 SO 리스트")]
    [Tooltip("스폰될 수 있는 일반 적 SO 목록")]
    [SerializeField] private List<EnemySO> normalEnemyList;
    [Tooltip("스폰될 수 있는 엘리트 적 SO 목록")]
    [SerializeField] private List<EnemySO> eliteEnemyList;
    [Tooltip("스폰될 수 있는 보스 적 SO 목록")]
    [SerializeField] private List<EnemySO> bossEnemyList;


    [Header("랜덤 스폰 설정")]
    [SerializeField] private int minEnemiesToSpawn = 1;
    [SerializeField] private int maxEnemiesToSpawn = 3; // 'n'에 해당하는 최대 스폰 수

    [Header("공용 설정")]
    [SerializeField] private GameObject hpBarPrefab; // HP 바 프리팹

    [Header("스폰 위치 설정")]
    public float initialSpawnXPosition = 0f; // 초기 스폰 X 위치
    public float spawnYPosition = 0f;
    public float spawnZPosition = 0f;
    public float enemySpacingX = -10f; // 적들 사이의 X 간격

    public List<Enemy> SpawnedEnemies { get; private set; } = new List<Enemy>();

    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 요청 1: 1~n 마리의 적을 랜덤하게 스폰
    public void SpawnRandomEnemies() 
    {
        SpawnedEnemies.Clear();
        float currentSpawnX = initialSpawnXPosition;
        int numberOfEnemies = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn + 1);

        // 스폰 가능한 적 카테고리 목록을 만듭니다.
        List<System.Action> spawnerActions = new List<System.Action>();

        if (normalEnemyList != null && normalEnemyList.Count > 0)
        {
            spawnerActions.Add(() => {
                EnemySO selectedSO = normalEnemyList[Random.Range(0, normalEnemyList.Count)];
                SpawnAndRegisterEnemy(selectedSO, ref currentSpawnX);
            });
        }
        if (eliteEnemyList != null && eliteEnemyList.Count > 0)
        {
            spawnerActions.Add(() => {
                EnemySO selectedSO = eliteEnemyList[Random.Range(0, eliteEnemyList.Count)];
                SpawnAndRegisterEnemy(selectedSO, ref currentSpawnX);
            });
        }
        if (bossEnemyList != null && bossEnemyList.Count > 0)
        {
            spawnerActions.Add(() => {
                EnemySO selectedSO = bossEnemyList[Random.Range(0, bossEnemyList.Count)];
                Debug.LogWarning("[Game Design] Boss enemy spawned via SpawnRandomEnemiesIncludingBosses function.");
                SpawnAndRegisterEnemy(selectedSO, ref currentSpawnX);
            });
        }

        if (spawnerActions.Count == 0)
        {
            Debug.LogWarning("스폰할 수 있는 적 SO 리스트가 모두 비어있습니다 (Normal, Elite, Boss).");
            return;
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            // 가능한 스폰 액션 중 하나를 랜덤하게 선택하여 실행
            int randomActionIndex = Random.Range(0, spawnerActions.Count);
            spawnerActions[randomActionIndex].Invoke();
        }
    }

    // 내부적으로 사용될 적 스폰 및 등록 헬퍼 함수
    private void SpawnAndRegisterEnemy(EnemySO soToSpawn, ref float currentX)
    {
        if (soToSpawn == null) return;

        Enemy newEnemy = SpawnSpecificEnemy(soToSpawn, new Vector3(currentX, spawnYPosition, spawnZPosition));
        if (newEnemy != null)
        {
            SpawnedEnemies.Add(newEnemy);
            newEnemy.PrintEnemyData();
            currentX += enemySpacingX; // 다음 적 스폰 위치 조정
        }
    }


    // 요청 2: EnemyPrefab을 SO에서 가져오도록 수정된 공용 스폰 메소드
    /// <summary>
    /// 특정 EnemySO 데이터를 기반으로 적 하나를 지정된 위치에 스폰합니다.
    /// </summary>
    /// <param name="soToSpawn">스폰할 적의 EnemySO</param>
    /// <param name="position">스폰될 위치</param>
    /// <returns>스폰된 Enemy 객체</returns>
    private Enemy SpawnSpecificEnemy(EnemySO soToSpawn, Vector3 position)
    {
        if (soToSpawn == null)
        {
            Debug.LogError("SpawnSpecificEnemy: soToSpawn 파라미터가 null입니다.");
            return null;
        }
        if (soToSpawn.EnemyPrefab == null)
        {
            Debug.LogError($"SpawnSpecificEnemy: {soToSpawn.EnemyName}의 EnemyPrefab이 할당되지 않았습니다.");
            return null;
        }

        // SO에 지정된 프리팹으로 적 인스턴스 생성
        GameObject enemyObject = Instantiate(soToSpawn.EnemyPrefab, position, Quaternion.identity);
        Enemy newEnemy = enemyObject.GetComponent<Enemy>();

        if (newEnemy == null)
        {
            Debug.LogError($"{soToSpawn.EnemyName} 프리팹에 Enemy 컴포넌트가 없습니다.");
            Destroy(enemyObject); // 불완전한 오브젝트 제거
            return null;
        }

        newEnemy.Initialize(soToSpawn); // EnemySO 데이터로 초기화

        // HP 바 생성 및 설정
        if (hpBarPrefab != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject hpBarInstance = Instantiate(hpBarPrefab, canvas.transform);
                HpBarController hpBarController = hpBarInstance.GetComponent<HpBarController>();
                if (hpBarController != null)
                {
                    hpBarController.SetTarget(newEnemy.transform);
                    hpBarController.SetMaxHP(newEnemy.EnemyData.HP); // EnemyData는 Enemy.cs에서 EnemySO를 가리키는 속성이라고 가정
                    hpBarController.SetCurrentHP(newEnemy.EnemyData.HP);
                    hpBarController.SetOffset(new Vector3(0, -380f, 0)); // 오프셋 조정
                    newEnemy.SetHPBarController(hpBarController); // Enemy 스크립트에 HP 바 컨트롤러 설정 메소드 필요
                }
                else Debug.LogError("HPBarController가 HP 바 프리팹에 존재하지 않습니다.");
            }
            else Debug.LogError("씬에 Canvas가 존재하지 않습니다.");
        }
        else Debug.LogError("HPBarPrefab이 EnemySpawner에 할당되지 않았습니다.");

        return newEnemy;
    }


    // --- 기존 독 관련 함수들은 그대로 유지 ---
    public void ApplyPoisonToAllEnemies(int poisonValue)
    {
        foreach (var enemy in SpawnedEnemies)
        {
            if (enemy != null)
                enemy.ApplyPoison(poisonValue);
        }
    }

    public void BoostPoisonOnAllEnemiesAndDamage(int damage)
    {
        foreach (Enemy enemy in SpawnedEnemies)
        {
            if (enemy != null)
            {
                enemy.BoostPoison(2);
                enemy.Hit(damage, null); // Player 참조가 필요 없는 경우 null 전달
            }
        }
    }
}