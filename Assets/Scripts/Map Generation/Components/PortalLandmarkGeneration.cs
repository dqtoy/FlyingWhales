using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class PortalLandmarkGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		PlacePortal(data);
		yield return null;
	}

	private void PlacePortal(MapGenerationData data) {
		//valid portal tile is a flat or tree tile adjacent to a settlement hex tile and in the same region
		List<HexTile> settlementTiles = GetAllSettlementTiles();
		List<HexTile> validPortalTiles = new List<HexTile>();
		for (int i = 0; i < settlementTiles.Count; i++) {
			HexTile settlementTile = settlementTiles[i];
			for (int j = 0; j < settlementTile.AllNeighbours.Count; j++) {
				HexTile neighbour = settlementTile.AllNeighbours[j];
				if (validPortalTiles.Contains(neighbour) == false && 
				    (neighbour.elevationType == ELEVATION.PLAIN || neighbour.elevationType == ELEVATION.TREES) &&
				    neighbour.region == settlementTile.region && 
				    neighbour.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature) == false) {
					validPortalTiles.Add(neighbour);
				}
			}
		}

		if (validPortalTiles.Count > 0) {
			HexTile portalTile = CollectionUtilities.GetRandomElement(validPortalTiles);
			BaseLandmark portalLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(portalTile, LANDMARK_TYPE.THE_PORTAL, false);
			data.portal = portalLandmark;	
		} else {
			throw new Exception("No valid portal tiles were found!");
		}
		
	}

	private List<HexTile> GetAllSettlementTiles() {
		List<HexTile> settlementTiles = new List<HexTile>();
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			if (tile.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature)) {
				settlementTiles.Add(tile);
			}
		}
		return settlementTiles;
	}
}
