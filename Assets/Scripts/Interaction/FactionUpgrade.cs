using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionUpgrade : Interaction {
    private const string Start = "Start";
    private const string Stop_Faction_Upgrade_Successful = "Stop Faction Upgrade Successful";
    private const string Stop_Faction_Upgrade_Fail = "Stop Faction Upgrade Fail";
    private const string Disrupt_Faction_Upgrade_Successful = "Disrupt Faction Upgrade Successful";
    private const string Disrupt_Faction_Upgrade_Fail = "Disrupt Faction Upgrade Fail";
    private const string Assisted_Faction_Upgrade  = "Assisted Faction Upgrade";
    private const string Normal_Faction_Upgrade = "Normal Faction Upgrade";

    public FactionUpgrade(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FACTION_UPGRADE, 0) {
        _name = "Faction Upgrade";
        _jobFilter = new JOB[] { JOB.DISSUADER, JOB.DISSUADER, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState stopUpgradeSuccessState = new InteractionState(Stop_Faction_Upgrade_Successful, this);
        InteractionState stopUpgradeFailState = new InteractionState(Stop_Faction_Upgrade_Fail, this);
        InteractionState disruptUpgradeSuccessState = new InteractionState(Disrupt_Faction_Upgrade_Successful, this);
        InteractionState disruptUpgradeFailState = new InteractionState(Disrupt_Faction_Upgrade_Fail, this);
        InteractionState assistedFactionUpgradeState = new InteractionState(Assisted_Faction_Upgrade, this);
        InteractionState normalFactionUpgradeState = new InteractionState(Normal_Faction_Upgrade, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner.leader, interactable.tileLocation.areaOfTile.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        stopUpgradeSuccessState.SetEffect(() => StopFactionUpgradeSuccessEffect(stopUpgradeSuccessState));
        stopUpgradeFailState.SetEffect(() => StopFactionUpgradeFailEffect(stopUpgradeFailState));
        disruptUpgradeSuccessState.SetEffect(() => DisruptFactionUpgradeSuccessEffect(disruptUpgradeSuccessState));
        disruptUpgradeFailState.SetEffect(() => DisruptFactionUpgradeFailEffect(disruptUpgradeFailState));
        assistedFactionUpgradeState.SetEffect(() => AssistedFactionUpgradeEffect(assistedFactionUpgradeState));
        normalFactionUpgradeState.SetEffect(() => NormalFactionUpgradeEffect(normalFactionUpgradeState));

        _states.Add(startState.name, startState);
        _states.Add(stopUpgradeSuccessState.name, stopUpgradeSuccessState);
        _states.Add(stopUpgradeFailState.name, stopUpgradeFailState);
        _states.Add(disruptUpgradeSuccessState.name, disruptUpgradeSuccessState);
        _states.Add(disruptUpgradeFailState.name, disruptUpgradeFailState);
        _states.Add(assistedFactionUpgradeState.name, assistedFactionUpgradeState);
        _states.Add(normalFactionUpgradeState.name, normalFactionUpgradeState);

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
            ActionOption disruptOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Disrupt the plan.",
                duration = 0,
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be Instigator.",
                effect = () => DisruptOption(),
            };
            ActionOption assistOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 100, currency = CURRENCY.SUPPLY },
                name = "Provide assistance in both supplies and training.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be Diplomat.",
                effect = () => AssistOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(disruptOption);
            state.AddActionOption(assistOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Stop_Faction_Upgrade_Successful, investigatorMinion.character.job.GetSuccessRate());
        effectWeights.AddElement(Stop_Faction_Upgrade_Fail, investigatorMinion.character.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DisruptOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Disrupt_Faction_Upgrade_Successful, investigatorMinion.character.job.GetSuccessRate());
        effectWeights.AddElement(Disrupt_Faction_Upgrade_Fail, investigatorMinion.character.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void AssistOption() {
        SetCurrentState(_states[Assisted_Faction_Upgrade]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Normal_Faction_Upgrade, 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Normal_Faction_Upgrade]);
    }
    #endregion

    #region State Effects
    private void StopFactionUpgradeSuccessEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        MinionSuccess();

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner.leader, interactable.tileLocation.areaOfTile.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void StopFactionUpgradeFailEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.LevelUp();
        interactable.tileLocation.areaOfTile.owner.leader.LevelUp();

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner.leader, interactable.tileLocation.areaOfTile.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.owner.level.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void DisruptFactionUpgradeSuccessEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        interactable.tileLocation.areaOfTile.owner.LevelUp(-1);
        interactable.tileLocation.areaOfTile.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.owner.level.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void DisruptFactionUpgradeFailEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.leader.LevelUp();
        interactable.tileLocation.areaOfTile.owner.LevelUp();
        interactable.tileLocation.areaOfTile.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -1);

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.owner.level.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void AssistedFactionUpgradeEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        interactable.tileLocation.areaOfTile.owner.leader.LevelUp();
        interactable.tileLocation.areaOfTile.owner.LevelUp(2);
        interactable.tileLocation.areaOfTile.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 2);

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner.leader, interactable.tileLocation.areaOfTile.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.owner.level.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalFactionUpgradeEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.leader.LevelUp();
        interactable.tileLocation.areaOfTile.owner.LevelUp();

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner.leader, interactable.tileLocation.areaOfTile.owner.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.owner.level.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
