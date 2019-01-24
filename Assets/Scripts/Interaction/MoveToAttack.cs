using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAttack : Interaction {

    private Area _target;
    private List<Character> _attackers;

    public MoveToAttack(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_ATTACK, 0) {
        _name = "Move To Attack";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetTargetAndAttackers(Area target, List<Character> attackers) {
        _target = target;
        _attackers = attackers;
    }

    #region Overrides
    public override void CreateStates() {
        if (_target != null && _attackers != null) {
            interactable.SetAttackTargetAndCharacters(_target, _attackers);
        }
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stopSuccessfulState = new InteractionState("Stop Successful", this);
        InteractionState stopFailState = new InteractionState("Stop Fail", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(interactable.attackTarget, interactable.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2);
        for (int i = 0; i < interactable.attackCharacters.Count; i++) {
            startStateDescriptionLog.AddToFillers(interactable.attackCharacters[i], interactable.attackCharacters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        CreateActionOptions(startState);

        stopSuccessfulState.SetEffect(() => StopSuccessfulEffect(stopSuccessfulState));
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(stopSuccessfulState.name, stopSuccessfulState);
        _states.Add(stopFailState.name, stopFailState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Stop them.",
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Minion must be Dissuader.",
                effect = () => StopOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        return interactable.owner != null;
    }
    //public override bool CanStillDoInteraction() {
    //    return interactable.owner != null;
    //}
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Stop Successful", investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Stop Fail", investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Do Nothing", 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Do Nothing"]);
    }
    #endregion

    #region State Effects
    private void StopSuccessfulEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        MinionSuccess();

        if (interactable.owner == null) {
            state.descriptionLog.AddToFillers(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1);
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        state.AddLogFiller(new LogFiller(interactable.attackTarget, interactable.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));

        interactable.SetAttackTargetAndCharacters(null, null);
    }
    private void StopFailEffect(InteractionState state) {
        if (interactable.owner == null) {
            state.descriptionLog.AddToFillers(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1);
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        state.AddLogFiller(new LogFiller(interactable.attackTarget, interactable.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));

        interactable.AttackTarget();
    }
    private void DoNothingEffect(InteractionState state) {
        if(interactable.owner == null) {
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        state.AddLogFiller(new LogFiller(interactable.attackTarget, interactable.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));
        for (int i = 0; i < interactable.attackCharacters.Count; i++) {
            state.AddLogFiller(new LogFiller(interactable.attackCharacters[i], interactable.attackCharacters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }

        interactable.AttackTarget();
    }
    #endregion
}
