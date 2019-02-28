using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Corpse : IPointOfInterest {

    public Character character { get; private set; }
    public LocationStructure location { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }

    public POINT_OF_INTEREST_TYPE poiType { get { return POINT_OF_INTEREST_TYPE.CORPSE; } }

    private LocationGridTile _gridTileLocation;
    public LocationGridTile gridTileLocation {
        get { return _gridTileLocation; }
    }

    public Corpse(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
    }

    public void SetGridTileLocation(LocationGridTile tile) {
        _gridTileLocation = tile;
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

    public override string ToString() {
        return "Corpse of " + character.name;
    }

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }
        return null;
    }
    #endregion
}