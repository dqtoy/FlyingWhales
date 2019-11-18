using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmallAnimal : TileObject {

    private const int Replenishment_Countdown = 96;

    public SmallAnimal(LocationStructure location) {
        SetStructureLocation(location);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.SMALL_ANIMAL);
    }
    public SmallAnimal(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(data);
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
        if (gridTileLocation != null) {
            //Debug.Log(GameManager.Instance.TodayLogString() + "Set " + this.ToString() + "' state to " + state.ToString());
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
            if (!IsAvailable()) {
                ScheduleCooldown();
            }
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
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE), this);
    }
}
