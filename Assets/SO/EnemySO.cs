using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
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



}
