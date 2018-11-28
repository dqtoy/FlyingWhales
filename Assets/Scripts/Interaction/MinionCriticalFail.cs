using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class MinionCriticalFail : Interaction {

    public MinionCriticalFail(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MINION_CRITICAL_FAIL, 70) {
        _name = "Minion Critical Fail";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        if (explorerMinion.icharacter.characterClass.jobType == JOB.RAIDER) {
            Character character = explorerMinion.icharacter as Character;
            Raider raider = character.job as Raider;
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.icharacter.characterClass.jobType.ToString().ToLower() + "-" + raider.action.ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-" + explorerMinion.icharacter.characterClass.jobType.ToString().ToLower() + "_description");
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }

        startState.SetEffect(() => StartEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        if (explorerMinion.icharacter.characterClass.jobType == JOB.RAIDER) {
            interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -1);
        }
        DemonDisappearsRewardEffect(state);
    }
    #endregion
}
