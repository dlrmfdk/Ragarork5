using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
public enum EnemyType
{
    Normal,
    Elite,
    Boss
    // 필요에 따라 다른 타입 추가 가능
}

[CreateAssetMenu(fileName ="Enemy Data",menuName ="Scriptable Object/Enemy Data",order =int.MaxValue)]

public class EnemySO : ScriptableObject
{
    [SerializeField]
    private string enemyName;
    public string EnemyName => enemyName;

    [SerializeField]
    private GameObject enemyPrefab;
    public GameObject EnemyPrefab => enemyPrefab;

    [SerializeField]
    private int hp;
    public int HP => hp;

    [SerializeField]
    private int damage;
    public int Damage => damage;


    [SerializeField]
    private int defense;
    public int Defense => defense;

    [SerializeField]
    private EnemyType category;
    public EnemyType Category => category;

    [Header("행동 패턴")]
    public List<EnemyActionSO> actionPatterns; //적이 사용할 수 있는 행동들의 목록

    // ▼▼▼ [이 부분을 추가하세요] ▼▼▼
    [Header("연계 패턴 (차지)")]
    [Tooltip("Charge 행동 이후에 '무조건' 사용할 강력한 공격 행동 (EnemyActionSO)")]
    public EnemyActionSO chargeAttackAction; // 차지 후 연계될 공격 SO
    // ▲▲▲ 추가 완료 ▲▲▲

    [Header("특수 패턴")]
    public RuneSO penaltyRune; // 이 적이 부여할 패널티 룬

}
