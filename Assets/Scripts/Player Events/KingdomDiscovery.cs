using UnityEngine;
using System.Collections;

public class KingdomDiscovery : GameEvent {
	
	private Kingdom discovererKingdom;
	private Kingdom discoveredKingdom;

	public KingdomDiscovery(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom discoveredKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_DISCOVERY;
		this.name = "Kingdom Discovery";
		this.isOneTime = true;
		this.discovererKingdom = this.startedByKingdom;
		this.discoveredKingdom = discoveredKingdom;
//		this.affectedKingdoms.Add (this.discovererKingdom);
//		this.affectedKingdoms.Add (this.discoveredKingdom);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "KingdomDiscovery", "event_title");
		newLogTitle.AddToFillers (this.discovererKingdom, this.discovererKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLogTitle.AddToFillers (this.discoveredKingdom, this.discoveredKingdom.name, LOG_IDENTIFIER.KINGDOM_2);


		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "KingdomDiscovery", "start");
		newLog.AddToFillers (this.discovererKingdom, this.discovererKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.discoveredKingdom, this.discoveredKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

        //		this.PlayerEventIsCreated ();
        //EventIsCreated(this.discovererKingdom, true);
        //EventIsCreated(this.discoveredKingdom, true);
        //if (UIManager.Instance.currentlyShowingKingdom != null && (UIManager.Instance.currentlyShowingKingdom.id == this.discovererKingdom.id
        //    || UIManager.Instance.currentlyShowingKingdom.id == this.discoveredKingdom.id)) {
            UIManager.Instance.ShowNotification(newLog, new System.Collections.Generic.HashSet<Kingdom> { discovererKingdom, discoveredKingdom } );
        //}

        this.DoneEvent ();
	}
}
