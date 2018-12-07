using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MinionFailed : Interaction {

    public MinionFailed(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MINION_FAILED, 70) {
        _name = "Minion Failed";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        if(explorerMinion.character.job.jobType == JOB.RAIDER) {
            Raider raider = explorerMinion.character.job as Raider;
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.character.job.jobType.ToString().ToLower() + "-" + raider.action.ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.character.job.jobType.ToString().ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }

        startState.SetEffect(() => StartEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        if (explorerMinion.character.job.jobType == JOB.RAIDER) {
            interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -1);
        }
    }
    #endregion
}
