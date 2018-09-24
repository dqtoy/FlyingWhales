using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAction : CharacterAction {

    protected const int baseCooldown = 1440;
    protected int cooldown;

    public TrainAction() : base(ACTION_TYPE.TRAIN) {
        cooldown = 0;
    }

    #region Overrides
    public override void PerformAction(NewParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //give exp per tick
        party.mainCharacter.AdjustExperience(_actionData.providedExp);
    }
    public override void DoneDuration(NewParty party, IObject targetObject) {
        ResetCooldown();
        ActionSuccess(targetObject);
    }
    public override CharacterAction Clone() {
        TrainAction trainAction = new TrainAction();
        SetCommonData(trainAction);
        trainAction.Initialize();
        return trainAction;
    }
    public override bool CanBeDone(IObject targetObject) {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    public override bool CanBeDoneBy(NewParty party, IObject targetObject) {
        if (cooldown != 0) {
            return false; //action has not yet cooled down
        }
        return true;
    }
    #endregion

    private void ResetCooldown() {
        cooldown = baseCooldown;
        Messenger.AddListener(Signals.HOUR_ENDED, Cooldown);
    }

    private void Cooldown() {
        cooldown--;
        if (cooldown <= 0) {
            cooldown = 0;
            Messenger.RemoveListener(Signals.HOUR_ENDED, Cooldown);
        }
    }
}
