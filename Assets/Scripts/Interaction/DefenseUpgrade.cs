using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseUpgrade : Interaction {
    private const string Start = "Start";
    private const string Stop_Defense_Upgrade_Successful = "Stop Defense Upgrade Successful";
    private const string Stop_Defense_Upgrade_Fail = "Stop Defense Upgrade Fail";
    private const string Defender_Group_Upgraded = "Defender Group Upgraded";

    public DefenseUpgrade(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DEFENSE_UPGRADE, 0) {
        _name = "Defense Upgrade";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState stopUpgradeSuccessState = new InteractionState(Stop_Defense_Upgrade_Successful, this);
        InteractionState stopUpgradeFailState = new InteractionState(Stop_Defense_Upgrade_Fail, this);
        InteractionState defenderGroupUpgradeState = new InteractionState(Defender_Group_Upgraded, this);

        CreateActionOptions(startState);

        stopUpgradeSuccessState.SetEffect(() => StopDefenseUpgradeSuccessEffect(stopUpgradeSuccessState));
        stopUpgradeFailState.SetEffect(() => StopDefenseUpgradeFailEffect(stopUpgradeFailState));
        defenderGroupUpgradeState.SetEffect(() => DefenseGroupsUpgradedEffect(defenderGroupUpgradeState));

        _states.Add(startState.name, startState);
        _states.Add(stopUpgradeSuccessState.name, stopUpgradeSuccessState);
        _states.Add(stopUpgradeFailState.name, stopUpgradeFailState);
        _states.Add(defenderGroupUpgradeState.name, defenderGroupUpgradeState);

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
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Stop_Defense_Upgrade_Successful, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Stop_Defense_Upgrade_Fail, investigatorCharacter.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Defender_Group_Upgraded, 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Defender_Group_Upgraded]);
    }
    #endregion

    #region State Effects
    private void StopDefenseUpgradeSuccessEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void StopDefenseUpgradeFailEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.UpgradeDefendersToMatchFactionLvl();
    }
    private void DefenseGroupsUpgradedEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.UpgradeDefendersToMatchFactionLvl();
    }
    #endregion
}
