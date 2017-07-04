using UnityEngine;
using System.Collections;

public class FirstAndKeystone : GameEvent {
	internal Kingdom firstOwner;
	internal Kingdom keystoneOwner;

	public FirstAndKeystone(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom firstOwner, Kingdom keystoneOwner) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.FIRST_AND_KEYSTONE;
		this.name = "The First and The Keystone";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.firstOwner = firstOwner;
		this.keystoneOwner = keystoneOwner;
	}

	#region Overrides

	#endregion
}
