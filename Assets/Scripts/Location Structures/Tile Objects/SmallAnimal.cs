using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallAnimal : Food {

    private const int Replenishment_Countdown = 96;

    public SmallAnimal(LocationStructure location, FOOD foodType) : base(location, foodType) {
    }

    #region Overrides
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        ScheduleCooldown(action);
    }
    public override List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (actor.GetTrait("Carnivore") != null) { //Carnivores only
            return base.AdvertiseActionsToActor(actor, actorAllowedInteractions);
        }
        return null;
    }
    #endregion

    private void ScheduleCooldown(GoapAction action) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(Replenishment_Countdown);
        SchedulingManager.Instance.AddEntry(dueDate, () => OnDoneActionTowardsTarget(action));
    }
}
