using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class SettlementGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature)) {
				yield return MapGenerator.Instance.StartCoroutine(CreateSettlement(region));
			}
		}
		yield return null;
	}

	private IEnumerator CreateSettlement(Region region) {
		List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		Settlement settlement = LandmarkManager.Instance.CreateNewSettlement
			(region, LOCATION_TYPE.HUMAN_SETTLEMENT, 1, settlementTiles.ToArray());
		int totalBuildSpots = settlementTiles.Count * 4; //*4 because each hex tile = 4 build spots
		totalBuildSpots -= 2; //to accomodate for city center
		
		
		int randomCitizens = Random.Range(2, totalBuildSpots + 1);

		if (totalBuildSpots > 6) {
			randomCitizens = Random.Range(9, totalBuildSpots + 1);
		} else if (totalBuildSpots > 2) { //this is so that settlements that initially have more than 1 tile are sure to occupy it.
			randomCitizens = Random.Range(5, totalBuildSpots + 1);
		}

		Faction faction = FactionManager.Instance.CreateNewFaction();
		List<Character> createdCharacters = faction.GenerateStartingCitizens(2, 1, randomCitizens, settlement.classManager, settlement);
		int dwellingCount = GetNumberOfDwellingsToHouseCharacters(createdCharacters);
		
		settlement.GenerateStructures(dwellingCount);
		settlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
		
		LandmarkManager.Instance.OwnSettlement(faction, settlement);
		//assign characters to their respective homes. No one should be homeless
        for (int i = 0; i < createdCharacters.Count; i++) {
            Character currCharacter = createdCharacters[i];
            settlement.AssignCharacterToDwellingInArea(currCharacter);
        }
		
		yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructures(settlement, region.innerMap));
		yield return MapGenerator.Instance.StartCoroutine(settlement.PlaceObjects());
		
		CharacterManager.Instance.PlaceInitialCharacters(createdCharacters, settlement);
		settlement.OnAreaSetAsActive();
	
	}
	private IEnumerator PlaceInitialStructures(Settlement settlement, InnerTileMap innerTileMap) {
		//order the structures based on their priorities
		Dictionary<STRUCTURE_TYPE, List<LocationStructure>> ordered = settlement.structures
			.OrderBy(x => x.Key.StructureGenerationPriority())
			.ToDictionary(x => x.Key, x => x.Value);

		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in ordered) {
			if (keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
				for (int i = 0; i < keyValuePair.Value.Count; i++) {
					LocationStructure structure = keyValuePair.Value[i];
					List<GameObject> choices =
						InnerMapManager.Instance.GetStructurePrefabsForStructure(keyValuePair.Key);
					GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
					LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
					BuildingSpot chosenBuildingSpot;
					if (TryGetBuildSpotForStructureInSettlement(lso, settlement, out chosenBuildingSpot)) {
						innerTileMap.PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
					} else {
						throw new System.Exception(
							$"Could not find valid building spot for {structure.ToString()} using prefab {chosenStructurePrefab.name}");
					}
					yield return null;
				}
			}
		}
	}

	private bool TryGetBuildSpotForStructureInSettlement(LocationStructureObject structureObject, Settlement settlement, out BuildingSpot spot) {
		for (int i = 0; i < settlement.tiles.Count; i++) {
			HexTile currTile = settlement.tiles[i];
			for (int j = 0; j < currTile.ownedBuildSpots.Length; j++) {
				BuildingSpot currSpot = currTile.ownedBuildSpots[j];
				if (currSpot.isOccupied == false && currSpot.CanFitStructureOnSpot(structureObject, settlement.innerMap)) {
					spot = currSpot;
					return true;
				}
			}
		}
		spot = null;
		return false;
	}
	
	private int GetNumberOfDwellingsToHouseCharacters(List<Character> characters) {
		//To get number of dwellings needed,
		//loop through all the characters, then check if each one is single
		//if a character is single, assign it 1 dwelling, then remove that character from the list
		//if the character is not single then remove it and its lover from the list of characters, and assign them 1 dwelling.
		//continue the loop until the list of characters becomes empty

		List<Character> listOfCharacters = new List<Character>(characters);
		int neededDwellingCount = 0;

		while (listOfCharacters.Count != 0) {
			Character currCharacter = listOfCharacters[0];
			Character lover = (currCharacter.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.LOVER) as Character) ?? null;
			if (lover != null) {
				listOfCharacters.Remove(lover);
			}
			listOfCharacters.Remove(currCharacter);
			neededDwellingCount++;
		}
		return neededDwellingCount;
	}
}
