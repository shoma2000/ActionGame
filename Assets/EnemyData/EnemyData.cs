using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Enemies
{
    [SerializeField] string Name;
    [SerializeField] int ID;
    [SerializeField] int HP;
    [SerializeField] int Attack;
    [SerializeField] float Speed;
    [SerializeField] float DamageInterval;
    [SerializeField] float AttackInterval;

    public int iD { get => ID; }
    public int hP { get => HP; }
    public int attack { get => Attack; }
    public float speed { get => Speed; }
    public float damageInterval { get => DamageInterval; }
    public float attackInterval { get => AttackInterval; }
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "©ìƒf[ƒ^/EnemyData")]
public class EnemyData : ScriptableObject
{
    public List<Enemies> DataList;
}
