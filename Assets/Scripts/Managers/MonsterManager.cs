using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MonsterManager : MonoBehaviour {
	public static MonsterManager Instance;

	public int tileRadiusDetection;
	public int minimumLairDistance;
	public bool activateLairImmediately;
	public LairSpawn[] lairSpawn;

	public List<Lair> allLairs = new List<Lair>();

	void Awake(){
		Instance = this;
	}

	public void GenerateLairs(){
		List<HexTile> lairEligibleTiles = new List<HexTile>(CityGenerator.Instance.lairHabitableTiles);
		bool noMoreTiles = false;
		for (int i = 0; i < lairSpawn.Length; i++) {
			for (int j = 0; j < lairSpawn[i].numberOfLairs; j++) {
				if(lairEligibleTiles != null && lairEligibleTiles.Count > 0){
					HexTile chosenLairTile = lairEligibleTiles[UnityEngine.Random.Range(0, lairEligibleTiles.Count)];
					LAIR type = lairSpawn[i].lairType;
					Lair newLair = CreateLair(type, chosenLairTile);
					AddToLairList(newLair);

					List<HexTile> nearHabitableTiles = chosenLairTile.GetTilesInRange(this.minimumLairDistance).Where(x => lairEligibleTiles.Contains(x) == true).ToList();
					for (int k = 0; k < nearHabitableTiles.Count; k++) {
						lairEligibleTiles.Remove(nearHabitableTiles[k]);
					}

				}else{
					noMoreTiles = true;
					break;
				}
			}
			if(noMoreTiles){
				break;
			}
		}
	}

	internal void AddToLairList(Lair lair){
		this.allLairs.Add(lair);
	}
	internal void RemoveFromLairList(Lair lair){
		this.allLairs.Remove(lair);
	}
	internal LAIR GetRandomLairType(){
		return (LAIR)(UnityEngine.Random.Range (0, System.Enum.GetNames (typeof(LAIR)).Length));
	}
	internal Lair CreateLair(LAIR type, HexTile hexTile){
		switch(type){
		case LAIR.LYCAN:
			LycanLair lycanLair = new LycanLair(type, hexTile);
			return lycanLair;
		case LAIR.STORM_WITCH:
			StormWitchLair stormWitchLair = new StormWitchLair(type, hexTile);
			return stormWitchLair;
		}
		return null;
	}
	internal void SummonNewMonster(MONSTER type, HexTile originHextile, HexTile targetHextile){
		Monster newMonster = null;
		switch(type){
		case MONSTER.LYCAN:
			Lycan newLycan = new Lycan(type, originHextile);
			newMonster = newLycan;
			break;
		}

		if(newMonster != null){
			newMonster.targetLocation = targetHextile;
			List<HexTile> path = PathGenerator.Instance.GetPath(originHextile, targetHextile, PATHFINDING_MODE.AVATAR);
			if(path != null){
				newMonster.path = path;
			}
		}
	}
}
