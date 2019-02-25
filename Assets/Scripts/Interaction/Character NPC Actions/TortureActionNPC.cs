using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureActionNPC : Interaction {
    private const string Start = "Start";
    private const string Persuade_Success = "Persuade Success";
    private const string Persuade_Character_Tortured_Died = "Persuade Character Tortured Died";
    private const string Persuade_Character_Tortured_Injured = "Persuade Character Tortured Injured";
    private const string Persuade_Character_Tortured_Knocked_Out = "Persuade Character Tortured Knocked Out";
    private const string Persuade_Character_Tortured_Escapes = "Persuade Character Tortured Escapes";
    private const string Character_Tortured_Died = "Character Tortured Died";
    private const string Character_Tortured_Injured = "Character Tortured Injured";
    private const string Character_Tortured_Knocked_Out = "Character Tortured Knocked Out";
    private const string Character_Tortured_Escapes = "Character Tortured Escapes";

    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public TortureActionNPC(Area interactable): base(interactable, INTERACTION_TYPE.TORTURE_ACTION_NPC, 0) {
        _name = "Torture Action NPC";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState persuadeSuccess = new InteractionState(Persuade_Success, this);
        InteractionState persuadeCharacterDied = new InteractionState(Persuade_Character_Tortured_Died, this);
        InteractionState persuadeCharacterInjured = new InteractionState(Persuade_Character_Tortured_Injured, this);
        InteractionState persuadeCharacterKnockedOut = new InteractionState(Persuade_Character_Tortured_Knocked_Out, this);
        InteractionState persuadeCharacterEscapes = new InteractionState(Persuade_Character_Tortured_Escapes, this);
        InteractionState characterDied = new InteractionState(Character_Tortured_Died, this);
        InteractionState characterInjured = new InteractionState(Character_Tortured_Injured, this);
        InteractionState characterKnockedOut = new InteractionState(Character_Tortured_Knocked_Out, this);
        InteractionState characterEscapes = new InteractionState(Character_Tortured_Escapes, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);

        persuadeSuccess.SetEffect(() => PersuadeSuccessEffect(persuadeSuccess));
        persuadeCharacterDied.SetEffect(() => PersuadeCharacterDiedEffect(persuadeCharacterDied));
        persuadeCharacterInjured.SetEffect(() => PersuadeCharacterInjuredEffect(persuadeCharacterInjured));
        persuadeCharacterKnockedOut.SetEffect(() => PersuadeCharacterKnockedOutEffect(persuadeCharacterKnockedOut));
        persuadeCharacterEscapes.SetEffect(() => PersuadeCharacterEscapesEffect(persuadeCharacterEscapes));

        characterDied.SetEffect(() => CharacterDiedEffect(characterDied));
        characterInjured.SetEffect(() => CharacterInjuredEffect(characterInjured));
        characterKnockedOut.SetEffect(() => CharacterKnockedOutEffect(characterKnockedOut));
        characterEscapes.SetEffect(() => CharacterEscapesEffect(characterEscapes));

        _states.Add(startState.name, startState);

        _states.Add(persuadeSuccess.name, persuadeSuccess);
        _states.Add(persuadeCharacterDied.name, persuadeCharacterDied);
        _states.Add(persuadeCharacterInjured.name, persuadeCharacterInjured);
        _states.Add(persuadeCharacterKnockedOut.name, persuadeCharacterKnockedOut);
        _states.Add(persuadeCharacterEscapes.name, persuadeCharacterEscapes);

        _states.Add(characterDied.name, characterDied);
        _states.Add(characterInjured.name, characterInjured);
        _states.Add(characterKnockedOut.name, characterKnockedOut);
        _states.Add(characterEscapes.name, characterEscapes);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption persuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from torturing " + _targetCharacter.name + ".",
                effect = () => PersuadeOptionEffect(),
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Minion must be a Dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(persuade);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        _targetStructure = _targetCharacter.currentStructure;
        targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure);
    }
    #endregion

    #region Option Effect
    private void PersuadeOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Persuade_Success, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorCharacter.job.GetFailRate());
        effectWeights.AddElement(Persuade_Character_Tortured_Escapes, investigatorCharacter.job.GetCritFailRate());

        string result = effectWeights.PickRandomElementGivenWeights();
        if (result == "Fail") {
            effectWeights.Clear();
            effectWeights.AddElement(Persuade_Character_Tortured_Died, 10);
            effectWeights.AddElement(Persuade_Character_Tortured_Injured, 40);
            effectWeights.AddElement(Persuade_Character_Tortured_Knocked_Out, 20);
            effectWeights.AddElement(Persuade_Character_Tortured_Escapes, 5);
            result = effectWeights.PickRandomElementGivenWeights();
        }
        SetCurrentState(_states[result]);
    }
    private void DoNothingOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Character_Tortured_Died, 10);
        effectWeights.AddElement(Character_Tortured_Injured, 40);
        effectWeights.AddElement(Character_Tortured_Knocked_Out, 20);
        effectWeights.AddElement(Character_Tortured_Escapes, 5);

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure));
    }
    private void PersuadeSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //investigatorCharacter.LevelUp();
    }
    private void PersuadeCharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void PersuadeCharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Injured");
    }
    private void PersuadeCharacterKnockedOutEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //_characterInvolved.LevelUp();
        _targetCharacter.AddTrait("Unconscious");
    }
    private void PersuadeCharacterEscapesEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        Trait abductedTrait = _targetCharacter.GetTrait("Abducted");
        if(abductedTrait != null) {
            _targetCharacter.RemoveTrait(abductedTrait);
            Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _targetCharacter.specificLocation);
            _targetCharacter.InduceInteraction(returnHome);
        } else {
            Trait restrainedTrait = _targetCharacter.GetTrait("Restrained");
            if (restrainedTrait != null) {
                _targetCharacter.RemoveTrait(restrainedTrait);
                _targetCharacter.ChangeFactionTo(FactionManager.Instance.neutralFaction);
                Area newHomeArea = LandmarkManager.Instance.GetRandomUnownedAreaWithAvailableStructure(STRUCTURE_TYPE.DWELLING);
                if(newHomeArea != null) {
                    _targetCharacter.MigrateHomeTo(newHomeArea);
                    Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _targetCharacter.specificLocation);
                    _targetCharacter.InduceInteraction(returnHome);
                }
            }
        }
    }
    private void CharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void CharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Injured");
    }
    private void CharacterKnockedOutEffect(InteractionState state) {
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Unconscious");
    }
    private void CharacterEscapesEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        Trait abductedTrait = _targetCharacter.GetTrait("Abducted");
        if (abductedTrait != null) {
            _targetCharacter.RemoveTrait(abductedTrait);
            Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _targetCharacter.specificLocation);
            _targetCharacter.InduceInteraction(returnHome);
        } else {
            Trait restrainedTrait = _targetCharacter.GetTrait("Restrained");
            if (restrainedTrait != null) {
                _targetCharacter.RemoveTrait(restrainedTrait);
                _targetCharacter.ChangeFactionTo(FactionManager.Instance.neutralFaction);
                Area newHomeArea = LandmarkManager.Instance.GetRandomUnownedAreaWithAvailableStructure(STRUCTURE_TYPE.DWELLING);
                if (newHomeArea != null) {
                    _targetCharacter.MigrateHomeTo(newHomeArea);
                    Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _targetCharacter.specificLocation);
                    _targetCharacter.InduceInteraction(returnHome);
                }
            }
        }
    }
    #endregion
}
