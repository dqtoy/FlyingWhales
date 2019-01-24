﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinateActionFaction : Interaction {

    private Character _targetCharacter;

    private const string Normal_Assassination_Success = "Normal Assassination Success";
    private const string Normal_Assassination_Fail = "Normal Assassination Fail";
    private const string Normal_Assassination_Critical_Fail = "Normal Assassination Critical Fail";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public AssassinateActionFaction(BaseLandmark interactable)
        : base(interactable, INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION, 0) {
        _name = "Assassinate Action Faction";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalRecruitmenSuccess = new InteractionState(Normal_Assassination_Success, this);
        InteractionState normalRecruitmentFail = new InteractionState(Normal_Assassination_Fail, this);
        InteractionState normalRecruitmentCriticalFail = new InteractionState(Normal_Assassination_Critical_Fail, this);

        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        normalRecruitmenSuccess.SetEffect(() => NormalAssassinationSuccessRewardEffect(normalRecruitmenSuccess));
        normalRecruitmentFail.SetEffect(() => NormalAssassinationFailRewardEffect(normalRecruitmentFail));
        normalRecruitmentCriticalFail.SetEffect(() => NormalAssassinationCriticalFailRewardEffect(normalRecruitmentCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(normalRecruitmenSuccess.name, normalRecruitmenSuccess);
        _states.Add(normalRecruitmentFail.name, normalRecruitmentFail);
        _states.Add(normalRecruitmentCriticalFail.name, normalRecruitmentCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
            return false;
        }
        if (_targetCharacter != null) { //if there is a target character, he/she must still be in this location
            return _targetCharacter.specificLocation.id == interactable.tileLocation.areaOfTile.id;
        } else { //if there is no set target character
            if (GetTargetCharacter(character) == null) { //check if a target character can be found using the provided weights
                return false;
            }
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Assassination_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Assassination_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Assassination_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void NormalAssassinationSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Target character dies
        targetCharacter.Death();
        //**Level Up**: Assassin Character +1
        _characterInvolved.LevelUp();
    }
    private void NormalAssassinationFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalAssassinationCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Assassin character dies
        _characterInvolved.Death();
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
        AddToDebugLog("Set target character to " + targetCharacter.name);
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        /*
         Once the actual action is triggered, the character will find a random non-Warded character in the location that is a member of an Enemy or War faction.
         */
        List<Character> choices = GetElligibleCharacters(interactable.tileLocation.areaOfTile);
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }

    private List<Character> GetElligibleCharacters(Area area) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < area.charactersAtLocation.Count; i++) {
            Character currCharacter = area.charactersAtLocation[i];
            if (currCharacter.GetTrait("Warded") == null
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.faction.id != characterInvolved.faction.id) {
                switch (currCharacter.faction.GetRelationshipWith(characterInvolved.faction).relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                        characters.Add(currCharacter);
                        break;

                }
            }
        }
        return characters;
    }
}