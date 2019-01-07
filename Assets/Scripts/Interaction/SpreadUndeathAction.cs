using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadUndeathAction : Interaction {

    private Character targetCharacter;

    private const string Assisted_Conversion_Success = "Assisted Conversion Success";
    private const string Assisted_Conversion_Fail = "Assisted Conversion Fail";
    private const string Assisted_Conversion_Critical_Fail = "Assisted Conversion Critical Fail";

    private const string Thwarted_Conversion_Success = "Thwarted Conversion Success";
    private const string Thwarted_Conversion_Fail = "Thwarted Conversion Fail";
    private const string Thwarted_Conversion_Critical_Fail = "Thwarted Conversion Critical Fail";
    
    private const string Normal_Conversion_Success = "Normal Conversion Success";
    private const string Normal_Conversion_Fail = "Normal Conversion Fail";
    private const string Normal_Conversion_Critical_Fail = "Normal Conversion Critical Fail";

    public SpreadUndeathAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.SPREAD_UNDEATH_ACTION, 0) {
        _name = "Spread Undeath";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState thwartedCharmSuccess = new InteractionState(Thwarted_Conversion_Success, this);
        InteractionState thwartedCharmFail = new InteractionState(Thwarted_Conversion_Fail, this);
        InteractionState thwartedCharmCriticalFail = new InteractionState(Thwarted_Conversion_Critical_Fail, this);
        InteractionState assistedCharmSuccess = new InteractionState(Assisted_Conversion_Success, this);
        InteractionState assistedCharmFail = new InteractionState(Assisted_Conversion_Fail, this);
        InteractionState assistedCharmCriticalFail = new InteractionState(Assisted_Conversion_Critical_Fail, this);
        InteractionState normalCharmSuccess = new InteractionState(Normal_Conversion_Success, this);
        InteractionState normalCharmFail = new InteractionState(Normal_Conversion_Fail, this);
        InteractionState normalCharmCriticalFail = new InteractionState(Normal_Conversion_Critical_Fail, this);

        SetTargetCharacter(GetTargetCharacter(_characterInvolved));

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        thwartedCharmSuccess.SetEffect(() => ThwartedConversionSuccessRewardEffect(thwartedCharmSuccess));
        thwartedCharmFail.SetEffect(() => ThwartedConversionFailRewardEffect(thwartedCharmFail));
        thwartedCharmCriticalFail.SetEffect(() => ThwartedConversionCriticalFailRewardEffect(thwartedCharmCriticalFail));

        assistedCharmSuccess.SetEffect(() => AssistedConversionSuccessRewardEffect(assistedCharmSuccess));
        assistedCharmFail.SetEffect(() => AssistedConversionFailRewardEffect(assistedCharmFail));
        assistedCharmCriticalFail.SetEffect(() => AssistedConversionCriticalFailRewardEffect(assistedCharmCriticalFail));

        normalCharmSuccess.SetEffect(() => NormalConversionSuccessRewardEffect(normalCharmSuccess));
        normalCharmFail.SetEffect(() => NormalConversionFailRewardEffect(normalCharmFail));
        normalCharmCriticalFail.SetEffect(() => NormalConversionCriticalFailRewardEffect(normalCharmCriticalFail));

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
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetCharacter(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
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
                nextState = Thwarted_Conversion_Success;
                break;
            case RESULT.FAIL:
                nextState = Thwarted_Conversion_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Thwarted_Conversion_Critical_Fail;
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
                nextState = Assisted_Conversion_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Conversion_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Conversion_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Conversion_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Conversion_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Conversion_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedConversionSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));


        //**Mechanics**: Relationship between the two factions -1
        AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         * Change Character 2's race to Skeleton.
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Converter Character +1, Instigator Minion +1
        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();
    }
    private void AssistedConversionFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void AssistedConversionCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void ThwartedConversionSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Converter Character +1
        _characterInvolved.LevelUp();
    }
    private void ThwartedConversionFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Level Up**: Diplomat Minion +1
        investigatorMinion.LevelUp();
        //**Mechanics**: Player relationship with abductee's faction +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, targetCharacter.faction, 1, state);
    }
    private void ThwartedConversionCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Mechanics**: Player relationship with abductee's faction +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, targetCharacter.faction, 1, state);

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();

        //**Level Up**: Diplomat Minion +1
        investigatorMinion.LevelUp();
    }
    private void NormalConversionSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Relationship between the two factions -1
        AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Charmer Character +1
        _characterInvolved.LevelUp();
    }
    private void NormalConversionFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalConversionCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        //only add charmed trait to characters that have not been charmed yet, this is to retain it's original faction
        Charmed charmedTrait = new Charmed(character.faction);
        character.AddTrait(charmedTrait);
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
            if (currCharacter.id != characterInvolved.id 
                && !currCharacter.isLeader 
                && !currCharacter.isDefender 
                && currCharacter.minion == null 
                && currCharacter.faction.id != characterInvolved.faction.id) { //- character must not be in Defender Tile.
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 15; //character is not part of any Faction: Weight +15
                } else if (currCharacter.faction.id != characterInvolved.faction.id) { //exclude characters with same faction
                    //character is part of a Faction with Disliked, Neutral, Friend relationship with recruiter's Faction: Weight +25
                    FactionRelationship rel = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL 
                        || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED
                        || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight += 25; 
                    }
                }

                if (currCharacter.level > characterInvolved.level) {
                    weight -= 20; //character is higher level than Charmer: Weight -20
                } else if (currCharacter.level < characterInvolved.level) { //character is same level as Charmer: Weight +0
                    weight += 10; //character is lower level than Charmer: Weight +10
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
