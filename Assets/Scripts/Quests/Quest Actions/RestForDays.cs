using UnityEngine;
using System.Collections;

public class RestForDays : QuestAction {

    private int restDays;

    public RestForDays(Quest quest) : base(quest) {
    }

    #region overrides
    public override void InititalizeAction(int days) {
        base.InititalizeAction(days);
        restDays = days;
    }
    public override void ActionDone(QUEST_ACTION_RESULT result) {
        _actionDoer.StopRegeneration();
        base.ActionDone(result);
    }
    #endregion

    internal void Rest() {
        actionDoer.StartRegeneration(2);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(restDays);
        SchedulingManager.Instance.AddEntry(dueDate, () => ActionDone(QUEST_ACTION_RESULT.SUCCESS)); //End action after number of days
    }
}
