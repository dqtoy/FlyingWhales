using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmallAnimal : TileObject, IPointOfInterest {

    private const int Replenishment_Countdown = 96;

    public LocationStructure location { get; private set; }

    public SmallAnimal(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_SMALL_ANIMAL, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.SMALL_ANIMAL);
    }

    #region Overrides
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        //SetPOIState(POI_STATE.INACTIVE);
        //ScheduleCooldown(action);
    }
    //public override List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
    //    if (actor.GetTrait("Carnivore") != null) { //Carnivores only
    //        return base.AdvertiseActionsToActor(actor, actorAllowedInteractions);
    //    }
    //    return null;
    //}
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        //Debug.Log(GameManager.Instance.TodayLogString() + "Set " + this.ToString() + "' state to " + state.ToString());
        gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
        if (state == POI_STATE.INACTIVE) {
            ScheduleCooldown();
        }
    }
    public override string ToString() {
        return "Small Animal " + id.ToString();
    }
    #endregion

    private void ScheduleCooldown() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(Replenishment_Countdown);
        //Debug.Log("Will set " + this.ToString() + " as active again on " + GameManager.Instance.ConvertDayToLogString(dueDate));
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE));
    }
}
