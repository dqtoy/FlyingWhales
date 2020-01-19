using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileFeatureGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		DetermineSettlements(4);
		yield return null;
		GenerateFeaturesForNonSettlementTiles();

	}
	private void DetermineSettlements(int count) {
		string log = "Determining settlement tiles";
		List<Region> settlementChoices = new List<Region>(GridMap.Instance.allRegions);
		for (int i = 0; i < count; i++) {
			Region chosenRegion = Utilities.GetRandomElement(settlementChoices);
			HexTile randomTile = Utilities.GetRandomElement(chosenRegion.tiles);
			randomTile.SetElevation(ELEVATION.PLAIN);
			randomTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, randomTile);
			LandmarkManager.Instance.CreateNewLandmarkOnTile(randomTile, LANDMARK_TYPE.VILLAGE, false);
			log += $"\nChose {randomTile.ToString()} to be a settlement";
			if (Random.Range(0, 2) == 1) {
				//2 tiles are settlements
				HexTile adjacentTile = Utilities.GetRandomElement(randomTile.AllNeighbours
					.Where(x => x.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature) == false
					            && x.region == randomTile.region).ToList());
				adjacentTile.SetElevation(ELEVATION.PLAIN);
				adjacentTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, adjacentTile);
				LandmarkManager.Instance.CreateNewLandmarkOnTile(adjacentTile, LANDMARK_TYPE.VILLAGE, false);
				log += $"\nChose {adjacentTile.ToString()} next to {randomTile.ToString()} to be a settlement";
			}
			settlementChoices.Remove(chosenRegion);
		}
	}
	private void GenerateFeaturesForNonSettlementTiles() {
		for (int x = 0; x < GridMap.Instance.width; x++) {
			for (int y = 0; y < GridMap.Instance.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				if (tile.featureComponent.features.Count == 0) {
					//only add features to tiles without features yet
					if (tile.elevationType == ELEVATION.TREES) {
						tile.featureComponent.AddFeature(TileFeatureDB.Wood_Source_Feature, tile);
					} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
						tile.featureComponent.AddFeature(TileFeatureDB.Metal_Source_Feature, tile);	
					}
				}
			}	
		}

		List<HexTile> flatTilesWithNoFeatures = GridMap.Instance.normalHexTiles
			.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0)
			.ToList();

		int stoneSourceCount = 5;
		int fertileCount = 8;
		int gameCount = 6;

		//stone source
		for (int i = 0; i < stoneSourceCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = Utilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Stone_Source_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}		
		
		//fertile
		for (int i = 0; i < fertileCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = Utilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Fertile_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}
		
		//game
		for (int i = 0; i < gameCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = Utilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Game_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}
	}
}
