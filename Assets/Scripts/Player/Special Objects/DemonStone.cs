using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonStone : SpecialObject {

	public DemonStone() : base (SPECIAL_OBJECT_TYPE.DEMON_STONE){ }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        //show begin summon UI
        PlayerUI.Instance.ShowGeneralConfirmation("Demon Stone", "You've obtained a Demon Stone, you can expect a new minion after 24 hours", "Begin Invocation", ScheduleInvocation);
    }
    #endregion

    private void ScheduleInvocation() {
        GameDate due = GameManager.Instance.Today();
        due.AddDays(1);
        SchedulingManager.Instance.AddEntry(due, OnInvocationDone, this);
        TimerHubUI.Instance.AddItem("Demon Invocation", GameManager.ticksPerDay, null);
    }

    private void OnInvocationDone() {
        Minion minion = PlayerManager.Instance.player.CreateNewMinionRandomClass();
        PlayerManager.Instance.player.AddMinion(minion, true);
    }
}
