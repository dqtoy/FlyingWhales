using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class PlayGuitar : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PlayGuitar() : base(INTERACTION_TYPE.PLAY_GUITAR) {
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT, };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        // showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Play Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ":";
        int cost = Utilities.rng.Next(80, 121);
        costLog += " +" + cost + "(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Played > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += " +" + timesCost + "(10 x Times Played)";
        }
        Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if(trait.name == "Music Hater") {
                cost += 2000;
                costLog += " +2000(Music Hater)";
            } else {
                cost += -15;
                costLog += " -15(Music Lover)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.needsComponent.AdjustDoNotGetBored(-1);
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Play Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node) {
        string response = base.ReactionToActor(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        Trait trait = witness.traitContainer.GetNormalTrait<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor);
                if(RelationshipManager.Instance.GetCompatibilityBetween(witness, actor) >= 4
                    && RelationshipManager.Instance.IsSexuallyCompatible(witness, actor)
                    && witness.moodComponent.moodState != MOOD_STATE.CRITICAL) {
                    int value = 50;
                    if (actor.traitContainer.GetNormalTrait<Trait>("Ugly") != null) {
                        value = 20;
                    }
                    if(UnityEngine.Random.Range(0, 100) < value) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor);
                    }
                }
            }
        }
        return response;
    }
    #endregion

    #region State Effects
    public void PrePlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        //TODO: currentState.SetIntelReaction(PlaySuccessIntelReaction);
    }
    public void PerTickPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(4f);
    }
    public void AfterPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    //public void PreTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (actor.traitContainer.GetNormalTrait<Trait>("MusicHater") != null) {
                return false; //music haters will never play guitar
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            LocationGridTile knownLoc = poiTarget.gridTileLocation;
            //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
            if (knownLoc.structure.isDwelling) {
                if (actor.homeStructure == knownLoc.structure) {
                    return true;
                } else {
                    IDwelling dwelling = knownLoc.structure as IDwelling;
                    if (dwelling.IsOccupied()) {
                        for (int i = 0; i < dwelling.residents.Count; i++) {
                            Character currResident = dwelling.residents[i];
                            if (currResident.opinionComponent.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                                return true;
                            }
                        }
                        //the actor does NOT have any positive relations with any resident
                        return false;
                    } else {
                        //in cases that the guitar is at a dwelling with no residents, always allow.
                        return true;
                    }
                }
            } else {
                //in cases that the guitar is not inside a dwelling, always allow.
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> PlaySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if(status == SHARE_INTEL_STATUS.WITNESSED && recipient.traitContainer.GetNormalTrait<Trait>("Music Hater") != null) {
    //        recipient.traitContainer.AddTrait(recipient, "Annoyed");
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            if (recipient.CreateBreakupJob(actor) != null) {
    //                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
    //                log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                log.AddLogToInvolvedObjects();
    //                PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            }
    //        } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
    //            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
    //            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //            log.AddLogToInvolvedObjects();
    //            PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class PlayGuitarData : GoapActionData {
    public PlayGuitarData() : base(INTERACTION_TYPE.PLAY_GUITAR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (actor.traitContainer.GetNormalTrait<Trait>("MusicHater") != null) {
            return false; //music haters will never play guitar
        }
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
        if (knownLoc.structure.isDwelling) {
            if (actor.homeStructure == knownLoc.structure) {
                return true;
            } else {
                IDwelling dwelling = knownLoc.structure as IDwelling;
                if (dwelling.IsOccupied()) {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character currResident = dwelling.residents[i];
                        if (currResident.opinionComponent.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                            return true;
                        }
                    }
                    //the actor does NOT have any positive relations with any resident
                    return false;
                } else {
                    //in cases that the guitar is at a dwelling with no residents, always allow.
                    return true;
                }
            }
        } else {
            //in cases that the guitar is not inside a dwelling, always allow.
            return true;
        }
    }
}