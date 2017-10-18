using UnityEngine;
using System.Collections;

public class Regression : GameEvent {

    public Regression(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom) : base (startWeek, startMonth, startYear, startedBy){
        this.eventType = EVENT_TYPES.REGRESSION;
        this.name = "Regression";

        //The kingdom's tech level will be reduced by 1 (down to a minimum of Tech Level 0), Keep percent at which tech counter is at before degrading
        if (sourceKingdom.techLevel > 0) {
            float currTechCounterPercent = (float)sourceKingdom.techCounter / (float)sourceKingdom.techCapacity;
            sourceKingdom.DegradeTechLevel(1);
            sourceKingdom.AdjustTechCounter(Mathf.FloorToInt((float)sourceKingdom.techCapacity * currTechCounterPercent));
        } else {
            sourceKingdom.AdjustTechCounter(-sourceKingdom.techCapacity);
        }
        
        //Stability will reset to 50.
        sourceKingdom.ChangeStability(50);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Regression", "regression_start");
        newLog.AddToFillers(sourceKingdom, sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

        UIManager.Instance.ShowNotification(newLog);

        DoneEvent();
    }
}
