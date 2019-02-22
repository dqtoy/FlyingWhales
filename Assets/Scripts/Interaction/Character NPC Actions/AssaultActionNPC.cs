using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultActionNPC : Interaction {
    private const string Start = "Start";
    private const string Target_Injured = "Target Injured";
    private const string Target_Knocked_Out = "Target Knocked Out";
    private const string Target_Killed = "Target Killed";
    private const string Actor_Injured = "Actor Injured";
    private const string Actor_Knocked_Out = "Actor Knocked Out";
    private const string Actor_Killed = "Actor Killed";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public AssaultActionNPC(Area interactable): base(interactable, INTERACTION_TYPE.ASSAULT_ACTION_NPC, 0) {
        _name = "Assault Action NPC";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState targetInjured = new InteractionState(Target_Injured, this);
        InteractionState targetKnockedOut = new InteractionState(Target_Knocked_Out, this);
        InteractionState targetKilled = new InteractionState(Target_Killed, this);
        InteractionState actorInjured = new InteractionState(Actor_Injured, this);
        InteractionState actorKnockedOut = new InteractionState(Actor_Knocked_Out, this);
        InteractionState actorKilled = new InteractionState(Actor_Killed, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        targetInjured.SetEffect(() => TargetInjuredEffect(targetInjured));
        targetKnockedOut.SetEffect(() => TargetKnockedOutEffect(targetKnockedOut));
        targetKilled.SetEffect(() => TargetKilledEffect(targetKilled));
        actorInjured.SetEffect(() => ActorInjuredEffect(actorInjured));
        actorKnockedOut.SetEffect(() => ActorKnockedOutEffect(actorKnockedOut));
        actorKilled.SetEffect(() => ActorKilledEffect(actorKilled));

        _states.Add(startState.name, startState);
        _states.Add(targetInjured.name, targetInjured);
        _states.Add(targetKnockedOut.name, targetKnockedOut);
        _states.Add(targetKilled.name, targetKilled);
        _states.Add(actorInjured.name, actorInjured);
        _states.Add(actorKnockedOut.name, actorKnockedOut);
        _states.Add(actorKilled.name, actorKilled);

        //SetCurrentState(startState);
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
        if (_targetCharacter == null) {
            return false;
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

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Actor Win
            resultWeights.AddElement(Target_Injured, 30);
            resultWeights.AddElement(Target_Knocked_Out, 20);
            resultWeights.AddElement(Target_Killed, 5);
        } else {
            //Target Win
            resultWeights.AddElement(Actor_Injured, 30);
            resultWeights.AddElement(Actor_Knocked_Out, 20);
            resultWeights.AddElement(Actor_Killed, 5);
        }

        string result = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetCharacter.currentStructure));
    }
    private void TargetInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Injured");
    }
    private void TargetKnockedOutEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Unconscious");
    }
    private void TargetKilledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void ActorInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Injured");
    }
    private void ActorKnockedOutEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Unconscious");
    }
    private void ActorKilledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.Death();
    }
    #endregion
}
