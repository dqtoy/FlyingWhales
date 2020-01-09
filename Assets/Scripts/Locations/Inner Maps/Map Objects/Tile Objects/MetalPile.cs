using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MetalPile : ResourcePile {

    public MetalPile() : base(RESOURCE.METAL) {
        Initialize(TILE_OBJECT_TYPE.METAL_PILE);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
        Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
    }
    public MetalPile(SaveDataTileObject data) : base(RESOURCE.METAL) {
        Initialize(data);
        Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if(this.state != state) {
            if (!IsAvailable()) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckSupply);
            } else {
                Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
            }
        }
        base.SetPOIState(state);
    }
    //public override void SetGridTileLocation(LocationGridTile tile) {
    //    base.SetGridTileLocation(tile);
    //    if (tile != null) {
    //        //when a supply pile is placed, and the settlement does not yet have a supply pile, then set its supply pile to this
    //        if (tile.parentAreaMap.settlement.supplyPile == null) {
    //            tile.parentAreaMap.settlement.SetSupplyPile(this);
    //        }
    //    }
    //}
    #endregion

    private void CheckSupply() {
        if (gridTileLocation != null) {
            if (structureLocation == structureLocation.location.mainStorage) {
                if (resourceInPile < 100) {
                    if (structureLocation.settlementLocation != null && !structureLocation.settlementLocation.HasJob(JOB_TYPE.PRODUCE_METAL)) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_METAL, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_METAL, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, structureLocation.settlementLocation);
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
                        structureLocation.settlementLocation.AddToAvailableJobs(job);
                    }
                } else {
                    ForceCancelNotAssignedProduceJob(JOB_TYPE.PRODUCE_METAL);
                }
            } 
            //else {
            //    CreateHaulJob();
            //}
        }
    }
    //public override void AdjustResourceInPile(int adjustment) {
    //    base.AdjustResourceInPile(adjustment);
    //    if (adjustment < 0) {
    //        Messenger.Broadcast(Signals.METAL_IN_PILE_REDUCED, this);
    //    }
    //}
    public override string ToString() {
        return "Metal Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

public class SaveDataMetalPile : SaveDataTileObject {
    public int suppliesInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        MetalPile obj = tileObject as MetalPile;
        suppliesInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        MetalPile obj = base.Load() as MetalPile;
        obj.SetResourceInPile(suppliesInPile);
        return obj;
    }
}