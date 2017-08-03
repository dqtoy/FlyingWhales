using UnityEngine;
using System.Collections;

public class KingdomDiscovery : PlayerEvent {
	
	private Kingdom discovererKingdom;
	private Kingdom discoveredKingdom;

	public KingdomDiscovery(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom discoveredKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = PLAYER_EVENT.KINGDOM_DISCOVERY;
		this.name = "Kingdom Discovery";
		this.discovererKingdom = this.startedByKingdom;
		this.discoveredKingdom = discoveredKingdom;

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "KingdomDiscovery", "event_title");
		newLogTitle.AddToFillers (this.discovererKingdom, this.discovererKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLogTitle.AddToFillers (this.discoveredKingdom, this.discoveredKingdom.name, LOG_IDENTIFIER.KINGDOM_2);


		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "KingdomDiscovery", "start");
		newLog.AddToFillers (this.discovererKingdom, this.discovererKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.discoveredKingdom, this.discoveredKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		this.PlayerEventIsCreated ();

		this.DoneEvent ();
	}
}
