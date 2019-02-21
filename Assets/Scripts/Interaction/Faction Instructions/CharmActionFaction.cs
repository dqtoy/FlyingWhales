using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmActionFaction : Interaction {

    private Character _targetCharacter;

    private const string Normal_Charm_Success = "Normal Charm Success";
    private const string Normal_Charm_Fail = "Normal Charm Fail";
    private const string Normal_Charm_Critical_Fail = "Normal Charm Critical Fail";
    private const string Target_Missing = "Target Missing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public CharmActionFaction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.CHARM_ACTION_FACTION, 0) {
        _name = "Charm Action Faction";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalCharmSuccess = new InteractionState(Normal_Charm_Success, this);
        InteractionState normalCharmFail = new InteractionState(Normal_Charm_Fail, this);
        InteractionState normalCharmCriticalFail = new InteractionState(Normal_Charm_Critical_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        //SetTargetCharacter(GetTargetCharacter(_characterInvolved));

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);

        normalCharmSuccess.SetEffect(() => NormalCharmSuccessRewardEffect(normalCharmSuccess));
        normalCharmFail.SetEffect(() => NormalCharmFailRewardEffect(normalCharmFail));
        normalCharmCriticalFail.SetEffect(() => NormalCharmCriticalFailRewardEffect(normalCharmCriticalFail));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);

        _states.Add(normalCharmSuccess.name, normalCharmSuccess);
        _states.Add(normalCharmFail.name, normalCharmFail);
        _states.Add(normalCharmCriticalFail.name, normalCharmCriticalFail);
        _states.Add(targetMissing.name, targetMissing);

        //SetCurrentState(startState);
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
        Character targetCharacter = GetTargetCharacter(character);
        if (targetCharacter == null || character.homeArea.IsResidentsFull()) {
            return false;
        }
        SetTargetCharacter(targetCharacter);
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
        _targetStructure = targetCharacter.currentStructure;
        AddToDebugLog("Set " + targetCharacter.name + " as target");
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (_targetCharacter.currentStructure == _targetStructure) {
            WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
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
        } else {
            nextState = Target_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure));
    }
    private void NormalCharmSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(_targetCharacter, _characterInvolved.faction);
        //**Level Up**: Charmer Character +1
        //_characterInvolved.LevelUp();
    }
    private void NormalCharmFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalCharmCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        _characterInvolved.Death();
    }
    private void TargetMissingEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        AddToDebugLog("Transferring character " + character.name + " to " + faction.name);
        //if (character.isDefender) {
        //    AddToDebugLog("Target character is defender of " + character.defendingArea + ", removing him/her as defender...");
        //    character.defendingArea.GetFirstDefenderGroup().RemoveCharacterFromGroup(character);
        //}
        //only add charmed trait to characters that have not been charmed yet, this is to retain it's original faction
        Charmed charmedTrait = new Charmed(character.faction, character.homeArea);
        character.AddTrait(charmedTrait);
        character.faction.RemoveCharacter(character);
        faction.AddNewCharacter(character);
        AddToDebugLog("Successfully transferred " + character.name + " to " + character.faction.name);
        character.MigrateHomeTo(_characterInvolved.homeArea);
        //character.homeLandmark.RemoveCharacterHomeOnLandmark(character);
        //_characterInvolved.homeLandmark.AddCharacterHomeOnLandmark(character);
        AddToDebugLog("Set " + character.name + "'s home to " + character.homeArea.name);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation);
        character.SetForcedInteraction(interaction);
        AddToDebugLog("Forced " + character.name + " to go home");
    }

    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id 
                && !currCharacter.isLeader //- character must not be a Faction Leader
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.minion == null 
                && currCharacter.currentStructure.isInside
                && currCharacter.faction.id != characterInvolved.faction.id
                && !currCharacter.HasRelationshipOfEffectWith(characterInvolved, TRAIT_EFFECT.POSITIVE)) { //- character must not have any Positive relationship with Charmer
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 150; // character is not part of any Faction: Weight +150
                } else if (currCharacter.faction.id != characterInvolved.faction.id) { //exclude characters with same faction
                    //character is part of a Faction with Disliked, Neutral, Friend relationship with recruiter's Faction: Weight +25
                    FactionRelationship rel = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL 
                        || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED
                        || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        weight += 200; //character is part of an Enemy, Disliked or Neutral Faction: Weight +200
                    } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND
                        || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                        weight += 15; //- character is part of a Friend or Ally Faction: Weight +15
                    }
                }

                if (currCharacter.level > characterInvolved.level) {
                    weight -= 120; //character is higher level than Charmer: Weight -120
                } else if (currCharacter.level < characterInvolved.level) { //character is same level as Charmer: Weight +0
                    weight += 50; //character is lower level than Charmer: Weight +50
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
