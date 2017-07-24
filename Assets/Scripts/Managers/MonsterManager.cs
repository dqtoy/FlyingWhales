using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MonsterManager : MonoBehaviour {
	public static MonsterManager Instance;

	public int tileRadiusDetection;
	public int numberOfLairs;
	public int minimumLairDistance;

	public List<Lair> allLairs = new List<Lair>();

	void Awake(){
		Instance = this;
	}

	public void GenerateLairs(){
		List<HexTile> lairEligibleTiles = new List<HexTile>(CityGenerator.Instance.lairHabitableTiles);

		for (int i = 0; i < numberOfLairs; i++) {
			if(lairEligibleTiles != null && lairEligibleTiles.Count > 0){
				HexTile chosenLairTile = lairEligibleTiles[UnityEngine.Random.Range(0, lairEligibleTiles.Count)];
				LAIR type = GetRandomLairType();
				Lair newLair = CreateLair(type, chosenLairTile);
				AddToLairList(newLair);

				List<HexTile> nearHabitableTiles = chosenLairTile.GetTilesInRange(this.minimumLairDistance).Where(x => lairEligibleTiles.Contains(x) == true).ToList();
				for (int j = 0; j < nearHabitableTiles.Count; j++) {
					lairEligibleTiles.Remove(nearHabitableTiles[j]);
				}

			}else{
				break;
			}
		}
	}

	internal void AddToLairList(Lair lair){
		this.allLairs.Add(lair);
	}
	internal LAIR GetRandomLairType(){
		return (LAIR)(UnityEngine.Random.Range (0, System.Enum.GetNames (typeof(LAIR)).Length));
	}
	internal Lair CreateLair(LAIR type, HexTile hexTile){
		switch(type){
		case LAIR.LYCAN:
			LycanLair lycanLair = new LycanLair(type, hexTile);
			return lycanLair;
		}
	}
}
