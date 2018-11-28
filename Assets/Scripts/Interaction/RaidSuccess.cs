using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidSuccess : Interaction {

    public RaidSuccess(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.RAID_SUCCESS, 0) {
        _name = "Raid Success";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] successfully raided [Location Name 1]. [He/She] returns with [Amount] Supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, otherData[0].ToString(), LOG_IDENTIFIER.STRING_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        startState.SetEffect(() => RaidSuccessEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    #endregion

    private void RaidSuccessEffect(InteractionState state) {
        //**Mechanics**: Favor Count -2
        interactable.faction.AdjustFavorFor(_characterInvolved.faction, -2);
    }
}
