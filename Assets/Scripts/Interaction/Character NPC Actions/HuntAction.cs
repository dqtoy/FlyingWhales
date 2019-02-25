using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntAction : Interaction {
    private const string Start = "Start";
    private const string Hunter_Killed_Character = "Hunter Killed Character";
    private const string Hunter_Injured_Character = "Hunter Injured Character";
    private const string Character_Killed_Hunter = "Character Killed Hunter";
    private const string Character_Injured_Hunter = "Character Injured Hunter";

    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public HuntAction(Area interactable) : base(interactable, INTERACTION_TYPE.HUNT_ACTION, 0) {
        _name = "Hunt Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState hunterKilledCharacter = new InteractionState(Hunter_Killed_Character, this);
        InteractionState hunterInjuredCharacter = new InteractionState(Hunter_Injured_Character, this);
        InteractionState characterKilledHunter = new InteractionState(Character_Killed_Hunter, this);
        InteractionState characterInjuredHunter = new InteractionState(Character_Injured_Hunter, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        hunterKilledCharacter.SetEffect(() => HunterKilledCharacterEffect(hunterKilledCharacter));
        hunterInjuredCharacter.SetEffect(() => HunterInjuredCharacterEffect(hunterInjuredCharacter));
        characterKilledHunter.SetEffect(() => CharacterKilledHunterEffect(characterKilledHunter));
        characterInjuredHunter.SetEffect(() => CharacterInjuredHunterEffect(characterInjuredHunter));

        _states.Add(startState.name, startState);
        _states.Add(hunterKilledCharacter.name, hunterKilledCharacter);
        _states.Add(hunterInjuredCharacter.name, hunterInjuredCharacter);
        _states.Add(characterKilledHunter.name, characterKilledHunter);
        _states.Add(characterInjuredHunter.name, characterInjuredHunter);

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
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(character));
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        if (_targetCharacter != null) {
            //Debug.LogWarning("CHOSEN TARGET CHARACTER FOR HUNT ACTION OF " + _characterInvolved.name + " IS " + _targetCharacter.name);
            _targetStructure = _targetCharacter.currentStructure;
            targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure);
        }
    }
    public override bool CanStillDoInteraction(Character character) {
        if (base.CanStillDoInteraction(character)) {
            if (_targetCharacter.faction.id == character.faction.id && !character.isFactionless) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(_targetCharacter);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Hunter Win
            resultWeights.AddElement(Hunter_Killed_Character, 20);
            resultWeights.AddElement(Hunter_Injured_Character, 40);
        } else {
            //Target Win
            resultWeights.AddElement(Character_Killed_Hunter, 20);
            resultWeights.AddElement(Character_Injured_Hunter, 40);
        }

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure));
    }
    private void HunterKilledCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //_characterInvolved.LevelUp();

        if (!_targetCharacter.isFactionless && !_characterInvolved.isFactionless) {
            //Debug.LogError(_targetCharacter.name + " of " + _targetCharacter.faction.name + " will adjust faction relationship with " + _characterInvolved.name + " of " + _characterInvolved.faction.name);
            AdjustFactionsRelationship(_targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _targetCharacter.Death();
        _characterInvolved.ResetFullnessMeter();
    }
    private void HunterInjuredCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //_characterInvolved.LevelUp();
        _targetCharacter.AddTrait("Injured");
    }
    private void CharacterKilledHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //_targetCharacter.LevelUp();

        if (!_targetCharacter.isFactionless && !_characterInvolved.isFactionless) {
            //Debug.LogError(_targetCharacter.name + " of " + _targetCharacter.faction.name + " will adjust faction relationship with " + _characterInvolved.name + " of " + _characterInvolved.faction.name);
            AdjustFactionsRelationship(_targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.Death();
    }
    private void CharacterInjuredHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //_targetCharacter.LevelUp();
        _characterInvolved.AddTrait("Injured");
    }
    #endregion

    public Character GetTargetCharacter(Character characterInvolved) {
        if(characterInvolved.role.roleType == CHARACTER_ROLE.BEAST) {
            return null;
        }
        List<Character> characterChoices = new List<Character>();
        for (int i = 0; i < characterInvolved.specificLocation.charactersAtLocation.Count; i++) {
            Character currCharacter = characterInvolved.specificLocation.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && !currCharacter.isLeader
                && currCharacter.role.roleType == CHARACTER_ROLE.BEAST && currCharacter.isFactionless) {
                characterChoices.Add(currCharacter);
            }
        }
        if (characterChoices.Count > 0) {
            return characterChoices[UnityEngine.Random.Range(0, characterChoices.Count)];
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
