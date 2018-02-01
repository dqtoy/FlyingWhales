using UnityEngine;
using System.Collections;

[System.Serializable]
public struct DungeonEncounterChances {
	public LANDMARK_TYPE dungeonType;
	public int encounterPartyChance;
	public int encounterLootChance;
}
