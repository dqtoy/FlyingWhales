using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Riot : GameEvent {


	public Riot(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RIOT;
		this.name = "Riot";

        //The kingdom will lose 25% of its Weapons and 25% of its Armors.
        int weaponLoss = Mathf.FloorToInt((float)sourceKingdom.baseWeapons * 0.25f);
        int armorLoss = Mathf.FloorToInt((float)sourceKingdom.baseArmor * 0.25f);

        sourceKingdom.AdjustBaseWeapons(-weaponLoss);
        sourceKingdom.AdjustBaseArmors(-armorLoss);

        //Stability will reset to 50.
        sourceKingdom.ChangeStability(50);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Riot", "riot_start");
        newLog.AddToFillers(sourceKingdom, sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(null, weaponLoss.ToString(), LOG_IDENTIFIER.OTHER);
        newLog.AddToFillers(null, armorLoss.ToString(), LOG_IDENTIFIER.WAR_NAME); //TODO: Change this to another identifier once a more appropriate one is available

        UIManager.Instance.ShowNotification(newLog);

        DoneEvent();
    }


}
