using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MinionCriticalFail : Interaction {

    public MinionCriticalFail(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MINION_CRITICAL_FAIL, 70) {
        _name = "Minion Critical Fail";
    }

    #region Overrides
    public override void Initialize() {
        SetExplorerMinion(interactable.tileLocation.areaOfTile.areaInvestigation.assignedMinion);
        base.Initialize();
    }
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        if (explorerMinion.character.characterClass.jobType == JOB.RAIDER) {
            Raider raider = explorerMinion.character.job as Raider;
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.character.characterClass.jobType.ToString().ToLower() + "-" + raider.action.ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.character.characterClass.jobType.ToString().ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }

        startState.SetEffect(() => StartEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        if (explorerMinion.character.characterClass.jobType == JOB.RAIDER) {
            interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -1);
        }
        DemonDisappearsRewardEffect(state);
    }
    #endregion
}
