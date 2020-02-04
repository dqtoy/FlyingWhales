using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		string[] monsterChoices = new[] {"Golem", "Wolves", "Seducer"};
		List<BaseLandmark> monsterLairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		for (int i = 0; i < monsterLairs.Count; i++) {
			BaseLandmark monsterLair = monsterLairs[i];
			string randomSet = CollectionUtilities.GetRandomElement(monsterChoices);
			Settlement settlementOnTile = monsterLair.tileLocation.settlementOnTile;
			LocationStructure monsterLairStructure =
				settlementOnTile.GetRandomStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR);
			Assert.IsTrue(monsterLairStructure.unoccupiedTiles.Count > 0, 
				$"Monster Lair at {monsterLair.tileLocation.region.name} does not have any unoccupied tiles, but is trying to spawn monsters!");
			if (randomSet == "Golem") {
				int randomAmount = Random.Range(1, 3);
				for (int j = 0; j < randomAmount; j++) {
					Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Golem, FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(summon, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					summon.AddTerritory(monsterLair.tileLocation);
				}
			} else if (randomSet == "Wolves") {
				int randomAmount = Random.Range(3, 6);
				for (int j = 0; j < randomAmount; j++) {
					Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Wolf, FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(summon, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					summon.AddTerritory(monsterLair.tileLocation);
				}
			} else if (randomSet == "Seducer") {
				int random = Random.Range(0, 3);
				if (random == 0) {
					//incubus, succubus
					Summon incubus = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Incubus,
						FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(incubus, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					incubus.AddTerritory(monsterLair.tileLocation);
					
					Summon succubus = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Succubus,
						FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(succubus, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					succubus.AddTerritory(monsterLair.tileLocation);
				} else if (random == 1) {
					//incubus
					Summon incubus = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Incubus,
						FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(incubus, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					incubus.AddTerritory(monsterLair.tileLocation);
				} else if (random == 2) {
					//succubus
					Summon succubus = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Succubus,
						FactionManager.Instance.neutralFaction, settlementOnTile);
					CharacterManager.Instance.PlaceSummon(succubus, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
					succubus.AddTerritory(monsterLair.tileLocation);
				}
			}
		}
		yield return null;
	}
}
