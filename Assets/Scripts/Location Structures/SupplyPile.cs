using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }
    public int suppliesInPile { get; private set; }

    public SupplyPile(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_SUPPLY, INTERACTION_TYPE.DROP_SUPPLY };
        Initialize(TILE_OBJECT_TYPE.SUPPLY_PILE);
        SetSuppliesInPile(50);
        Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if(this.state != state) {
            if (state == POI_STATE.INACTIVE) {
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
            if (!location.location.jobQueue.HasJob("Obtain Supply", this)) {
                GoapPlanJob job = new GoapPlanJob("Obtain Supply", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = this });
                job.SetCanTakeThisJobChecker(CanCharacterTakeThisJob);
                location.location.jobQueue.AddJobInQueue(job);
            }
        } else {
            location.location.jobQueue.CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION.HAS_SUPPLY, this);
        }
    }
    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    private bool CanCharacterTakeThisJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Supply Pile " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }
}
