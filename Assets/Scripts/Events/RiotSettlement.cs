using UnityEngine;
using System.Collections;

public class RiotSettlement : GameEvent {

    public RiotSettlement(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom) : base (startWeek, startMonth, startYear, startedBy){
        this.eventType = EVENT_TYPES.RIOTING_SETTLEMENTS;
        this.name = "Rioting Settlements and Governors";

        //A random city will lose 3 levels (down to a minimum of City Level 1) and lose its Governor
        City randomCity = sourceKingdom.cities[Random.Range(0, sourceKingdom.cities.Count)];
        for (int i = 0; i < 3; i++) {
            if(randomCity.structures.Count <= 0) {
                break;
            } else {
                HexTile tileToRemove = randomCity.structures[randomCity.structures.Count - 1];
                randomCity.RemoveTileFromCity(tileToRemove);
            }
        }
        Citizen governor = randomCity.governor;
        governor.Death(DEATH_REASONS.RIOT);

        //Stability will reset to 50.
        sourceKingdom.ChangeStability(50);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "RiotSettlement", "riot_settlement_start");
        newLog.AddToFillers(randomCity, randomCity.name, LOG_IDENTIFIER.CITY_1);
        newLog.AddToFillers(sourceKingdom, sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(governor, governor.name, LOG_IDENTIFIER.GOVERNOR_1);

        UIManager.Instance.ShowNotification(newLog);

        DoneEvent();
    }
}
