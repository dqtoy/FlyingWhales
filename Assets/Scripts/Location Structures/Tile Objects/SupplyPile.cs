using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : TileObject {
    public int suppliesInPile { get; private set; }

    public SupplyPile(LocationStructure location) {
        SetStructureLocation(location);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_SUPPLY, INTERACTION_TYPE.DROP_RESOURCE, INTERACTION_TYPE.REPAIR_TILE_OBJECT, INTERACTION_TYPE.DESTROY_RESOURCE };
        Initialize(TILE_OBJECT_TYPE.SUPPLY_PILE);
        SetSuppliesInPile(2000);
        traitContainer.RemoveTrait(this, "Flammable");
        Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
    }
    public SupplyPile(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_SUPPLY, INTERACTION_TYPE.DROP_RESOURCE, INTERACTION_TYPE.REPAIR_TILE_OBJECT, INTERACTION_TYPE.DESTROY_RESOURCE };
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
    #endregion

    private void CheckSupply() {
        if (suppliesInPile < 100) {
            if (!structureLocation.location.HasJob(JOB_TYPE.OBTAIN_SUPPLY)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_SUPPLY, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_SUPPLY, "0", true, GOAP_EFFECT_TARGET.TARGET ), this, structureLocation.location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
                structureLocation.location.AddToAvailableJobs(job);
            }
        } else {
            structureLocation.location.ForceCancelJob(structureLocation.location.GetJob(JOB_TYPE.OBTAIN_SUPPLY));
        }
    }
    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
        if (adjustment < 0) {
            Messenger.Broadcast(Signals.SUPPLY_IN_PILE_REDUCED, this);
        }
    }
    public bool HasSupply() {
        if (structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
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
        suppliesInPile = obj.suppliesInPile;
    }

    public override TileObject Load() {
        SupplyPile obj = base.Load() as SupplyPile;
        obj.SetSuppliesInPile(suppliesInPile);
        return obj;
    }
}