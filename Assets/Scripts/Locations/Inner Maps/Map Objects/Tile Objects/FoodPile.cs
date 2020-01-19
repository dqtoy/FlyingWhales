using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodPile : ResourcePile {

    public FoodPile() : base(RESOURCE.FOOD) {
        Initialize(TILE_OBJECT_TYPE.FOOD_PILE);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
        Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
    }
    public FoodPile(SaveDataTileObject data) : base(RESOURCE.FOOD) {
        Initialize(data);
        Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if (this.state != state) {
            if (!IsAvailable()) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckSupply);
            } else {
                Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
            }
        }
        base.SetPOIState(state);
    }
    public override string ToString() {
        return "Food Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
    #endregion

    private void CheckSupply() {
        if (gridTileLocation != null) {
            if (structureLocation.settlementLocation != null && structureLocation.settlementLocation.mainStorage == structureLocation) {
                if (resourceInPile < 100) {
                    if (structureLocation.settlementLocation != null && !structureLocation.settlementLocation.HasJob(JOB_TYPE.PRODUCE_FOOD)) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, structureLocation.settlementLocation);
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
                        structureLocation.settlementLocation.AddToAvailableJobs(job);
                    }
                } else {
                    ForceCancelNotAssignedProduceJob(JOB_TYPE.PRODUCE_FOOD);
                    //structureLocation.location.ForceCancelJob(structureLocation.location.GetJob(JOB_TYPE.OBTAIN_FOOD));
                }
            } 
            //else {
            //    CreateHaulJob();
            //}
        }
    }
}

public class SaveDataFoodPile : SaveDataTileObject {
    public int foodInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        FoodPile obj = tileObject as FoodPile;
        foodInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        FoodPile obj = base.Load() as FoodPile;
        obj.SetResourceInPile(foodInPile);
        return obj;
    }
}