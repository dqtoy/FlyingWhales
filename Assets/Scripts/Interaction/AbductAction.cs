using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductAction : Interaction {

    private Character targetCharacter;

    private const string Start = "Start";
    private const string Assisted_Abduction_Success = "Assisted Abduction Success";
    private const string Assisted_Abduction_Fail = "Assisted Abduction Fail";
    private const string Assisted_Abduction_Critical_Fail = "Assisted Abduction Critical Fail";
    private const string Thwarted_Abduction_Success = "Thwarted Abduction Success";
    private const string Thwarted_Abduction_Fail = "Thwarted Abduction Fail";
    private const string Thwarted_Abduction_Critical_Fail = "Thwarted Abduction Critical Fail";
    private const string Normal_Abduction_Success = "Normal Abduction Success";
    private const string Normal_Abduction_Fail = "Normal Abduction Fail";
    private const string Normal_Abduction_Critical_Fail = "Normal Abduction Critical Fail";

    public AbductAction(BaseLandmark interactable): base(interactable, INTERACTION_TYPE.ABDUCT_ACTION, 0) {
        _name = "Abduct Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        if(targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState assistedAbductionSuccess = new InteractionState(Assisted_Abduction_Success, this);
        InteractionState assistedAbductionFail = new InteractionState(Assisted_Abduction_Fail, this);
        InteractionState assistedAbductionCriticalFail = new InteractionState(Assisted_Abduction_Critical_Fail, this);
        InteractionState thwartedAbductionSuccess = new InteractionState(Thwarted_Abduction_Success, this);
        InteractionState thwartedAbductionFail = new InteractionState(Thwarted_Abduction_Fail, this);
        InteractionState thwartedAbductionCriticalFail = new InteractionState(Thwarted_Abduction_Critical_Fail, this);
        InteractionState normalAbductionSuccess = new InteractionState(Normal_Abduction_Success, this);
        InteractionState normalAbductionFail = new InteractionState(Normal_Abduction_Fail, this);
        InteractionState normalAbductionCriticalFail = new InteractionState(Normal_Abduction_Critical_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.homeLandmark.tileLocation.areaOfTile, _characterInvolved.homeLandmark.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        assistedAbductionSuccess.SetEffect(() => AssistedAbductionSuccessRewardEffect(assistedAbductionSuccess));
        assistedAbductionFail.SetEffect(() => AssistedAbductionFailRewardEffect(assistedAbductionFail));
        assistedAbductionCriticalFail.SetEffect(() => AssistedAbductionCriticalFailRewardEffect(assistedAbductionCriticalFail));

        thwartedAbductionSuccess.SetEffect(() => ThwartedAbductionSuccessRewardEffect(thwartedAbductionSuccess));
        thwartedAbductionFail.SetEffect(() => ThwartedAbductionFailRewardEffect(thwartedAbductionFail));
        thwartedAbductionCriticalFail.SetEffect(() => ThwartedAbductionCriticalFailRewardEffect(thwartedAbductionCriticalFail));

        normalAbductionSuccess.SetEffect(() => NormalAbductionSuccessRewardEffect(normalAbductionSuccess));
        normalAbductionFail.SetEffect(() => NormalAbductionFailRewardEffect(normalAbductionFail));
        normalAbductionCriticalFail.SetEffect(() => NormalAbductionCriticalFailRewardEffect(normalAbductionCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(thwartedAbductionSuccess.name, thwartedAbductionSuccess);
        _states.Add(thwartedAbductionFail.name, thwartedAbductionFail);
        _states.Add(thwartedAbductionCriticalFail.name, thwartedAbductionCriticalFail);

        _states.Add(assistedAbductionSuccess.name, assistedAbductionSuccess);
        _states.Add(assistedAbductionFail.name, assistedAbductionFail);
        _states.Add(assistedAbductionCriticalFail.name, assistedAbductionCriticalFail);

        _states.Add(normalAbductionSuccess.name, normalAbductionSuccess);
        _states.Add(normalAbductionFail.name, normalAbductionFail);
        _states.Add(normalAbductionCriticalFail.name, normalAbductionCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the abduction.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                disabledTooltipText = "Minion must be an Instigator",
            };
            ActionOption thwart = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Thwart the abduction.",
                effect = () => ThwartOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                disabledTooltipText = "Minion must be a Diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(assist);
            state.AddActionOption(thwart);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        SetTargetCharacter(GetTargetCharacter(character));
        if (targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
            nextState = Assisted_Abduction_Success;
            break;
            case RESULT.FAIL:
            nextState = Assisted_Abduction_Fail;
            break;
            case RESULT.CRITICAL_FAIL:
            nextState = Assisted_Abduction_Critical_Fail;
            break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void ThwartOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
            nextState = Thwarted_Abduction_Success;
            break;
            case RESULT.FAIL:
            nextState = Thwarted_Abduction_Fail;
            break;
            case RESULT.CRITICAL_FAIL:
            nextState = Thwarted_Abduction_Critical_Fail;
            break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
            nextState = Normal_Abduction_Success;
            break;
            case RESULT.FAIL:
            nextState = Normal_Abduction_Fail;
            break;
            case RESULT.CRITICAL_FAIL:
            nextState = Normal_Abduction_Critical_Fail;
            break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedAbductionSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));

        /* Mechanics**: Abduct Character 2. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        AbductCharacter(targetCharacter);
        //**Level Up**: Abductioner Character +1, Instigator Minion +1
        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();
        //**Mechanics**: Relationship between the two factions -1
        AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
    }
    private void AssistedAbductionFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void AssistedAbductionCriticalFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void ThwartedAbductionSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        AbductCharacter(targetCharacter);
        //**Level Up**: Abductioner Character +1
        _characterInvolved.LevelUp();
    }
    private void ThwartedAbductionFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Level Up**: Diplomat Minion +1
        investigatorMinion.LevelUp();
        //**Mechanics**: Player relationship with abductee's faction +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, targetCharacter.faction, 1, state);
    }
    private void ThwartedAbductionCriticalFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Mechanics**: Player relationship with abductee's faction +1 Relationship between the two factions -1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, targetCharacter.faction, 1, state);
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);

        //**Level Up**: Diplomat Minion +1
        investigatorMinion.LevelUp();

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void NormalAbductionSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        AbductCharacter(targetCharacter);
        //**Level Up**: Abductioner Character +1
        _characterInvolved.LevelUp();
    }
    private void NormalAbductionFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
 
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalAbductionCriticalFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Character Name 1 dies. Relationship between the two factions -1
        AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);

        _characterInvolved.Death();
    }
    #endregion

    private void AbductCharacter(Character character) {
        //only add abducted trait to characters that have not been abducted yet, this is to retain it's original faction
        Abducted abductedTrait = new Abducted(character.homeLandmark);
        character.AddTrait(abductedTrait);
        character.MigrateTo(_characterInvolved.homeLandmark);
        Interaction interactionAbductor = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation.tileLocation.landmarkOnTile);
        Interaction interactionAbducted = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation.tileLocation.landmarkOnTile);
        _characterInvolved.SetForcedInteraction(interactionAbductor);
        _characterInvolved.SetDailyInteractionGenerationTick(GameManager.Instance.continuousDays + 1);
        character.SetForcedInteraction(interactionAbducted);
        character.SetDailyInteractionGenerationTick(GameManager.Instance.continuousDays + 1);
    }

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()) {
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 15;
                } else if (currCharacter.faction.id != characterInvolved.faction.id) {
                    FactionRelationship relationship = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if(relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                        weight += 25;
                    }else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight -= 10;
                    }
                    if(currCharacter.level > characterInvolved.level) {
                        weight -= 15;
                    }else if(currCharacter.level < characterInvolved.level) {
                        weight += 15;
                    }
                }
                if(weight > 0) {
                    characterWeights.AddElement(currCharacter, weight);
                }
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
