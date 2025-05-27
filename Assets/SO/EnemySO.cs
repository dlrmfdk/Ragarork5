using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Enemy Data",menuName ="Scriptable Object/Enemy Data",order =int.MaxValue)]

public class EnemySO : ScriptableObject
{
    [SerializeField]
    private string enemyName;
    public string EnemyName => enemyName;

    [SerializeField]
    private int hp;
    public int HP => hp;

    [SerializeField]
    private int damage;
    public int Damage => damage;


    [SerializeField]
    private int defense;
    public int Defense => defense;




}
