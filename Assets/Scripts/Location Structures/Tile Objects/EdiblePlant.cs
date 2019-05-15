using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdiblePlant : TileObject {

    private const int Replenishment_Countdown = 96;

    public EdiblePlant(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_PLANT, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.EDIBLE_PLANT);
    }

    #region Overrides
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        //SetPOIState(POI_STATE.INACTIVE);
        //ScheduleCooldown(action);
    }
    //public override List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
    //    if (actor.GetTrait("Herbivore") != null) { //Herbivores only
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
        return "Edible Plant " + id.ToString();
    }
    #endregion

    private void ScheduleCooldown() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(Replenishment_Countdown);
        //Debug.Log("Will set " + this.ToString() + " as active again on " + GameManager.Instance.ConvertDayToLogString(dueDate));
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE));
    }
}
