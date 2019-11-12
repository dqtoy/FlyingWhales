using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class AbductCharacter : GoapAction {
    public override LocationStructure targetStructure { get { return structureToBeDropped; } }

    private LocationStructure structureToBeDropped;
    private LocationGridTile gridTileToBeDropped;

    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //isNotificationAnIntel = false;
        whileMovingState = "In Progress";
    }

    #region Overrides
    //protected override void ConstructRequirement() {
    //    _requirementAction = Requirement;
    //}
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.RESTRAIN_CARRY, conditionKey = actor, targetPOI = poiTarget }, IsInActorPartyAndRestrained);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY_NO_CONSENT, conditionKey = actor.homeRegion, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Abduct Success");
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        if (gridTileToBeDropped != null) {
            return gridTileToBeDropped;
        }
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionWhileTravelling() {
        base.OnStopActionWhileTravelling();
        Character targetCharacter = poiTarget as Character;
        actor.currentParty.RemoveCharacter(targetCharacter);
    }
    public override void OnStopActionDuringCurrentState() {
        base.OnStopActionDuringCurrentState();
        Character targetCharacter = poiTarget as Character;
        actor.currentParty.RemoveCharacter(targetCharacter);
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is LocationStructure) {
            structureToBeDropped = otherData[0] as LocationStructure;
            return true;
        } else if (otherData.Length == 2 && otherData[0] is LocationStructure && otherData[1] is LocationGridTile) {
            structureToBeDropped = otherData[0] as LocationStructure;
            gridTileToBeDropped = otherData[1] as LocationGridTile;
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Preconditions
    private bool IsInActorPartyAndRestrained() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty && target.traitContainer.GetNormalTrait("Restrained") != null;
    }
    #endregion

    #region State Effects
    public void PreInProgress() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        currentState.SetIntelReaction(AbductInProgressIntelReaction);
    }
    public void PreAbductSuccess() {
        //Will not SetCommittedCrime anymore because it is already set in PreInProgress, and since this action is an exception where two states will be called, it will be redundant if we set it again here.
        //In Progress state will always be called first before Abduct Success state
        currentState.AddLogFiller(actor.currentStructure, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(AbductInProgressIntelReaction);
    }
    public void AfterAbductSuccess() {
        Character target = poiTarget as Character;
        actor.currentParty.RemoveCharacter(target, dropLocation: gridTileToBeDropped);
        //if (gridTileToBeDropped.objHere != null && gridTileToBeDropped.objHere is TileObject) {
        //    TileObject to = gridTileToBeDropped.objHere as TileObject;
        //    to.AddUser(target);
        //}
        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeRegion, targetPOI = poiTarget });
    }
    #endregion

    #region Intel Reactions
    private List<string> AbductInProgressIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        //Recipient and Target have at least one non-negative relationship and Actor is not from the same faction:
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                if (recipient != actor && recipient != targetCharacter && recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("I think {0} is going to do something bad to {1}. I must stop this!", actor.name, targetCharacter.name));
                    recipient.ReactToCrime(committedCrime, this, actor.currentAlterEgo, status);
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class AbductCharacterData: GoapActionData {
    public AbductCharacterData() : base(INTERACTION_TYPE.ABDUCT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}