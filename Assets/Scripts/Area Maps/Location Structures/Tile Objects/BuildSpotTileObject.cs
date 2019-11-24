using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSpotTileObject : TileObject {

    public BuildingSpot spot { get; private set; }

    public BuildSpotTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.BUILD_STRUCTURE, };
        Initialize(TILE_OBJECT_TYPE.BUILD_SPOT_TILE_OBJECT);
    }
    public BuildSpotTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.BUILD_STRUCTURE, };
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return "Build Spot " + id.ToString();
    }
    #endregion

    public void SetBuildingSpot(BuildingSpot spot) {
        this.spot = spot;
    }

    public void PlaceBlueprintOnBuildingSpot(STRUCTURE_TYPE structureType) {
        List<GameObject> choices = InteriorMapManager.Instance.GetStructurePrefabsForStructure(structureType);
        GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);

        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, gridTileLocation.parentAreaMap.structureParent);
        structurePrefab.transform.localPosition = spot.centeredLocation;
        LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
        structureObject.RefreshAllTilemaps();
        List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentAreaMap);
        for (int j = 0; j < occupiedTiles.Count; j++) {
            LocationGridTile tile = occupiedTiles[j];
            tile.SetHasBlueprint(true);
        }
        spot.SetIsOpen(false);
        spot.SetIsOccupied(true);
        spot.SetAllAdjacentSpotsAsOpen(gridTileLocation.parentAreaMap);
        spot.CheckIfAdjacentSpotsCanStillBeOccupied(gridTileLocation.parentAreaMap);
        structureObject.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Blueprint, gridTileLocation.parentAreaMap);
        spot.SetBlueprint(structureObject, structureType);
        structureObject.SetTilesInStructure(occupiedTiles.ToArray());
        //structure.SetStructureObject(structureObject);
        //structureObject.OnStructureObjectPlaced();
    }

    public void BuildBlueprint() {
        spot.blueprint.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built, gridTileLocation.parentAreaMap);
        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.parentAreaMap.area, spot.blueprintType, true);

        spot.blueprint.ClearOutUnimportantObjectsBeforePlacement();

        for (int j = 0; j < spot.blueprint.tiles.Length; j++) {
            LocationGridTile tile = spot.blueprint.tiles[j];
            tile.SetStructure(structure);
        }
        structure.SetStructureObject(spot.blueprint);
        spot.blueprint.OnStructureObjectPlaced(gridTileLocation.parentAreaMap);
        spot.ClearBlueprints();
        
    }

}
