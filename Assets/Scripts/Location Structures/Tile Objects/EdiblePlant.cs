using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdiblePlant : TileObject, IPointOfInterest {

    private const int Replenishment_Countdown = 96;
    public LocationStructure location { get; private set; }

    public EdiblePlant(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_PLANT, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.EDIBLE_PLANT);
    }

    #region Overrides
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        SetPOIState(POI_STATE.INACTIVE);
        ScheduleCooldown(action);
    }
    public override List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (actor.GetTrait("Herbivore") != null) { //Herbivores only
            return base.AdvertiseActionsToActor(actor, actorAllowedInteractions);
        }
        return null;
    }
    public override string ToString() {
        return "Edible Plant " + id.ToString();
    }
    #endregion

    private void ScheduleCooldown(GoapAction action) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(Replenishment_Countdown);
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE));
    }
}
