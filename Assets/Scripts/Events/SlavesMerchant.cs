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

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		this.EventIsCreated();
	}

	#region Overrides
	internal override void PerformAction (){
		this.daysCounter += 1;
		if(this.daysCounter == 3){
			KingDecision();
		}else if(this.daysCounter == 10){
			if(this.hasBoughtSlaves){
				SlavesDistribution();
			}
		}
	}
	#endregion

	private void KingDecision(){
		int chance = UnityEngine.Random.Range(0,2);
		if(chance == 0 && this.buyerKingdom.nonRebellingCities.Count > 0){
			//Buy
			BuySlaves();
		}else{
			//No buy
			//TODO: Add log - king doesn't buy the slaves
		}
	}

	private void BuySlaves(){
		this.slavesQuantity = UnityEngine.Random.Range(100,201);
		this.hasBoughtSlaves = true;

		//TODO: Add log - king buys the slaves
	}

	private void SlavesDistribution(){
		List<City> allCities = this.buyerKingdom.nonRebellingCities;
		int slavesPerCity = this.slavesQuantity / allCities.Count;
		int unrestCount = 0;
		bool hasFreedSlaves = false;

		for (int i = 0; i < allCities.Count; i++) {
			if(allCities[i].governor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY)){
				//TODO: Add log - set slaves free by this governor
				hasFreedSlaves = true;
				((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Anti-slavery", this);
				unrestCount += 5;
			}else{
				//Add production rate by slavesPerCity
			}
		}

		this.buyerKingdom.AdjustUnrest(unrestCount);

		if(hasFreedSlaves){
			int chanceModifier = unrestCount / 5;
			RandomGovernorExecution(chanceModifier, allCities);
		}

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
			}
		}
	}
}
