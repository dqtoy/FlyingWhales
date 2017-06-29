using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class King : Role {

	public Kingdom ownedKingdom;
	internal int abductionCounter;
	public King(Citizen citizen): base(citizen){
		this.citizen.isKing = true;
//		if(this.citizen.city.kingdom.king != null){
//			this.citizen.CopyCampaignManager (this.citizen.city.kingdom.king.campaignManager);
//		}
		this.citizen.city.kingdom.king = this.citizen;
		this.SetOwnedKingdom(this.citizen.city.kingdom);
		this.citizen.GenerateCharacterValues ();
		this.abductionCounter = 0;
		if(this.citizen.city.kingdom.plague != null){
			this.citizen.city.kingdom.plague.UpdateApproach (this.citizen.city.kingdom);
		}
		EventManager.Instance.onWeekEnd.AddListener (TriggerSpouseAbduction);
	}

	internal void SetOwnedKingdom(Kingdom ownedKingdom){
		this.ownedKingdom = ownedKingdom;
	}

	internal void TriggerSpouseAbduction(){
		if(!this.citizen.isDead){
			if((MONTH)GameManager.Instance.month == this.citizen.birthMonth && GameManager.Instance.days == this.citizen.birthWeek && GameManager.Instance.year > this.citizen.birthYear){
				this.abductionCounter += 1;
				int chance = UnityEngine.Random.Range (0, 100);
				if(chance < 100 * this.abductionCounter){
					Citizen targetKing = null;
					if(IsReadyForAbduction(ref targetKing) && IsEligibleForAbduction(this.citizen.city.kingdom, targetKing.city.kingdom)){
						EventCreator.Instance.CreateSpouseAbductionEvent (this.citizen, targetKing);
					}
				}
			}
		}else{
			EventManager.Instance.onWeekEnd.RemoveListener (TriggerSpouseAbduction);
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
			List<Citizen> targetKings = KingdomManager.Instance.allKingdoms.Select (x => x.king).Where (x => x.isMarried && x.spouse != null && x.gender == this.citizen.gender).ToList();
			if(targetKings != null && targetKings.Count > 0){
				for (int i = 0; i < targetKings.Count; i++) {
					RelationshipKings relationship = this.citizen.GetRelationshipWithCitizen (targetKings [i]);
					if(relationship != null){
						targetKing = targetKings [i];
						return true;
//						if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY || relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
//							targetKing = targetKings [i];
//							return true;
//						}
					}
				}
			}
		}
		return false;
	}
}
