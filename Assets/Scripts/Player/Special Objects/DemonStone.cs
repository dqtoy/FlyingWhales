using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonStone : SpecialObject {

    public GameDate dueDate { get; private set; }
    public GameDate startDate { get; private set; }
    public bool hasScheduledInvocation { get; private set; }

    public DemonStone() : base (SPECIAL_OBJECT_TYPE.DEMON_STONE){ }
    public DemonStone(SaveDataSpecialObject data) : base(data) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        //show begin summon UI
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You found a demon stone!", () => PlayerUI.Instance.ShowGeneralConfirmation("Demon Stone", "You've obtained a Demon Stone, you can expect a new minion after 24 hours", "Begin Invocation", ScheduleInvocation));
    }
    #endregion

    private void ScheduleInvocation() {
        startDate = GameManager.Instance.Today();
        dueDate = GameManager.Instance.Today().AddDays(1);
        hasScheduledInvocation = true;
        SchedulingManager.Instance.AddEntry(dueDate, OnInvocationDone, this);
        TimerHubUI.Instance.AddItem("Demon Invocation", GameManager.ticksPerDay, null);
    }

    private void OnInvocationDone() {
        Minion minion = PlayerManager.Instance.player.CreateNewMinionRandomClass();
        //PlayerManager.Instance.player.AddMinion(minion, true);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", () => PlayerManager.Instance.player.AddMinion(minion, true));
    }

    public void LoadInvocation(SaveDataDemonStone data) {
        hasScheduledInvocation = data.hasScheduledInvocation;
        if (hasScheduledInvocation) {
            startDate = new GameDate(startDate.month, startDate.day, startDate.year, startDate.tick);
            dueDate = new GameDate(dueDate.month, dueDate.day, dueDate.year, dueDate.tick);
            SchedulingManager.Instance.AddEntry(dueDate, OnInvocationDone, this);

            int ticksDiff = GameManager.Instance.GetTicksDifferenceOfTwoDates(dueDate, startDate);
            TimerHubUI.Instance.AddItem("Demon Invocation", ticksDiff, null);
        }
    }
}

public class SaveDataDemonStone : SaveDataSpecialObject {
    public int dueMonth;
    public int dueDay;
    public int dueYear;
    public int dueTick;

    public int startMonth;
    public int startDay;
    public int startYear;
    public int startTick;
    public bool hasScheduledInvocation;

    public override void Save(SpecialObject specialObject) {
        base.Save(specialObject);
        if (specialObject is DemonStone) {
            DemonStone demonStone = specialObject as DemonStone;
            dueMonth = demonStone.dueDate.month;
            dueDay = demonStone.dueDate.day;
            dueYear = demonStone.dueDate.year;
            dueTick = demonStone.dueDate.tick;

            startMonth = demonStone.startDate.month;
            startDay = demonStone.startDate.day;
            startYear = demonStone.startDate.year;
            startTick = demonStone.startDate.tick;

            hasScheduledInvocation = demonStone.hasScheduledInvocation;
        }
    }

    public override SpecialObject Load() {
        DemonStone demonStone = new DemonStone(this);
        demonStone.LoadInvocation(this);
        return demonStone;
    }
}
