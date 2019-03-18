using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmallAnimal : TileObject, IPointOfInterest {

    private const int Replenishment_Countdown = 96;

    public string name { get { return ToString(); } }
    public LocationStructure location { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; protected set; }

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

    public SmallAnimal(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_SMALL_ANIMAL };
        Initialize(this, TILE_OBJECT_TYPE.SMALL_ANIMAL);
    }

    #region Overrides
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        ScheduleCooldown(action);
    }
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (actor.GetTrait("Carnivore") != null) { //Carnivores only
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
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public override string ToString() {
        return "Small Animal " + id.ToString();
    }
    #endregion

    #region Interface
    public void SetGridTileLocation(LocationGridTile tile) {
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


    private void ScheduleCooldown(GoapAction action) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(Replenishment_Countdown);
        SchedulingManager.Instance.AddEntry(dueDate, () => OnDoneActionTowardsTarget(action));
    }
}
