using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Normal, Elite, Boss }

public class EnemySpawner : MonoBehaviour
{

    [SerializeField]
    private List<EnemySO> enemyDatas;
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject hpBarPrefab; // HP 바 프리팹을 인스펙터에 할당
    public float spawnXPositon;
    public float spawnYPosition;
    public float spawnZPosition;
    public List<Enemy> SpawnedEnemies { get; private set; } = new List<Enemy>(); // NEW: 스폰된 적 리스트


    //싱글톤
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
    void Start()
    {
        // 기존 SpawnAllEnemies 메서드 호출 (필요 시 주석 해제)
        // SpawnAllEnemies();

        // NEW: 랜덤하게 1~3명의 적 스폰
       // SpawnRandomEnemies();
    }




    // NEW: 1~3명의 Normal 또는 Elite 적을 랜덤하게 스폰하는 메서드
    public void SpawnRandomEnemies()
    {
        // 1부터 3까지 랜덤한 수 선택
        int numberOfEnemies = Random.Range(1,4); // Upper bound is inclusive for int

        for (int i = 0; i < numberOfEnemies; i++)
        {
            // EnemyType.Normal (0) 또는 EnemyType.Elite (1)을 랜덤하게 선택
            EnemyType enemyType = (EnemyType)Random.Range(0, 2); // 0 inclusive, 2 exclusive

            // EnemySO 데이터가 존재하는지 확인
            if ((int)enemyType < enemyDatas.Count)
            {
                var enemy = SpawnEnemy(enemyType);
                enemy.PrintEnemyData(); // 적 정보 출력
                SpawnedEnemies.Add(enemy); // 스폰된 적 리스트에 추가
            }
            else
            {
                Debug.LogError($"EnemyType {enemyType} does not have corresponding EnemySO data.");
            }
        }
    }

    // 모든 적에게 독을 부여하는 함수
    public void ApplyPoisonToAllEnemies(int poisonValue)
    {
        foreach (var enemy in SpawnedEnemies)
        {
            
            // 적에게 독을 부여
            if (enemy != null)
                enemy.ApplyPoison(poisonValue);
        }
    }
    // 모든 적의 독을 2배로 증가시키고, 즉시 데미지를 주는 함수
    public void BoostPoisonOnAllEnemiesAndDamage(int damage)
    {
        foreach (Enemy enemy in SpawnedEnemies)
        {
            // 예시: 독 수치를 2배로 증가
            enemy.BoostPoison(2);
            // 그리고 즉시 데미지를 줍니다.
            enemy.Hit(damage, null);
        }
    }


    public Enemy SpawnEnemy(EnemyType type)
    {
        // 적 인스턴스
        var newEnemy = Instantiate(enemyPrefab).GetComponent<Enemy>();

        // EnemySO 데이터 초기화
        newEnemy.Initialize(enemyDatas[(int)type]);
        newEnemy.transform.position = new Vector3(spawnXPositon, spawnYPosition, spawnZPosition);
        spawnXPositon -= 10f; // 다음 적을 위해 y 위치 조정

        // 2. HP 바 생성 및 설정
        if (hpBarPrefab != null)
        {
            // 씬 내 Canvas 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // HP 바 인스턴스화
                GameObject hpBarInstance = Instantiate(hpBarPrefab, canvas.transform);

                // HPBarController 컴포넌트 참조
                HpBarController hpBarController = hpBarInstance.GetComponent<HpBarController>();
                if (hpBarController != null)
                {
                    // HP 바의 타겟 설정 (생성된 적)
                    hpBarController.SetTarget(newEnemy.transform);

                    // HP 바의 최대 HP 및 현재 HP 설정
                    hpBarController.SetMaxHP(newEnemy.EnemyData.HP);
                    hpBarController.SetCurrentHP(newEnemy.EnemyData.HP);
                   
                    // HP 바의 오프셋 설정 (적의 y축 아래로 위치)
                    hpBarController.SetOffset(new Vector3(0, 150f, 0)); // 필요에 따라 조정

                    newEnemy.SetHPBarController(hpBarController);

                }
                else
                {
                    Debug.LogError("HPBarController가 HP 바 프리팹에 존재하지 않습니다.");
                }
            }
            else
            {
                Debug.LogError("씬에 Canvas가 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("HPBarPrefab이 EnemySpawner에 할당되지 않았습니다.");
        }

        return newEnemy;
    }
}
