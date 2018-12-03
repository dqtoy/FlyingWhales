using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicPortal : StructureObj {

    public DemonicPortal() : base() {
        _specificObjectType = LANDMARK_TYPE.DEMONIC_PORTAL;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 50, currency = CURRENCY.MANA };
    }

    #region Overrides
    public override IObject Clone() {
        DemonicPortal clone = new DemonicPortal();
        SetCommonData(clone);
        return clone;
    }
    public override void StartDayAction() {
        base.StartDayAction();

    }
    //public override void StartState(ObjectState state) {
    //    base.StartState(state);
    //    if (state.stateName == "Preparing") {
    //        ScheduleDoneTraining();
    //    }
    //}
    #endregion

    #region Utilities
    private void SummonNewMinion() {
        if (!isRuined && PlayerManager.Instance.player.currencies[_effectCost.currency] >= _effectCost.amount) {
            ApplyEffectCostToPlayer();
            Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false);
            PlayerManager.Instance.player.AddMinion(newMinion);
        }
    }
    private void ScheduleDoneTraining() {
        GameDate readyDate = GameManager.Instance.Today();
        readyDate.AddDays(10); // 1 week
        SchedulingManager.Instance.AddEntry(readyDate, DoneTraining);
    }
    private void DoneTraining() {
        if (_currentState.stateName == "Preparing") {
            ObjectState readyState = GetState("Ready");
            ChangeState(readyState);
        }
    }
    public void CommenceTraining() {
        if (_currentState.stateName == "Ready") {
            ObjectState trainingState = GetState("Preparing");
            ChangeState(trainingState);
        }
    }
    #endregion
}
