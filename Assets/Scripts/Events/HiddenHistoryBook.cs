using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HiddenHistoryBook : GameEvent {
	private delegate void OnPerformAction();
	private OnPerformAction onPerformAction;

	internal Kingdom kingdom;

	private int daysCounter;

	public HiddenHistoryBook(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.HIDDEN_HISTORY_BOOK;
		this.name = "Hidden History Book";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.kingdom = startedBy.city.kingdom;
		this.daysCounter = 0;
		//TODO: Add log - event_title
		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "event_title");

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

		this.EventIsCreated();

		KnowsExistence();
	}

	#region Overrides
	internal override void PerformAction (){
		if(onPerformAction != null){
			onPerformAction ();
		}

	}

	internal override void DoneEvent (){
		base.DoneEvent ();
		if (this.startedBy.assignedRole != null){
			if (this.startedBy.assignedRole is King){
				((King)this.startedBy.assignedRole).isHiddenHistoryBooking = false;
			}
		}
		((King)this.kingdom.king.assignedRole).isHiddenHistoryBooking = false;

		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);

	}
	#endregion
	private void KnowsExistence(){
		//TODO: Add log - start
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "start");
		newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.kingdom, this.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		this.daysCounter = 0;
		onPerformAction += WaitDaysBeforeDecisionForSearchParty;
	}
	private void WaitDaysBeforeDecisionForSearchParty(){
		this.daysCounter += 1;
		if(this.daysCounter >= 6){
			DecisionForSearchParty();
		}
	}
	private void DecisionForSearchParty(){
		onPerformAction -= WaitDaysBeforeDecisionForSearchParty;
		if(this.kingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)){
			SearchParty();
		}else{
			int chance = UnityEngine.Random.Range(0,100);
			if (chance < 40){
				SearchParty();
			}else{
				//TODO: Add log - ignores the existence of the book and will not look for it
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "ignore_existence");
				newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);

				this.DoneEvent();
			}
		}
	}
	private void SearchParty(){
		//TODO: Add log - send search party
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "search_party");
		newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);

		this.daysCounter = 0;
		onPerformAction += WaitDaysBeforeCheckIfBookIsFound;

	}
	private void WaitDaysBeforeCheckIfBookIsFound(){
		this.daysCounter += 1;
		if(this.daysCounter >= 30){
			this.daysCounter = 0;
			CheckIfBookIsFound();
		}
	}
	private void CheckIfBookIsFound(){
		onPerformAction -= WaitDaysBeforeCheckIfBookIsFound;
		int chance = UnityEngine.Random.Range(0,100);
		if(chance < 75){
			//TODO: Add log - found book
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "book_found");

			KingDecision();
		}else{
			//TODO: Add log - book not found
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "book_not_found");
			newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);

			this.DoneEvent();
		}
	}
	private void KingDecision(){
		if(this.kingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)){
			UpholdOldWays();
		}else{
			int chance = UnityEngine.Random.Range(0,100);
			if (chance < 20){
				UpholdOldWays();
			}else{
				//TODO: Add log - ignores the old ways
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "ignore_old_ways");
				newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);

				this.DoneEvent();
			}
		}
	}

	private void UpholdOldWays(){
		//TODO: Add log - uphold old ways
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "HiddenHistoryBook", "uphold_old_ways");
		newLog.AddToFillers (this.kingdom.king, this.kingdom.king.name, LOG_IDENTIFIER.KING_1);

		GovernorReactions();
		OtherKingsReactions();

		this.DoneEvent();
	}

	private void GovernorReactions(){
		List<City> allCities = this.kingdom.nonRebellingCities;
		if(allCities != null && allCities.Count > 0){
			for (int i = 0; i < allCities.Count; i++) {
				if(allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)){
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(15, "Same tradition values", this);
				}else{
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(-15, "Opposing tradition values", this);
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
					if(otherKingdoms[i].king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)){
						relationship.AdjustLikeness(10, this);
					}else{
						relationship.AdjustLikeness(-10, this);
					}
				}
			}
		}
	}
}
