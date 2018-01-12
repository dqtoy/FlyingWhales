using UnityEngine;
using System.Collections;

public class RestForDays : QuestAction {

    private int restDays;

    #region overrides
    public override void InititalizeAction(int days) {
        base.InititalizeAction(days);
        restDays = days;
    }
    public override void DoAction(Character actionDoer) {
        base.DoAction(actionDoer);
        actionDoer.StartRegeneration(2);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(restDays);
        SchedulingManager.Instance.AddEntry(dueDate, () => ActionDone()); //End action after number of days
        
    }
    public override void ActionDone() {
        actionDoer.StopRegeneration();
        base.ActionDone();
    }
    #endregion
}
