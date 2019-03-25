using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Table : TileObject, IPointOfInterest {
    public string name { get { return ToString(); } }
    public LocationStructure location { get; private set; }
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

    public Table(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_DWELLING_TABLE, INTERACTION_TYPE.DRINK, INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_TYPE.TABLE_POISON, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(this, TILE_OBJECT_TYPE.TABLE);
    }

    public override string ToString() {
        return "Table " + id.ToString();
    }

    #region Interface
    public void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        this.tile = tile;
    }
    //public LocationGridTile GetNearestUnoccupiedTileFromThis() {
    //    if (gridTileLocation != null && location == structure) {
    //        List<LocationGridTile> choices = location.unoccupiedTiles.Where(x => x != gridTileLocation).OrderBy(x => Vector2.Distance(gridTileLocation.localLocation, x.localLocation)).ToList();
    //        if (choices.Count > 0) {
    //            LocationGridTile nearestTile = choices[0];
    //            if (otherCharacter.currentStructure == structure && otherCharacter.gridTileLocation != null) {
    //                float ogDistance = Vector2.Distance(this.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
    //                float newDistance = Vector2.Distance(this.gridTileLocation.localLocation, nearestTile.localLocation);
    //                if (newDistance > ogDistance) {
    //                    return otherCharacter.gridTileLocation; //keep the other character's current tile
    //                }
    //            }
    //            return nearestTile;
    //        }
    //    }
    //    return null;
    //}
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
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

    #region Tile Objects
    public override void OnTargetObject(GoapAction action) {
        
    }
    public override void OnDoActionToObject(GoapAction action) {
    }
    #endregion
}
