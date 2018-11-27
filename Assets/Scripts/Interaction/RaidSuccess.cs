using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidSuccess : Interaction {

    public RaidSuccess(IInteractable interactable) : base(interactable, INTERACTION_TYPE.RAID_SUCCESS, 0) {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] successfully raided [Location Name 1]. [He/She] returns with [Amount] Supplies.
        startState.SetEndEffect(() => StartStateRewardEffect(startState));
    }

    protected void StartStateRewardEffect(InteractionState state) { }
}
