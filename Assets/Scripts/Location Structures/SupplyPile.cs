using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : TileObject, IPointOfInterest {
    public string name { get { return ToString(); } }
    public LocationStructure location { get; private set; }
    public int suppliesInPile { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }

    private LocationGridTile tile;
    private POI_STATE _state;

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    #endregion

    public SupplyPile(LocationStructure location) {
        this.location = location;
        Initialize(this, TILE_OBJECT_TYPE.SUPPLY_PILE);
    }

    public int GetSuppliesObtained() {
        if (location.structureType == STRUCTURE_TYPE.DUNGEON) {
            return Random.Range(location.location.dungeonSupplyRangeMin, location.location.dungeonSupplyRangeMax + 1);
        } else {
            return Random.Range(1, suppliesInPile);
        }
    }

    public int GetAndReduceSuppliesObtained(Area reciever) {
        int suppliesObtained = GetSuppliesObtained();
        TransferSuppliesTo(reciever, suppliesObtained);
        return suppliesObtained;
    }

    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void TransferSuppliesTo(Area reciever, int amount) {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) { //if supplies come from warehouse, reduce amount
            location.location.AdjustSuppliesInBank(-amount);
            reciever.AdjustSuppliesInBank(amount);
        } else if (location.structureType == STRUCTURE_TYPE.DUNGEON) { //if supplies come from dungeon, do not reduce amount in area
            reciever.AdjustSuppliesInBank(amount);
        }
    }

    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Supply Pile";
    }

    #region Area Map
    public void SetGridTileLocation(LocationGridTile tile) {
        if (tile != null) {
            tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        }
        this.tile = tile;
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis(LocationStructure structure, Character otherCharacter) {
        if (gridTileLocation != null && location == structure) {
            List<LocationGridTile> choices = location.unoccupiedTiles.Where(x => x != gridTileLocation).OrderBy(x => Vector2.Distance(gridTileLocation.localLocation, x.localLocation)).ToList();
            if (choices.Count > 0) {
                LocationGridTile nearestTile = choices[0];
                if (otherCharacter.currentStructure == structure && otherCharacter.gridTileLocation != null) {
                    float ogDistance = Vector2.Distance(this.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
                    float newDistance = Vector2.Distance(this.gridTileLocation.localLocation, nearestTile.localLocation);
                    if (newDistance > ogDistance) {
                        return otherCharacter.gridTileLocation; //keep the other character's current tile
                    }
                }
                return nearestTile;
            }
        }
        return null;
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0  && state == POI_STATE.ACTIVE) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }

        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    #endregion
}
