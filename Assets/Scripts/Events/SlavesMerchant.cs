using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlavesMerchant : GameEvent {

	internal Kingdom buyerKingdom;

	private int daysCounter;
	private bool hasBoughtSlaves;
	private int slavesQuantity;

	public SlavesMerchant(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SLAVES_MERCHANT;
		this.name = "Slaves Merchant";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.buyerKingdom = startedBy.city.kingdom;
		this.daysCounter = 0;
		this.hasBoughtSlaves = false;
		this.slavesQuantity = 0;
		//TODO: Add log - event title
		//TODO: Add log - start
		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "event_title");

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "start");
		newLog.AddToFillers (this.buyerKingdom, this.buyerKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

        EventManager.Instance.AddEventToDictionary(this);
		Messenger.AddListener("OnDayEnd", this.PerformAction);
		this.EventIsCreated();
	}

	#region Overrides
	internal override void PerformAction (){
		this.daysCounter += 1;
		if(this.daysCounter == 7){
			KingDecision();
		}else if(this.daysCounter == 10){
			if(this.hasBoughtSlaves){
				SlavesDistribution();
			}
		}
	}

	internal override void DoneEvent (){
		base.DoneEvent ();
		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}
	#endregion

	private void KingDecision(){
		if((!this.buyerKingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) && !this.buyerKingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.EQUALITY)) && this.buyerKingdom.nonRebellingCities.Count > 0){
			//Buy
			BuySlaves();
		}else{
			//No buy
			//TODO: Add log - king doesn't buy the slaves
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "no_buy");
			newLog.AddToFillers (this.buyerKingdom.king, this.buyerKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.buyerKingdom, this.buyerKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

			this.DoneEvent();
		}
	}

	private void BuySlaves(){
		this.slavesQuantity = UnityEngine.Random.Range(50,101);
		this.hasBoughtSlaves = true;

		//TODO: Add log - king buys the slaves
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "buy");
		newLog.AddToFillers (this.buyerKingdom.king, this.buyerKingdom.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.buyerKingdom, this.buyerKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, this.slavesQuantity.ToString(), LOG_IDENTIFIER.OTHER);

	}

	private void SlavesDistribution(){
		Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "slaves_distribution");
		newLog2.AddToFillers (this.buyerKingdom.king, this.buyerKingdom.king.name, LOG_IDENTIFIER.KING_1);

		List<City> allCities = this.buyerKingdom.nonRebellingCities;
		int slavesPerCity = this.slavesQuantity / allCities.Count;
		int unrestCount = 0;
		bool hasFreedSlaves = false;
		bool hasObeyedButDecreaseLoyalty = false;

		for (int i = 0; i < allCities.Count; i++) {
			hasFreedSlaves = false;
			hasObeyedButDecreaseLoyalty = false;

			if (allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.LAW_AND_ORDER) && (allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) 
				|| allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.EQUALITY))) {

				if (allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.LIBERTY)
				   && allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.EQUALITY)) {
					if (allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LAW_AND_ORDER] > allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LIBERTY]
						&& allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LAW_AND_ORDER] > allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.EQUALITY]){
						//Obey but negative
						hasObeyedButDecreaseLoyalty = true;
					}else{
						//Free
						hasFreedSlaves = true;
					}
				}else if (allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.LIBERTY)) {
					if (allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LAW_AND_ORDER] > allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LIBERTY]){
						//Obey but negative
						hasObeyedButDecreaseLoyalty = true;
					}else{
						//Free
						hasFreedSlaves = true;
					}
				}else if (allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.EQUALITY)) {
					if (allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.LAW_AND_ORDER] > allCities [i].governor.importantCharacterValues [CHARACTER_VALUE.EQUALITY]){
						//Obey but negative
						hasObeyedButDecreaseLoyalty = true;
					}else{
						//Free
						hasFreedSlaves = true;
					}
				}

			}else{
				if(!allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.LAW_AND_ORDER) && (allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) 
					|| allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.EQUALITY))){
					//Free
					hasFreedSlaves = true;
				}
//				else if(allCities [i].governor.importantCharacterValues.ContainsKey (CHARACTER_VALUE.LAW_AND_ORDER) && (!allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) 
//					&& !allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.EQUALITY))){
//					//Obey
//				}else{
//					//Obey
//				}
			}
			if(hasFreedSlaves){
				((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Anti-slavery", this);
				unrestCount += 5;

				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "slaves_free");
				newLog.AddToFillers (allCities[i].governor, allCities[i].governor.name, LOG_IDENTIFIER.GOVERNOR_1);
				newLog.AddToFillers (allCities[i], allCities[i].name, LOG_IDENTIFIER.CITY_1);
			}else{
				if(hasObeyedButDecreaseLoyalty){
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Anti-slavery", this);
					unrestCount += 5;
					allCities[i].AdjustSlavesCount(slavesPerCity);

					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "obey_against_will");
					newLog.AddToFillers (allCities[i].governor, allCities[i].governor.name, LOG_IDENTIFIER.GOVERNOR_1);
					newLog.AddToFillers (allCities[i], allCities[i].name, LOG_IDENTIFIER.CITY_1);
				}else{
					allCities[i].AdjustSlavesCount(slavesPerCity);

					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "obey");
					newLog.AddToFillers (allCities[i].governor, allCities[i].governor.name, LOG_IDENTIFIER.GOVERNOR_1);
					newLog.AddToFillers (allCities[i], allCities[i].name, LOG_IDENTIFIER.CITY_1);
				}
			}
//			if(allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY)){
//				//TODO: Add log - set slaves free by this governor
//				hasFreedSlaves = true;
//				((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Anti-slavery", this);
//				unrestCount += 5;
//
//				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "slaves_free");
//				newLog.AddToFillers (allCities[i].governor, allCities[i].governor.name, LOG_IDENTIFIER.GOVERNOR_1);
//				newLog.AddToFillers (allCities[i], allCities[i].name, LOG_IDENTIFIER.CITY_1);
//
//			}else{
//				//Add production rate by slavesPerCity
//				allCities[i].AdjustSlavesCount(slavesPerCity);
//			}
		}

		this.buyerKingdom.AdjustUnrest(unrestCount);

		if(hasFreedSlaves){
			int chanceModifier = unrestCount / 5;
			RandomGovernorExecution(chanceModifier, allCities);
		}

		this.DoneEvent();
	}
	private void RandomGovernorExecution(int chanceModifier, List<City> allCities){
		int chance = UnityEngine.Random.Range(0,100);
		int value = 2 * chanceModifier;

		if(chance < value){
			List<Citizen> governorsForExecution = allCities.Select(x => x.governor).Where(y => y.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY)).ToList();
			if(governorsForExecution != null && governorsForExecution.Count > 0){
				Citizen chosenGovernor = governorsForExecution[UnityEngine.Random.Range(0, governorsForExecution.Count)];
				chosenGovernor.Death(DEATH_REASONS.TREACHERY);
				this.buyerKingdom.AdjustUnrest(20);

				//TODO: Add log - king has executed a governor
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SlavesMerchant", "governor_execution");
				newLog.AddToFillers (this.buyerKingdom.king, this.buyerKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (chosenGovernor, chosenGovernor.name, LOG_IDENTIFIER.GOVERNOR_1);
				newLog.AddToFillers (chosenGovernor.city, chosenGovernor.city.name, LOG_IDENTIFIER.CITY_1);
			}
		}
	}
}
