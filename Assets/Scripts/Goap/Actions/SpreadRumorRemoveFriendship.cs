using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class SpreadRumorRemoveFriendship : GoapAction {

    public Character rumoredCharacter { get; private set; } //This is the character whom the actor wants the poiTarget to remove friendship with
    public List<Log> crimeMemoriesInvolvingRumoredCharacter { get; private set; }
    public Log chosenMemory { get; private set; }

    public SpreadRumorRemoveFriendship(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Friend", targetPOI = rumoredCharacter });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            weights.AddElement("Break Friendship Success", 10);
            //weights.AddElement("Break Friendship Fail", 20);
            SetState(weights.PickRandomElementGivenWeights());
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 15;
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 2 && otherData[0] is Character && otherData[1] is List<Log>) {
            rumoredCharacter = otherData[0] as Character;
            //int dayTo = GameManager.days;
            //int dayFrom = dayTo - 3;
            //if (dayFrom < 1) {
            //    dayFrom = 1;
            //}
            //crimeMemoriesInvolvingRumoredCharacter = actor.GetCrimeMemories(dayFrom, dayTo, rumoredCharacter);
            crimeMemoriesInvolvingRumoredCharacter = otherData[1] as List<Log>;
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructPreconditionsAndEffects();
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(rumoredCharacter != null) {
            Character target = poiTarget as Character;
            if (target.relationshipContainer.HasRelationshipWith(rumoredCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                return actor != poiTarget && actor != rumoredCharacter && crimeMemoriesInvolvingRumoredCharacter.Count > 0;
            }
            return false;
        }
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreBreakFriendshipSuccess() {
        Character target = poiTarget as Character;
        chosenMemory = crimeMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, crimeMemoriesInvolvingRumoredCharacter.Count)];
        //currentState.AddLogFiller(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        //currentState.AddLogFiller(null, Utilities.LogReplacer(_chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(null, Utilities.LogDontReplace(chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        currentState.AddLogFillers(chosenMemory.goapAction.currentState.descriptionLog.fillers);
        currentState.AddLogFiller(actor, actor.name, LOG_IDENTIFIER.OTHER);
        currentState.AddLogFiller(target, target.name, LOG_IDENTIFIER.OTHER_2);
    }
    public void AfterBreakFriendshipSuccess() {
        if (chosenMemory.goapAction.currentState.shareIntelReaction != null) {
            chosenMemory.goapAction.currentState.shareIntelReaction.Invoke(poiTarget as Character, null, SHARE_INTEL_STATUS.INFORMED);
        }
        //Character target = poiTarget as Character;
        ////**Effect 1**: Target - Remove Friend relationship with Character 2
        //RelationshipManager.Instance.RemoveOneWayRelationship(target, rumoredCharacter, RELATIONSHIP_TRAIT.FRIEND);
        ////**Effect 2**: Target - Add shared event to Target's memory
        //target.CreateInformedEventLog(chosenMemory.goapAction);
    }
    //public void PreBreakFriendshipFail() {
    //    Character target = poiTarget as Character;
    //    chosenMemory = crimeMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, crimeMemoriesInvolvingRumoredCharacter.Count)];
    //    currentState.AddLogFiller(null, Utilities.LogReplacer(chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
    //}
    //public void AfterBreakFriendshipFail() {
    //    Character target = poiTarget as Character;

    //    //**Effect 2**: Target - Add shared event to Target's memory
    //    target.CreateInformedEventLog(chosenMemory.goapAction);
    //    //Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", _chosenMemory.goapAction);
    //    //informedLog.AddToFillers(target, target.name, LOG_IDENTIFIER.OTHER);
    //    //informedLog.AddToFillers(null, Utilities.LogDontReplace(_chosenMemory), LOG_IDENTIFIER.APPEND);
    //    //informedLog.AddToFillers(_chosenMemory.fillers);
    //    //target.AddHistory(informedLog);
    //}
    public void PreTargetMissing() {
        currentState.AddLogFiller(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
    }
    #endregion
}

public class SpreadRumorRemoveFriendshipData : GoapActionData {
    public SpreadRumorRemoveFriendshipData() : base(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character rumoredCharacter = null;
        List<Log> crimeMemoriesInvolvingRumoredCharacter = null;
        if(otherData != null && otherData.Length == 2 && otherData[0] is Character && otherData[1] is List<Log>) {
            rumoredCharacter = otherData[0] as Character;
            crimeMemoriesInvolvingRumoredCharacter = otherData[1] as List<Log>;
        }
        if (rumoredCharacter != null) {
            Character target = poiTarget as Character;
            if (target.relationshipContainer.HasRelationshipWith(rumoredCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                return actor != poiTarget && actor != rumoredCharacter 
                    && crimeMemoriesInvolvingRumoredCharacter != null && crimeMemoriesInvolvingRumoredCharacter.Count > 0;
            }
            return false;
        }
        return actor != poiTarget;
    }
}