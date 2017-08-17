using UnityEngine;
using System.Collections;

[System.Serializable]
public struct LairSpawn {
	public LAIR lairType;
	public BEHAVIOR behavior;
	public int tileRadiusDetection;
	public int numberOfLairs;
	public int lairHP;
	public int monsterHP;
	public int spawnRate;
	public int maxActiveMonster;
	public int moveRange;
}
