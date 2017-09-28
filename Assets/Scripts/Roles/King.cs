using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class King : Role {

	public Kingdom ownedKingdom;
	internal int abductionCounter;

	internal bool isRumoring;
	internal bool isHiddenHistoryBooking;
	internal bool hasFoundHiddenHistoryBook;

	private int _triggerMonthOfSerum;
	private int _triggerDayOfSerum;
	private int _triggerYearOfSerum;

	public King(Citizen citizen): base(citizen){
		this.citizen.isKing = true;
//		if(this.citizen.city.kingdom.king != null){
//			this.citizen.CopyCampaignManager (this.citizen.city.kingdom.king.campaignManager);
//		}
		this.citizen.city.kingdom.king = this.citizen;
		this.SetOwnedKingdom(this.citizen.city.kingdom);
		this.citizen.GenerateCharacterValues ();
		//PrestigeContribution (false);
		HappinessContribution (false);
		//IntelligenceContribution (false);
		this.abductionCounter = 0;
		if(this.citizen.city.kingdom.plague != null){
			this.citizen.city.kingdom.plague.UpdateApproach (this.citizen.city.kingdom);
		}
		this.isRumoring = false;
		this.isHiddenHistoryBooking = false;
//		RandomTriggerDateOfSerum(true);
//		Messenger.AddListener("OnDayEnd", EverydayActions);
	}
	internal override void OnDeath (){
		base.OnDeath ();
		//PrestigeContribution (true);
		HappinessContribution (true);
		//IntelligenceContribution (true);
//		Messenger.RemoveListener("OnDayEnd", EverydayActions);

	}
	internal void SetOwnedKingdom(Kingdom ownedKingdom){
		this.ownedKingdom = ownedKingdom;
	}

	//private void PrestigeContribution(bool isRemove){
	//	int contribution = 0;
	//	switch(this.citizen.charismaLevel){
	//	case CHARISMA.HIGH:
	//		contribution = 10;
	//		break;
	//	case CHARISMA.AVERAGE:
	//		contribution = 7;
	//		break;
	//	case CHARISMA.LOW:
	//		contribution = 5;
	//		break;
	//	}
	//	if(isRemove){
	//		contribution *= -1;
	//	}
	//	this.ownedKingdom.AdjustBonusPrestige (contribution);

	//}
	private void HappinessContribution(bool isRemove){
		int contribution = 0;
		switch(this.citizen.efficiencyLevel){
		case EFFICIENCY.HIGH:
			contribution = 6;
			break;
		case EFFICIENCY.AVERAGE:
			contribution = 4;
			break;
		case EFFICIENCY.LOW:
			contribution = 2;
			break;
		}
		if(isRemove){
			contribution *= -1;
		}
		this.citizen.city.AdjustBonusHappiness (contribution);
	}
	//private void IntelligenceContribution(bool isRemove){
	//	int contribution = 0;
	//	switch(this.citizen.intelligenceLevel){
	//	case INTELLIGENCE.HIGH:
	//		contribution = 5;
	//		break;
	//	case INTELLIGENCE.AVERAGE:
	//		contribution = 3;
	//		break;
	//	case INTELLIGENCE.LOW:
	//		contribution = 2;
	//		break;
	//	}
	//	if(isRemove){
	//		contribution *= -1;
	//	}
	//	this.ownedKingdom.AdjustBonusTech (contribution);
	//}
	private void EverydayActions(){
		TriggerSpouseAbduction();
		TriggerRumor();
		TriggerHiddenHistoryBook();
		TriggerSerumOfAlacrity();
	}
	private void TriggerRumor(){
		if(GameManager.Instance.days % 10 == 0 && !this.isRumoring){
			int chance = UnityEngine.Random.Range(0, 100);
			if(chance < 20){
				if(this.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE) && this.citizen.city.kingdom.discoveredKingdoms.Count >= 2){
					List<Kingdom> targetKingdoms = new List<Kingdom>();
					List<Kingdom> rumorKingdoms = new List<Kingdom>();
					for (int i = 0; i < this.citizen.city.kingdom.discoveredKingdoms.Count; i++) {
						if(this.citizen.city.kingdom.discoveredKingdoms[i].isAlive()){
							KingdomRelationship relationship = this.citizen.city.kingdom.GetRelationshipWithKingdom(this.citizen.city.kingdom.discoveredKingdoms[i]);
							if(relationship != null && (relationship.relationshipStatus == RELATIONSHIP_STATUS.DISLIKE || relationship.relationshipStatus == RELATIONSHIP_STATUS.HATE || relationship.relationshipStatus == RELATIONSHIP_STATUS.SPITE)){
								targetKingdoms.Add(this.citizen.city.kingdom.discoveredKingdoms[i]);
							}
							if(relationship != null && (relationship.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE || relationship.relationshipStatus == RELATIONSHIP_STATUS.LOVE)){
								rumorKingdoms.Add(this.citizen.city.kingdom.discoveredKingdoms[i]);
							}
						}
					}
					if(targetKingdoms.Count > 0 && rumorKingdoms.Count > 0){
						Kingdom targetKingdom = targetKingdoms[UnityEngine.Random.Range(0,targetKingdoms.Count)];
						Kingdom rumorKingdom = rumorKingdoms[UnityEngine.Random.Range(0,rumorKingdoms.Count)];

						this.isRumoring = true;
						EventCreator.Instance.CreateRumorEvent(this.citizen, rumorKingdom, targetKingdom);
					}
				}
			}
		}
	}
	internal void TriggerSpouseAbduction(){
		if(!this.citizen.isDead){
			if((MONTH)GameManager.Instance.month == this.citizen.birthMonth && GameManager.Instance.days == this.citizen.birthDay && GameManager.Instance.year > this.citizen.birthYear){
				this.abductionCounter += 1;
				int chance = UnityEngine.Random.Range (0, 100);
				if(chance < 100 * this.abductionCounter){
					Citizen targetKing = null;
					if(IsReadyForAbduction(ref targetKing) && IsEligibleForAbduction(this.citizen.city.kingdom, targetKing.city.kingdom)){
						EventCreator.Instance.CreateSpouseAbductionEvent (this.citizen, targetKing);
					}
				}
			}
		}
	}
	internal void TriggerHiddenHistoryBook(){
		if(this.ownedKingdom.race == RACE.HUMANS){
			if(GameManager.Instance.days == 3 && !this.isHiddenHistoryBooking && !this.hasFoundHiddenHistoryBook && !this.ownedKingdom.hasUpheldHiddenHistoryBook){
				int chance = UnityEngine.Random.Range(0,100);
				if(chance < 3){
					this.isHiddenHistoryBooking = true;
					EventCreator.Instance.CreateHiddenHistoryBookEvent(this.citizen);
				}
			}
		}
	}
	private void TriggerSerumOfAlacrity(){
		if(this.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)){
			if(GameManager.Instance.month == this._triggerMonthOfSerum && GameManager.Instance.days == this._triggerDayOfSerum && GameManager.Instance.year == this._triggerYearOfSerum){
				int chance = UnityEngine.Random.Range(0,100);
				if(chance < 10){
					EventCreator.Instance.CreateSerumOfAlacrityEvent(this.citizen);
				}else{
					RandomTriggerDateOfSerum();
				}
			}
		}
	}

	private void RandomTriggerDateOfSerum(bool isFirst = false){
		this._triggerMonthOfSerum = UnityEngine.Random.Range(1,13);
		this._triggerDayOfSerum = UnityEngine.Random.Range(1, GameManager.daysInMonth[this._triggerMonthOfSerum]);

		if(isFirst){
			this._triggerYearOfSerum = GameManager.Instance.year;
		}else{
			this._triggerYearOfSerum = GameManager.Instance.year + 1;
		}
	}
	private bool IsEligibleForAbduction(Kingdom kingdom1, Kingdom kingdom2){
		List<GameEvent> allSpouseAbduction = EventManager.Instance.GetEventsOfType (EVENT_TYPES.SPOUSE_ABDUCTION).Where (x => x.isActive).ToList ();
		int counter = 0;
		if(allSpouseAbduction != null && allSpouseAbduction.Count > 0){
			for (int i = 0; i < allSpouseAbduction.Count; i++) {
				counter = 0;
				if(((SpouseAbduction)allSpouseAbduction[i]).abductorKingdom.id == kingdom1.id || ((SpouseAbduction)allSpouseAbduction[i]).targetKingdom.id == kingdom1.id){
					counter += 1;
				}
				if(((SpouseAbduction)allSpouseAbduction[i]).abductorKingdom.id == kingdom2.id || ((SpouseAbduction)allSpouseAbduction[i]).targetKingdom.id == kingdom2.id){
					counter += 1;
				}
				if(counter == 2){
					return false;
				}
			}
		}
		return true;
	}
	private bool IsReadyForAbduction(ref Citizen targetKing){
		if(this.citizen.spouse == null && !this.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR)){
			List<Citizen> targetKings = this.citizen.city.kingdom.discoveredKingdoms.Select (x => x.king).Where (x => x.isMarried && x.spouse != null && x.gender == this.citizen.gender).ToList();
			if(targetKings != null && targetKings.Count > 0){
				for (int i = 0; i < targetKings.Count; i++) {
					KingdomRelationship relationship = this.citizen.city.kingdom.GetRelationshipWithKingdom (targetKings [i].city.kingdom);
					if(relationship != null){
//						targetKing = targetKings [i];
//						return true;
						if(relationship.relationshipStatus == RELATIONSHIP_STATUS.LOVE || relationship.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE){
							targetKing = targetKings [i];
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
