using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseAction : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Curse_Successful = "Curse Successful";
    private const string Curser_Unconscious = "Curser Unconscious";
    private const string Curser_Injured = "Curser Injured";
    private const string Curser_Killed = "Curser Killed";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public CurseAction(Area interactable): base(interactable, INTERACTION_TYPE.CURSE_ACTION, 0) {
        _name = "Curse Action";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE };
        //_alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Override
    public override void CreateStates() {

        InteractionState startState = new InteractionState(Start, this);
        InteractionState curseSuccessful = new InteractionState(Curse_Successful, this);
        InteractionState curserUnconscious = new InteractionState(Curser_Unconscious, this);
        InteractionState curserInjured = new InteractionState(Curser_Injured, this);
        InteractionState curserKilled = new InteractionState(Curser_Killed, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        curseSuccessful.SetEffect(() => CurseSuccessfulEffect(curseSuccessful));
        curserUnconscious.SetEffect(() => CurserUnconsciousEffect(curserUnconscious));
        curserInjured.SetEffect(() => CurserInjuredEffect(curserInjured));
        curserKilled.SetEffect(() => CurserKilledEffect(curserKilled));

        _states.Add(startState.name, startState);
        _states.Add(curseSuccessful.name, curseSuccessful);
        _states.Add(curserUnconscious.name, curserUnconscious);
        _states.Add(curserInjured.name, curserInjured);
        _states.Add(curserKilled.name, curserKilled);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (_targetCharacter == null || _targetCharacter.currentParty.icon.isTravelling || _targetCharacter.isDead || _targetCharacter.specificLocation != character.specificLocation) {
            return false;
        }
        List<RelationshipTrait> relationships = character.GetAllRelationshipTraitWith(_targetCharacter);
        if (relationships == null || relationships.Count <= 0) {
            return false;
        } else {
            bool isEnemy = false;
            for (int i = 0; i < relationships.Count; i++) {
                if(relationships[i].relType == RELATIONSHIP_TRAIT.ENEMY) {
                    isEnemy = true;
                    break;
                }
            }
            if (!isEnemy) {
                return false;
            }
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(_targetCharacter);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        string nextState = string.Empty;
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Curser Win
            nextState = Curse_Successful;
        } else {
            //Target Win
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement(Curser_Unconscious, 20);
            resultWeights.AddElement(Curser_Injured, 20);
            resultWeights.AddElement(Curser_Killed, 5);
            nextState = resultWeights.PickRandomElementGivenWeights();
        }

        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void CurseSuccessfulEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Cursed"]);
    }
    private void CurserUnconsciousEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Unconscious"]);
    }
    private void CurserInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void CurserKilledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.Death();
    }
    #endregion
}
