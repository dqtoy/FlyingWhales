using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterComponent : MonoBehaviour {
    public string monsterName;
    public MONSTER_TYPE type;
    public MONSTER_CATEGORY category;
    public int level;
    public int experienceDrop;
    public int maxHP;
    public int maxSP;
    public float attackPower;
    public float speed;
    public int def;
    public float dodgeChance;
    public float hitChance;
    public float critChance;
    public bool isSleepingOnSpawn;
    public List<TextAsset> skills;
    public List<string> skillNames;
    public List<ElementChance> elementChanceWeaknesses;
    public List<ElementChance> elementChanceResistances;
}
