using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodPile : TileObject {
    public LocationStructure location { get; private set; }
    public int foodInPile { get; private set; }

    public FoodPile(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_FOOD, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(TILE_OBJECT_TYPE.FOOD_PILE);
        SetFoodInPile(2000);
        RemoveTrait("Flammable");
        Messenger.AddListener(Signals.TICK_STARTED, CheckFood);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if(this.state != state) {
            if (!IsAvailable()) {
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckFood);
            } else {
                Messenger.AddListener(Signals.TICK_STARTED, CheckFood);
            }
        }
        base.SetPOIState(state);
    }
    #endregion

    private void CheckFood() {
        //if (foodInPile < 100) {
        //    if (!location.location.jobQueue.HasJob(JOB_TYPE.OBTAIN_SUPPLY, this)) {
        //        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_SUPPLY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = this });
        //        job.SetCanTakeThisJobChecker(CanCharacterTakeThisJob);
        //        location.location.jobQueue.AddJobInQueue(job);
        //    }
        //} else {
        //    location.location.jobQueue.CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION.HAS_SUPPLY, this);
        //}
    }
    public void SetFoodInPile(int amount) {
        foodInPile = amount;
        foodInPile = Mathf.Max(0, foodInPile);
    }

    public void AdjustFoodInPile(int adjustment) {
        foodInPile += adjustment;
        foodInPile = Mathf.Max(0, foodInPile);
    }

    private bool CanCharacterTakeThisJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return foodInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Food Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}
