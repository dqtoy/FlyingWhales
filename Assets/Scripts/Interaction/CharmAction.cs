using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmAction : Interaction {

    private Character targetCharacter;

    private const string Thwarted_Charm_Success = "Thwarted Charm Success";
    private const string Thwarted_Charm_Fail = "Thwarted Charm Fail";
    private const string Thwarted_Charm_Critical_Fail = "Thwarted Charm Critical Fail";
    private const string Assisted_Charm_Success = "Assisted Charm Success";
    private const string Assisted_Charm_Fail = "Assisted Charm Fail";
    private const string Assisted_Charm_Critical_Fail = "Assisted Charm Critical Fail";
    private const string Normal_Charm_Success = "Normal Charm Success";
    private const string Normal_Charm_Fail = "Normal Charm Fail";
    private const string Normal_Charm_Critical_Fail = "Normal Charm Critical Fail";

    public CharmAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.CHARM_ACTION, 0) {
        _name = "Charm Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState thwartedCharmSuccess = new InteractionState(Thwarted_Charm_Success, this);
        InteractionState thwartedCharmFail = new InteractionState(Thwarted_Charm_Fail, this);
        InteractionState thwartedCharmCriticalFail = new InteractionState(Thwarted_Charm_Critical_Fail, this);
        InteractionState assistedCharmSuccess = new InteractionState(Assisted_Charm_Success, this);
        InteractionState assistedCharmFail = new InteractionState(Assisted_Charm_Fail, this);
        InteractionState assistedCharmCriticalFail = new InteractionState(Assisted_Charm_Critical_Fail, this);
        InteractionState normalCharmSuccess = new InteractionState(Normal_Charm_Success, this);
        InteractionState normalCharmFail = new InteractionState(Normal_Charm_Fail, this);
        InteractionState normalCharmCriticalFail = new InteractionState(Normal_Charm_Critical_Fail, this);

        if (targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        thwartedCharmSuccess.SetEffect(() => ThwartedCharmSuccessRewardEffect(thwartedCharmSuccess));
        thwartedCharmFail.SetEffect(() => ThwartedCharmFailRewardEffect(thwartedCharmFail));
        thwartedCharmCriticalFail.SetEffect(() => ThwartedCharmCriticalFailRewardEffect(thwartedCharmCriticalFail));

        assistedCharmSuccess.SetEffect(() => AssistedCharmSuccessRewardEffect(assistedCharmSuccess));
        assistedCharmFail.SetEffect(() => AssistedCharmFailRewardEffect(assistedCharmFail));
        assistedCharmCriticalFail.SetEffect(() => AssistedCharmCriticalFailRewardEffect(assistedCharmCriticalFail));

        normalCharmSuccess.SetEffect(() => NormalCharmSuccessRewardEffect(normalCharmSuccess));
        normalCharmFail.SetEffect(() => NormalCharmFailRewardEffect(normalCharmFail));
        normalCharmCriticalFail.SetEffect(() => NormalCharmCriticalFailRewardEffect(normalCharmCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(thwartedCharmSuccess.name, thwartedCharmSuccess);
        _states.Add(thwartedCharmFail.name, thwartedCharmFail);
        _states.Add(thwartedCharmCriticalFail.name, thwartedCharmCriticalFail);

        _states.Add(assistedCharmSuccess.name, assistedCharmSuccess);
        _states.Add(assistedCharmFail.name, assistedCharmFail);
        _states.Add(assistedCharmCriticalFail.name, assistedCharmCriticalFail);

        _states.Add(normalCharmSuccess.name, normalCharmSuccess);
        _states.Add(normalCharmFail.name, normalCharmFail);
        _states.Add(normalCharmCriticalFail.name, normalCharmCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the Spell.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
            };
            ActionOption thwart = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Thwart the attempt.",
                effect = () => ThwartOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
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
    #endregion

    #region Option Effect
    private void ThwartOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Thwarted_Charm_Success;
                break;
            case RESULT.FAIL:
                nextState = Thwarted_Charm_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Thwarted_Charm_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Assisted_Charm_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Charm_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Charm_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Charm_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Charm_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Charm_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedCharmSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Diplomat +1, Recruiting Character +1
        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void AssistedCharmFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void AssistedCharmCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void ThwartedCharmSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Recruiting Character +1
        _characterInvolved.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void ThwartedCharmFailRewardEffect(InteractionState state) {
        //**Level Up**: Instigator Minion +1
        investigatorMinion.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void ThwartedCharmCriticalFailRewardEffect(InteractionState state) {
        //**Level Up**: Instigator Minion +1
        investigatorMinion.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalCharmSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Recruiting Character +1
        _characterInvolved.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalCharmFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalCharmCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        character.faction.RemoveCharacter(character);
        faction.AddNewCharacter(character);
        character.homeLandmark.RemoveCharacterHomeOnLandmark(character);
        _characterInvolved.homeLandmark.AddCharacterHomeOnLandmark(character);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation.tileLocation.landmarkOnTile);
        character.SetForcedInteraction(interaction);
    }

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.isLeader && !currCharacter.isDefender && currCharacter.minion == null) { //- character must not be in Defender Tile.
                int weight = 0;
                if (currCharacter.faction == null || currCharacter.faction.id == FactionManager.Instance.neutralFaction.id) {
                    weight += 35; //- character is not part of any Faction: Weight +35
                } else if (currCharacter.faction.id != characterInvolved.faction.id) { //exclude characters with same faction
                    FactionRelationship rel = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                        weight += 15; //- character is part of a Faction with Neutral relationship with recruiter's Faction: Weight +15
                    } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight += 25; //- character is part of a Faction with Friend relationship with recruiter's Faction: Weight +25
                    }
                }

                if (currCharacter.level > characterInvolved.level) {
                    weight -= 30; //- character is higher level than Recruiter: Weight -30
                } else if (currCharacter.level < characterInvolved.level) { //- character is same level as Recruiter: Weight +0
                    weight += 10; //- character is lower level than Recruiter: Weight +10
                }

                weight = Mathf.Max(0, weight);
                characterWeights.AddElement(currCharacter, weight);
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
