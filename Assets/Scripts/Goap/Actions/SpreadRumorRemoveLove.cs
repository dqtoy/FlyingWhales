using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadRumorRemoveLove : GoapAction {

    public Character rumoredCharacter { get; private set; } //This is the character whom the actor wants the poiTarget to remove love with
    public List<Log> affairMemoriesInvolvingRumoredCharacter { get; private set; }
    public Log chosenMemory { get; private set; }

    public SpreadRumorRemoveLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = rumoredCharacter });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Paramour", targetPOI = rumoredCharacter });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing) {
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            weights.AddElement("Break Love Success", 10);
            //weights.AddElement("Break Love Fail", 20);
            SetState(weights.PickRandomElementGivenWeights());
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
        return 15;
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 2 && otherData[0] is Character && otherData[1] is List<Log>) {
            rumoredCharacter = otherData[0] as Character;
            affairMemoriesInvolvingRumoredCharacter = otherData[1] as List<Log>;
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructBasePreconditionsAndEffects();
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
        if (rumoredCharacter != null) {
            Character target = poiTarget as Character;
            if (target.HasRelationshipOfTypeWith(rumoredCharacter, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
                return actor != poiTarget && actor != rumoredCharacter && affairMemoriesInvolvingRumoredCharacter.Count > 0;
            }
            return false;
        }
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreBreakLoveSuccess() {
        Character target = poiTarget as Character;
        chosenMemory = affairMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, affairMemoriesInvolvingRumoredCharacter.Count)];
        currentState.AddLogFiller(null, Utilities.LogDontReplace(chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        currentState.AddLogFillers(chosenMemory.goapAction.currentState.descriptionLog.fillers);
        currentState.AddLogFiller(actor, actor.name, LOG_IDENTIFIER.OTHER);
        currentState.AddLogFiller(target, target.name, LOG_IDENTIFIER.OTHER_2);
        //currentState.AddLogFiller(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        //currentState.AddLogFiller(null, Utilities.LogReplacer(chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterBreakLoveSuccess() {
        if (chosenMemory.goapAction.currentState.shareIntelReaction != null) {
            chosenMemory.goapAction.currentState.shareIntelReaction.Invoke(poiTarget as Character, null, SHARE_INTEL_STATUS.INFORMED);
        }
        //Character target = poiTarget as Character;
        ////**Effect 1**: Target - Remove Love relationship with Character 2 
        //CharacterManager.Instance.RemoveRelationshipBetween(target, rumoredCharacter, RELATIONSHIP_TRAIT.LOVER);
        //CharacterManager.Instance.RemoveRelationshipBetween(target, rumoredCharacter, RELATIONSHIP_TRAIT.PARAMOUR);
        ////**Effect 2**: Target - Add shared event to Target's memory
        //target.CreateInformedEventLog(chosenMemory.goapAction);
    }
    //public void PreBreakLoveFail() {
    //    Character target = poiTarget as Character;
    //    chosenMemory = affairMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, affairMemoriesInvolvingRumoredCharacter.Count)];
    //    currentState.AddLogFiller(null, Utilities.LogReplacer(chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
    //}
    //public void AfterBreakLoveFail() {
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

public class SpreadRumorRemoveLoveData : GoapActionData {
    public SpreadRumorRemoveLoveData() : base(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character rumoredCharacter = null;
        List<Log> affairMemoriesInvolvingRumoredCharacter = null;
        if (otherData != null && otherData.Length == 2 && otherData[0] is Character && otherData[1] is List<Log>) {
            rumoredCharacter = otherData[0] as Character;
            affairMemoriesInvolvingRumoredCharacter = otherData[1] as List<Log>;
        }
        if (rumoredCharacter != null) {
            Character target = poiTarget as Character;
            if (target.HasRelationshipOfTypeWith(rumoredCharacter, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
                return actor != poiTarget && actor != rumoredCharacter
                    && affairMemoriesInvolvingRumoredCharacter != null && affairMemoriesInvolvingRumoredCharacter.Count > 0;
            }
            return false;
        }
        return actor != poiTarget;
    }
}