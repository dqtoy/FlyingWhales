using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Corpse : TileObject, IPointOfInterest {
    public string name { get { return ToString(); } }
    public Character character { get; private set; }
    public LocationStructure location { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }

    public POINT_OF_INTEREST_TYPE poiType { get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; } }
    public POI_STATE state {
        get { return _state; }
    }
    private POI_STATE _state;

    private LocationGridTile _gridTileLocation;
    public LocationGridTile gridTileLocation {
        get { return _gridTileLocation; }
    }

    public Corpse(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
        Initialize(this, TILE_OBJECT_TYPE.CORPSE);
    }

    public void SetGridTileLocation(LocationGridTile tile) {
        _gridTileLocation = tile;
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

    public override string ToString() {
        return "Corpse of " + character.name;
    }

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
}