using UnityEngine;
using System.Collections;

public class RestForDays : TaskAction {

    private int restDays;

    public RestForDays(OldQuest.Quest quest) : base(quest) {
    }

    #region overrides
    public override void InitializeAction(int days) {
        base.InitializeAction(days);
        restDays = days;
    }
    public override void ActionDone(TASK_ACTION_RESULT result) {
        _actionDoer.StopRegeneration();
        base.ActionDone(result);
    }
    #endregion

    internal void Rest() {
        actionDoer.StartRegeneration(2);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(restDays);
        SchedulingManager.Instance.AddEntry(dueDate, () => ActionDone(TASK_ACTION_RESULT.SUCCESS)); //End action after number of days
    }
}
