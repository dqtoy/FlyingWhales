using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SerumOfAlacrity : GameEvent {

	internal Kingdom kingdom;

	private int daysCounter;

	public SerumOfAlacrity(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SERUM_OF_ALACRITY;
		this.name = "Serum of Alacrity";
		this.durationInDays = UnityEngine.Random.Range(20,31);
		this.kingdom = startedBy.city.kingdom;
		this.daysCounter = 0;
		//TODO: Add log - event title
		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SerumOfAlacrity", "event_title");

		//TODO: Add log - start
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SerumOfAlacrity", "start");
		newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.kingdom, this.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		this.EventIsCreated();
	}
	#region Overrides
	internal override void PerformAction (){
		this.daysCounter += 1;
		if(this.daysCounter >= this.durationInDays){
			this.daysCounter = 0;
			DevelopedSerum();
		}

	}

	internal override void DoneEvent (){
		base.DoneEvent ();

		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);

	}
	#endregion

	private void DevelopedSerum(){
		int numberOfSerum = UnityEngine.Random.Range(5,11);
		this.kingdom.AdjustSerumOfAlacrity(numberOfSerum);

		GovernorReactions();
		OtherKingsReactions();

		//TODO: Add log - serum is developed
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SerumOfAlacrity", "develop_serum");
		newLog.AddToFillers (null, numberOfSerum.ToString(), LOG_IDENTIFIER.OTHER);

		this.DoneEvent();
	}

	private void GovernorReactions(){
		List<City> allCities = this.kingdom.nonRebellingCities;
		if(allCities != null && allCities.Count > 0){
			for (int i = 0; i < allCities.Count; i++) {
				if(allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)){
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(15, "Same strength values", this);
				}else{
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(-15, "Opposing strength values", this);
				}
			}
		}
	}

	private void OtherKingsReactions(){
		List<Kingdom> otherKingdoms = this.kingdom.discoveredKingdoms;
		if(otherKingdoms != null && otherKingdoms.Count > 0){
			for (int i = 0; i < otherKingdoms.Count; i++) {
				RelationshipKings relationship = otherKingdoms[i].king.GetRelationshipWithCitizen(this.kingdom.king);
				if(relationship != null){
					if(otherKingdoms[i].king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)){
						relationship.AdjustLikeness(10, this);
					}else{
						relationship.AdjustLikeness(-10, this);
					}
				}
			}
		}
	}
}