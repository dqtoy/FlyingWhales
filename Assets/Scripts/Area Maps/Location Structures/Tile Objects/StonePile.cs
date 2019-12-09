using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StonePile : ResourcePile {

    public StonePile() : base(RESOURCE.STONE) {
        Initialize(TILE_OBJECT_TYPE.STONE_PILE);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
        Messenger.AddListener(Signals.HOUR_STARTED, CheckSupply);
    }
    public StonePile(SaveDataTileObject data) : base(RESOURCE.STONE) {
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
    //        //when a supply pile is placed, and the area does not yet have a supply pile, then set its supply pile to this
    //        if (tile.parentAreaMap.area.supplyPile == null) {
    //            tile.parentAreaMap.area.SetSupplyPile(this);
    //        }
    //    }
    //}
    #endregion

    private void CheckSupply() {
        if (gridTileLocation != null) {
            if (structureLocation == structureLocation.location.mainStorage) {
                if (resourceInPile < 100) {
                    if (!structureLocation.location.HasJob(JOB_TYPE.PRODUCE_STONE)) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_STONE, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_STONE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), this, structureLocation.location);
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
                        structureLocation.location.AddToAvailableJobs(job);
                    }
                } else {
                    ForceCancelNotAssignedProduceJob(JOB_TYPE.PRODUCE_STONE);
                }
            } else {
                CreateHaulJob();
            }
        }
    }
    //public override void AdjustResourceInPile(int adjustment) {
    //    base.AdjustResourceInPile(adjustment);
    //    if (adjustment < 0) {
    //        Messenger.Broadcast(Signals.STONE_IN_PILE_REDUCED, this);
    //    }
    //}
    public override string ToString() {
        return "Stone Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

public class SaveDataStonePile : SaveDataTileObject {
    public int suppliesInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        StonePile obj = tileObject as StonePile;
        suppliesInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        StonePile obj = base.Load() as StonePile;
        obj.SetResourceInPile(suppliesInPile);
        return obj;
    }
}