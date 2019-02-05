using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AskForHelp : Interaction {
    private const string Start = "Start";
    private const string Ask_Help_Successful = "Ask Help Successful";
    private const string Ask_Help_Fail = "Ask Help Fail";

    private Character _otherCharacter;
    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public AskForHelp(Area interactable) : base(interactable, INTERACTION_TYPE.ASK_FOR_HELP, 0) {
        _name = "Ask For Help";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState askHelpSuccessfulState = new InteractionState(Ask_Help_Successful, this);
        InteractionState askHelpFailState = new InteractionState(Ask_Help_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startStateDescriptionLog.AddToFillers(_otherCharacter, _otherCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        askHelpSuccessfulState.SetEffect(() => AskSuccessfulEffect(askHelpSuccessfulState));
        askHelpFailState.SetEffect(() => AskFailEffect(askHelpFailState));

        _states.Add(startState.name, startState);
        _states.Add(askHelpSuccessfulState.name, askHelpSuccessfulState);
        _states.Add(askHelpFailState.name, askHelpFailState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    public override void SetOtherCharacter(Character character) {
        _otherCharacter = character;
    }
    #endregion

    #region Action Options
    private void DoNothingOption() {
        WeightedDictionary<string> weights = new WeightedDictionary<string>();
        weights.AddElement(Ask_Help_Successful, 100);

        int failWeight = 30;
        if(_targetCharacter.GetRelationshipTraitWith(_otherCharacter, RELATIONSHIP_TRAIT.ENEMY) != null) {
            failWeight = 500;
        } else if (_targetCharacter.faction.id != FactionManager.Instance.neutralFaction.id && _otherCharacter.faction.id != FactionManager.Instance.neutralFaction.id) {
            FactionRelationship factionRel = _targetCharacter.faction.GetRelationshipWith(_otherCharacter.faction);
            if(factionRel != null && factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                failWeight = 500;
            }
        }
        weights.AddElement(Ask_Help_Fail, failWeight);

        string result = weights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void AskSuccessfulEffect(InteractionState state) {
        //Add relationship save target and saver

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_otherCharacter, _otherCharacter.name, LOG_IDENTIFIER.CHARACTER_3);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_otherCharacter, _otherCharacter.name, LOG_IDENTIFIER.CHARACTER_3));
    }
    private void AskFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_otherCharacter, _otherCharacter.name, LOG_IDENTIFIER.CHARACTER_3);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_otherCharacter, _otherCharacter.name, LOG_IDENTIFIER.CHARACTER_3));

        StartMoveToAction();
    }
    #endregion
}
