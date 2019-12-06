using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSpotTileObject : TileObject {

    public BuildingSpot spot { get; private set; }

    public BuildSpotTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.BUILD_STRUCTURE, };
        Initialize(TILE_OBJECT_TYPE.BUILD_SPOT_TILE_OBJECT);
        RemoveCommonAdvertisments();
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public BuildSpotTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.BUILD_STRUCTURE, };
        Initialize(data);
        RemoveCommonAdvertisments();
        traitContainer.RemoveTrait(this, "Flammable");
    }

    #region Overrides
    public override string ToString() {
        return $"Build Spot owned by {structureLocation.ToString()}";
    }
    public override bool CanBeDamaged() {
        return false;
    }
    #endregion

    public void SetBuildingSpot(BuildingSpot spot) {
        this.spot = spot;
    }
    public void PlaceBlueprintOnBuildingSpot(STRUCTURE_TYPE structureType) {
        List<GameObject> choices = InteriorMapManager.Instance.GetStructurePrefabsForStructure(structureType);
        GameObject chosenStructurePrefab = null;
        for (int i = 0; i < choices.Count; i++) {
            GameObject currPrefab = choices[i];
            LocationStructureObject so = currPrefab.GetComponent<LocationStructureObject>();
            if (spot.CanPlaceStructureOnSpot(so, gridTileLocation.parentAreaMap)) {
                chosenStructurePrefab = currPrefab;
                break;
            }

        }
        if (chosenStructurePrefab != null) {
            GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, gridTileLocation.parentAreaMap.structureParent);
            LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
            structurePrefab.transform.localPosition = spot.GetPositionToPlaceStructure(structureObject);
            
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentAreaMap);
            for (int j = 0; j < occupiedTiles.Count; j++) {
                LocationGridTile tile = occupiedTiles[j];
                tile.SetHasBlueprint(true);
            }
            spot.SetIsOpen(false);
            spot.SetIsOccupied(true);
            spot.SetAllAdjacentSpotsAsOpen(gridTileLocation.parentAreaMap);
            spot.UpdateAdjacentSpotsOccupancy(gridTileLocation.parentAreaMap);
            structureObject.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Blueprint, gridTileLocation.parentAreaMap);
            spot.SetBlueprint(structureObject, structureType);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());
        } else {
            Debug.LogWarning($"Could not find a prefab for structure {structureType.ToString()} on build spot {spot.ToString()}");
        }
    }
    public LocationStructure BuildBlueprint() {
        spot.blueprint.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built, gridTileLocation.parentAreaMap);
        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.parentAreaMap.area, spot.blueprintType, true);

        spot.blueprint.ClearOutUnimportantObjectsBeforePlacement();

        for (int j = 0; j < spot.blueprint.tiles.Length; j++) {
            LocationGridTile tile = spot.blueprint.tiles[j];
            tile.SetStructure(structure);
        }
        structure.SetStructureObject(spot.blueprint);
        spot.blueprint.PlacePreplacedObjectsAsBlueprints(structure, gridTileLocation.parentAreaMap);
        spot.blueprint.OnStructureObjectPlaced(gridTileLocation.parentAreaMap, structure);
        spot.ClearBlueprints();

        structure.SetOccupiedBuildSpot(this);

        return structure;
        
    }
    public void RemoveOccupyingStructure(LocationStructure structure) {
        spot.SetIsOpen(true);
        spot.SetIsOccupied(false);
        spot.UpdateAdjacentSpotsOccupancy(structure.location.areaMap);
    }

}
