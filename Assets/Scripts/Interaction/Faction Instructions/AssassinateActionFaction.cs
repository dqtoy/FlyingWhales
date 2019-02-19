using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinateActionFaction : Interaction {

    private Character _targetCharacter;

    private const string Normal_Assassination_Success = "Normal Assassination Success";
    private const string Normal_Assassination_Fail = "Normal Assassination Fail";
    private const string Normal_Assassination_Critical_Fail = "Normal Assassination Critical Fail";
    private const string Target_Missing = "Target Missing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public AssassinateActionFaction(Area interactable)
        : base(interactable, INTERACTION_TYPE.ASSASSINATE_ACTION_FACTION, 0) {
        _name = "Assassinate Action Faction";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalAssassinationSuccess = new InteractionState(Normal_Assassination_Success, this);
        InteractionState normalAssassinationFail = new InteractionState(Normal_Assassination_Fail, this);
        InteractionState normalAssassinationCriticalFail = new InteractionState(Normal_Assassination_Critical_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        startState.SetEffect(() => StartEffect(startState), false);
        normalAssassinationSuccess.SetEffect(() => NormalAssassinationSuccessRewardEffect(normalAssassinationSuccess));
        normalAssassinationFail.SetEffect(() => NormalAssassinationFailRewardEffect(normalAssassinationFail));
        normalAssassinationCriticalFail.SetEffect(() => NormalAssassinationCriticalFailRewardEffect(normalAssassinationCriticalFail));
        targetMissing.SetEffect(() => TargetMissingRewardEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(normalAssassinationSuccess.name, normalAssassinationSuccess);
        _states.Add(normalAssassinationFail.name, normalAssassinationFail);
        _states.Add(normalAssassinationCriticalFail.name, normalAssassinationCriticalFail);
        _states.Add(targetMissing.name, targetMissing);

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
        Character target = GetTargetCharacter(character);
        if (target == null) { //check if a target character can be found using the provided weights
            return false;
        }
        SetTargetCharacter(target);
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        _targetStructure = _targetCharacter.currentStructure;
        AddToDebugLog("Set target character to " + targetCharacter.name);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (targetCharacter.currentStructure != _targetStructure) {
            nextState = Target_Missing;
        } else {
            WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
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
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(targetCharacter.currentStructure);
    }
    private void NormalAssassinationSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Target character dies
        targetCharacter.Death();
        //**Level Up**: Assassin Character +1
        //_characterInvolved.LevelUp();
    }
    private void NormalAssassinationFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalAssassinationCriticalFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Assassin character dies
        _characterInvolved.Death();
    }
    private void TargetMissingRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    public Character GetTargetCharacter(Character characterInvolved) {
        /*
         Once the actual action is triggered, the character will find a random non-Warded character in the location that is a member of an Enemy or War faction.
         */
        List<Character> choices = GetElligibleCharacters(interactable);
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
                && currCharacter.faction.id != characterInvolved.faction.id
                && currCharacter.currentStructure.isInside) {
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
