using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
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
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is LocationGridTile) {
                LocationGridTile tile = removedFrom as LocationGridTile;
                tile.parentTileMap.SetColor(tile.localPlace, Color.white);
                tile.SetDefaultTileColor(Color.white);
                tile.parentAreaMap.detailsTilemap.SetColor(tile.localPlace, Color.white);
                tile.parentAreaMap.northEdgeTilemap.SetColor(tile.localPlace, Color.white);
                tile.parentAreaMap.southEdgeTilemap.SetColor(tile.localPlace, Color.white);
                tile.parentAreaMap.eastEdgeTilemap.SetColor(tile.localPlace, Color.white);
                tile.parentAreaMap.westEdgeTilemap.SetColor(tile.localPlace, Color.white);
                if (tile.objHere == null) {
                    tile.parentAreaMap.objectsTilemap.SetColor(tile.localPlace, Color.white);
                }
            } else if (removedFrom is TileObject) {
                TileObject obj = removedFrom as TileObject;
                obj.SetPOIState(POI_STATE.ACTIVE);
                obj.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(obj.gridTileLocation.localPlace, Color.white);
                obj.SetSlotColor(Color.white);
            } else if (removedFrom is SpecialToken) {
                SpecialToken token = removedFrom as SpecialToken;
                token.SetPOIState(POI_STATE.ACTIVE);
                token.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(token.gridTileLocation.localPlace, Color.white);
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is TileObject) {
                TileObject targetPOI = traitOwner as TileObject;
                if (targetPOI.advertisedActions.Contains(INTERACTION_TYPE.REPAIR)) {
                    GoapPlanJob currentJob = targetPOI.GetJobTargettingThisCharacter(JOB_TYPE.REPAIR);
                    if (currentJob == null) {
                        GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET);
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPAIR, effect, targetPOI, characterThatWillDoJob);
                        //job.SetCanBeDoneInLocation(true);
                        if (InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob, job)) {
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                        //else {
                        //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairJob);
                        //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                        //    return false;
                        //}
                    } 
                    //else {
                    //    if (InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob, currentJob)) {
                    //        return TryTransferJob(currentJob, characterThatWillDoJob);
                    //    }
                    //}
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }
}

