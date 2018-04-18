using UnityEngine;
using System.Collections;

public class Plague : GameEvent {

    internal string _plagueName;
    private Kingdom _infectedKingdom;
    private GameDate nextCureCheckDay;

	public Plague(int startDay, int startMonth, int startYear, Citizen startedBy, Kingdom infectedKingdom, bool isResetStability = true) : base(startDay, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.PLAGUE;
        _plagueName = Utilities.GeneratePlagueName();
        _infectedKingdom = infectedKingdom;

        nextCureCheckDay = new GameDate(startMonth, startDay, startYear);
        nextCureCheckDay.AddMonths(3);
        SchedulingManager.Instance.AddEntry(nextCureCheckDay, () => CheckIfPlagueIsCured());

        EventIsCreated(infectedKingdom, false);

		if(isResetStability){
			//Stability will reset to 50.
			infectedKingdom.ChangeStability(50);
		}

        Log newLog = CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_start");
        newLog.AddToFillers(null, _plagueName, LOG_IDENTIFIER.OTHER);
        newLog.AddToFillers(_infectedKingdom, _infectedKingdom.name, LOG_IDENTIFIER.FACTION_1);

        UIManager.Instance.ShowNotification(newLog);
    }

    private void CheckIfPlagueIsCured() {
        if (_infectedKingdom.isDead) {
            DoneEvent();
            return;
        }
        //Starting 3 months after the plague, every 5 days, there will be a 3% that the plague will be done
        if (Random.Range(0, 100) < 3) {
            //Plague is cured
            CurePlague();
        } else {
            //Plague is not cured, reschedule cure check
            nextCureCheckDay.AddDays(5);
            SchedulingManager.Instance.AddEntry(nextCureCheckDay, () => CheckIfPlagueIsCured());
        }
    }

    private void CurePlague() {
        Log newLog = CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_end");
        newLog.AddToFillers(_infectedKingdom, _infectedKingdom.name, LOG_IDENTIFIER.FACTION_1);
        newLog.AddToFillers(null, _plagueName, LOG_IDENTIFIER.OTHER);

        UIManager.Instance.ShowNotification(newLog);
        DoneEvent();
    }
}
