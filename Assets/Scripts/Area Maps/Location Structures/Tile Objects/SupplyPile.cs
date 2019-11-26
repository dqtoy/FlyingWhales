using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : ResourcePile {

    public SupplyPile() : base(RESOURCE.WOOD) {
        Initialize(TILE_OBJECT_TYPE.SUPPLY_PILE);
        SetResourceInPile(2000);
        traitContainer.RemoveTrait(this, "Flammable");
        Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
    }
    public SupplyPile(SaveDataTileObject data) : base(RESOURCE.WOOD) {
        Initialize(data);
        Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if(this.state != state) {
            if (!IsAvailable()) {
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckSupply);
            } else {
                Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
            }
        }
        base.SetPOIState(state);
    }
    public override void SetGridTileLocation(LocationGridTile tile) {
        base.SetGridTileLocation(tile);
        if (tile != null) {
            //when a supply pile is placed, and the area does not yet have a supply pile, then set its supply pile to this
            if (tile.parentAreaMap.area.supplyPile == null) {
                tile.parentAreaMap.area.SetSupplyPile(this);
            }
        }
    }
    #endregion

    private void CheckSupply() {
        if (resourceInPile < 100) {
            if (!structureLocation.location.HasJob(JOB_TYPE.OBTAIN_SUPPLY)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_SUPPLY, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.TARGET ), this, structureLocation.location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
                structureLocation.location.AddToAvailableJobs(job);
            }
        } else {
            structureLocation.location.ForceCancelJob(structureLocation.location.GetJob(JOB_TYPE.OBTAIN_SUPPLY));
        }
    }
    public override void AdjustResourceInPile(int adjustment) {
        base.AdjustResourceInPile(adjustment);
        if (adjustment < 0) {
            Messenger.Broadcast(Signals.SUPPLY_IN_PILE_REDUCED, this);
        }
    }
    public override string ToString() {
        return "Supply Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

public class SaveDataSupplyPile : SaveDataTileObject {
    public int suppliesInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        SupplyPile obj = tileObject as SupplyPile;
        suppliesInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        SupplyPile obj = base.Load() as SupplyPile;
        obj.SetResourceInPile(suppliesInPile);
        return obj;
    }
}