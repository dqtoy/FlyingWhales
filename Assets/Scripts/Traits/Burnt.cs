using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnt : Trait {

    private Color burntColor {
        get {
            return Color.gray;
        }
    }

    public Burnt() {
        name = "Burnt";
        description = "This is burnt.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if (addedTo is LocationGridTile) {
            LocationGridTile tile = addedTo as LocationGridTile;
            tile.parentTileMap.SetColor(tile.localPlace, burntColor);
            tile.SetDefaultTileColor(burntColor);
            tile.parentAreaMap.detailsTilemap.SetColor(tile.localPlace, burntColor);
            tile.parentAreaMap.northEdgeTilemap.SetColor(tile.localPlace, burntColor);
            tile.parentAreaMap.southEdgeTilemap.SetColor(tile.localPlace, burntColor);
            tile.parentAreaMap.eastEdgeTilemap.SetColor(tile.localPlace, burntColor);
            tile.parentAreaMap.westEdgeTilemap.SetColor(tile.localPlace, burntColor);
            if (tile.objHere == null) {
                tile.parentAreaMap.objectsTilemap.SetColor(tile.localPlace, burntColor);
            }
        } else if (addedTo is TileObject) {
            TileObject obj = addedTo as TileObject;
            obj.SetPOIState(POI_STATE.INACTIVE);
            obj.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(obj.gridTileLocation.localPlace, burntColor);
            obj.SetSlotColor(burntColor);
        } else if (addedTo is SpecialToken) {
            SpecialToken token = addedTo as SpecialToken;
            token.SetPOIState(POI_STATE.INACTIVE);
            token.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(token.gridTileLocation.localPlace, burntColor);
        }
    }

    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is TileObject) {
            TileObject targetPOI = traitOwner as TileObject;
            if (!targetPOI.HasJobTargettingThis(JOB_TYPE.REPAIR)) {
                GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", targetPOI);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPAIR, effect);
                job.SetCanBeDoneInLocation(true);
                if (CanCharacterTakeRepairJob(characterThatWillDoJob, null)) {
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    job.SetCanTakeThisJobChecker(CanCharacterTakeRepairJob);
                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
