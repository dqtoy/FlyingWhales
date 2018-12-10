using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAttack : Interaction {

    public MoveToAttack(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_ATTACK, 0) {
        _name = "Move To Attack";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stopSuccessfulState = new InteractionState("Stop Successful", this);
        InteractionState stopFailState = new InteractionState("Stop Fail", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.attackTarget, interactable.tileLocation.areaOfTile.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2);
        for (int i = 0; i < interactable.tileLocation.areaOfTile.attackCharacters.Count; i++) {
            startStateDescriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.attackCharacters[i], interactable.tileLocation.areaOfTile.attackCharacters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
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
                jobNeeded = JOB.DISSUADER,
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
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Stop Successful", explorerMinion.character.job.GetSuccessRate());
        effectWeights.AddElement("Stop Fail", explorerMinion.character.job.GetFailRate());
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
        explorerMinion.LevelUp();
        MinionSuccess();

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.attackTarget, interactable.tileLocation.areaOfTile.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void StopFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.attackTarget, interactable.tileLocation.areaOfTile.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));

        interactable.tileLocation.areaOfTile.AttackTarget();
    }
    private void DoNothingEffect(InteractionState state) {
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.attackTarget, interactable.tileLocation.areaOfTile.attackTarget.name, LOG_IDENTIFIER.LANDMARK_2));

        interactable.tileLocation.areaOfTile.AttackTarget();
    }
    #endregion
}
