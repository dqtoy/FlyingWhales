using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Persuade_Success = "Persuade Success";
    private const string Persuade_Character_Tortured_Died = "Persuade Character Tortured Died";
    private const string Persuade_Character_Tortured_Injured = "Persuade Character Tortured Injured";
    private const string Persuade_Character_Tortured_Recruited = "Persuade Character Tortured Recruited";
    private const string Persuade_Critical_Fail = "Persuade Critical Fail";
    private const string Character_Tortured_Died = "Character Tortured Died";
    private const string Character_Tortured_Injured = "Character Tortured Injured";
    private const string Character_Tortured_Recruited = "Character Tortured Recruited";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public TortureAction(Area interactable): base(interactable, INTERACTION_TYPE.TORTURE_ACTION, 0) {
        _name = "Torture Action";
        _categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT };
        _alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState persuadeSuccess = new InteractionState(Persuade_Success, this);
        InteractionState persuadeCharacterDied = new InteractionState(Persuade_Character_Tortured_Died, this);
        InteractionState persuadeCharacterInjured = new InteractionState(Persuade_Character_Tortured_Injured, this);
        InteractionState persuadeCharacterRecruited = new InteractionState(Persuade_Character_Tortured_Recruited, this);
        InteractionState persuadeCriticalFail = new InteractionState(Persuade_Critical_Fail, this);
        InteractionState characterDied = new InteractionState(Character_Tortured_Died, this);
        InteractionState characterInjured = new InteractionState(Character_Tortured_Injured, this);
        InteractionState characterRecruited = new InteractionState(Character_Tortured_Recruited, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);

        persuadeSuccess.SetEffect(() => PersuadeSuccessEffect(persuadeSuccess));
        persuadeCharacterDied.SetEffect(() => PersuadeCharacterDiedEffect(persuadeCharacterDied));
        persuadeCharacterInjured.SetEffect(() => PersuadeCharacterInjuredEffect(persuadeCharacterInjured));
        persuadeCharacterRecruited.SetEffect(() => PersuadeCharacterRecruitedEffect(persuadeCharacterRecruited));
        persuadeCriticalFail.SetEffect(() => PersuadeCritFailEffect(persuadeCriticalFail));

        characterDied.SetEffect(() => CharacterDiedEffect(characterDied));
        characterInjured.SetEffect(() => CharacterInjuredEffect(characterInjured));
        characterRecruited.SetEffect(() => CharacterRecruitedEffect(characterRecruited));

        _states.Add(startState.name, startState);

        _states.Add(persuadeSuccess.name, persuadeSuccess);
        _states.Add(persuadeCharacterDied.name, persuadeCharacterDied);
        _states.Add(persuadeCharacterInjured.name, persuadeCharacterInjured);
        _states.Add(persuadeCharacterRecruited.name, persuadeCharacterRecruited);
        _states.Add(persuadeCriticalFail.name, persuadeCriticalFail);

        _states.Add(characterDied.name, characterDied);
        _states.Add(characterInjured.name, characterInjured);
        _states.Add(characterRecruited.name, characterRecruited);

        SetCurrentState(startState);
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
        SetTargetCharacter(GetTargetCharacter(character));
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void PersuadeOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Persuade_Success, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorCharacter.job.GetFailRate());
        effectWeights.AddElement(Persuade_Critical_Fail, investigatorCharacter.job.GetCritFailRate());

        string result = effectWeights.PickRandomElementGivenWeights();
        if (result == "Fail") {
            effectWeights.Clear();
            effectWeights.AddElement(Persuade_Character_Tortured_Died, 10);
            effectWeights.AddElement(Persuade_Character_Tortured_Injured, 40);
            effectWeights.AddElement(Persuade_Character_Tortured_Recruited, 20);
            result = effectWeights.PickRandomElementGivenWeights();
        }
        SetCurrentState(_states[result]);
    }
    private void DoNothingOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Character_Tortured_Died, 10);
        effectWeights.AddElement(Character_Tortured_Injured, 40);
        effectWeights.AddElement(Character_Tortured_Recruited, 20);

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void PersuadeSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        investigatorCharacter.LevelUp();
    }
    private void PersuadeCharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void PersuadeCharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void PersuadeCharacterRecruitedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        Abducted abductedTrait = _targetCharacter.GetTrait("Abducted") as Abducted;
        _targetCharacter.RemoveTrait(abductedTrait);
        _targetCharacter.ChangeFactionTo(_characterInvolved.faction);
    }
    private void PersuadeCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));

        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _characterInvolved.faction, -1, state);

        _targetCharacter.Death();
        investigatorCharacter.Death();
    }
    private void CharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void CharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void CharacterRecruitedEffect(InteractionState state) {
        _characterInvolved.LevelUp();

        Abducted abductedTrait = _targetCharacter.GetTrait("Abducted") as Abducted;
        _targetCharacter.RemoveTrait(abductedTrait);
        _targetCharacter.ChangeFactionTo(_characterInvolved.faction);

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && currCharacter.GetTrait("Abducted") != null) {
                int weight = 0;
                if (currCharacter.faction == FactionManager.Instance.neutralFaction) {
                    weight += 80;
                } else {
                    if (currCharacter.faction != characterInvolved.faction) {
                        weight += 30;
                    } else {
                        weight += 15;
                    }
                }
                List<RelationshipTrait> relationships = characterInvolved.GetAllRelationshipTraitWith(currCharacter);
                for (int j = 0; j < relationships.Count; j++) {
                    if (relationships[j].effect == TRAIT_EFFECT.POSITIVE) {
                        weight -= 70;
                    } else {
                        weight += 30;
                    }
                }
                if (weight > 0) {
                    characterWeights.AddElement(currCharacter, weight);
                }
            }
        }
        if (characterWeights.Count > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
