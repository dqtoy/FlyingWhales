using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiplomatTargetLocation : Interaction {

    private const string Start = "Start";
    private const string Induce_Expansion = "Induce Expansion";
    private const string Do_Nothing = "Do Nothing";

    private LocationToken _targetLocationToken;
    private Character _expander;

    public DiplomatTargetLocation(Area interactable) : base(interactable, INTERACTION_TYPE.DIPLOMAT_TARGET_LOCATION, 0) {
        _name = "Diplomat Target Location";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        _targetLocationToken = _tokenTrigger as LocationToken;
        SetExpander();

        InteractionState startState = new InteractionState(Start, this);
        InteractionState induceExpansionState = new InteractionState(Induce_Expansion, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        induceExpansionState.SetEffect(() => InduceExpansionEffect(induceExpansionState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(induceExpansionState.name, induceExpansionState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption induceOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Induce an expansion to " + _targetLocationToken.nameInBold + ".",
                enabledTooltipText = "This location will send someone to occupy " + _targetLocationToken.location.name + ".",
                disabledTooltipText = interactable.owner.name + " cannot occupy " + _targetLocationToken.location.name + " due to racial incompatibility.",
                canBeDoneAction = () => CanInduceExpansion(),
                effect = () => InduceOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do Nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(induceOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private bool CanInduceExpansion() {
        if (_expander == null) {
            return false;
        }
        return true;
    }
    private void InduceOption(InteractionState state) {
        SetCurrentState(_states[Induce_Expansion]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InduceExpansionEffect(InteractionState state) {
        investigatorCharacter.LevelUp();

        MoveToExpand moveToExpand = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_EXPAND, interactable) as MoveToExpand;
        moveToExpand.SetTargetLocation(_targetLocationToken.location);
        _expander.InduceInteraction(moveToExpand);

        state.descriptionLog.AddToFillers(_expander, _expander.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_expander, _expander.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion

    private void SetExpander() {
        List<Character> expanders = new List<Character>();
        for (int i = 0; i < interactable.areaResidents.Count; i++) {
            Character resident = interactable.areaResidents[i];
            if (resident.forcedInteraction == null && resident.doNotDisturb <= 0 && resident.IsInOwnParty() && !resident.isLeader && !resident.isDefender && !resident.currentParty.icon.isTravelling && resident.specificLocation.id == interactable.id && _targetLocationToken.location.possibleOccupants.Contains(resident.race)) {
                expanders.Add(resident);
            }
        }
        if (expanders.Count > 0) {
            _expander = expanders[UnityEngine.Random.Range(0, expanders.Count)];
        }
    }
}
