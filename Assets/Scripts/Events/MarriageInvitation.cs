using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarriageInvitation : GameEvent {

	public List<Citizen> elligibleCitizens;
	protected int goldForEvent;

	public MarriageInvitation(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.MARRIAGE_INVITATION;
		if (startedBy.gender == GENDER.MALE) {
			this.description = startedBy.name + " is looking for a suitable wife as the vessel of his heir";
		} else {
			this.description = startedBy.name + " is looking for a suitable husband.";
		}

		this.durationInDays = 8;
		this.remainingDays = this.remainingDays;
		this.goldForEvent = 0;
		this.GetGoldForEvent ();

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
		this.EventIsCreated ();

	}

	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		if (this.remainingDays > 0) {
			this.remainingDays -= 1;

			if (this.startedBy.isMarried && this.startedBy.spouse != null) {
				this.resolution = this.startedBy.name + " got married to " + this.startedBy.spouse.name + " in some other marriage invitation event.";
				this.DoneEvent();
				return;
			}
			this.elligibleCitizens = MarriageManager.Instance.GetElligibleCitizensForMarriage(this.startedBy);
		} 
		if (this.remainingDays <= 0) {
			if (this.startedBy.isMarried && this.startedBy.spouse != null) {
				this.resolution = this.startedBy.name + " got married to " + this.startedBy.spouse.name + " in some other marriage invitation event.";
				this.DoneEvent();
				return;
			}
			//Choose bride
			if (this.elligibleCitizens.Count > 0) {
				Citizen chosenCitizen = this.elligibleCitizens[Random.Range(0, this.elligibleCitizens.Count)];
				City cityRecievingGold = null;
				if (chosenCitizen.city.id != this.startedBy.city.id) {
					//pay 500 gold to the chosen citizens city
					cityRecievingGold = chosenCitizen.city;
					cityRecievingGold.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, this.goldForEvent);
				} else {
					//give back 500 gold to city
					this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, this.goldForEvent);
				}

				if (this.startedBy.isKing) {
					//if startedBy is King
					//check if chosenCitizen has a parent that is a king
					Citizen chosenCitizenKingParent = chosenCitizen.GetKingParent();
					if (chosenCitizenKingParent != null) {
						RelationshipKings relationship = startedBy.city.kingdom.king.GetRelationshipWithCitizen(chosenCitizenKingParent);
						relationship.AdjustLikeness(15, this);

						relationship = chosenCitizenKingParent.city.kingdom.king.GetRelationshipWithCitizen(this.startedBy);
						relationship.AdjustLikeness(15, this);
					}
				} else if (chosenCitizen.isKing) {
					//if chosenCitizen is King
					//check if startedBy has a parent that is a king
					Citizen startedByKingParent = this.startedBy.GetKingParent ();
					if (startedByKingParent != null) {
						RelationshipKings relationship = startedBy.city.kingdom.king.GetRelationshipWithCitizen(startedByKingParent);
						relationship.AdjustLikeness(15, this);

						relationship = startedByKingParent.city.kingdom.king.GetRelationshipWithCitizen(this.startedBy);
						relationship.AdjustLikeness(15, this);

					}
				} else {
					//both citizens to be married are not kings, check if both citizens have parents that are king
					Citizen startedByKingParent = startedBy.GetKingParent ();
					Citizen chosenCitizenKingParent = chosenCitizen.GetKingParent ();
					if (startedByKingParent != null && chosenCitizenKingParent != null) {
						RelationshipKings relationship = startedByKingParent.city.kingdom.king.GetRelationshipWithCitizen(chosenCitizenKingParent);
						relationship.AdjustLikeness(15, this);

						relationship = chosenCitizenKingParent.city.kingdom.king.GetRelationshipWithCitizen(startedByKingParent);
						relationship.AdjustLikeness(15, this);
					}
				}

				//move chosenCitizen to startedBy city if chosenCitizen is not king, else, move startedBy
				if (this.startedBy.city.id != chosenCitizen.city.id) {
					if (chosenCitizen.isKing) {
						this.MoveCitizenToCity (this.startedBy, chosenCitizen.city);
						for (int i = 0; i < this.startedBy.dependentChildren.Count; i++) {
							this.MoveCitizenToCity (this.startedBy.dependentChildren [i], chosenCitizen.city);
						}
					} else {
						this.MoveCitizenToCity (chosenCitizen, this.startedBy.city);
						for (int i = 0; i < chosenCitizen.dependentChildren.Count; i++) {
							this.MoveCitizenToCity (chosenCitizen.dependentChildren [i], this.startedBy.city);
						}
					}
				}

				this.endDay = GameManager.Instance.days;
				this.endMonth = GameManager.Instance.month;
				this.endYear = GameManager.Instance.year;

				if (startedBy.gender == GENDER.MALE) {
					this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " has selected " +
					chosenCitizen.name + " as his wife. ";
				} else {
					this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " has selected " +
						chosenCitizen.name + " as her husband. ";
				}
				if (cityRecievingGold != null) {
					this.resolution += cityRecievingGold.name + ", " +  chosenCitizen.name + "'s home city, recieved " + this.goldForEvent.ToString() + " Gold as compensation.";
				}

				MarriageManager.Instance.Marry(startedBy, chosenCitizen);
				this.DoneEvent();
				return;
			}

			this.endDay = GameManager.Instance.days;
			this.endMonth = GameManager.Instance.month;
			this.endYear = GameManager.Instance.year;
			//return gold to city
			this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, 500);
			this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay.ToString () + ", " + this.endYear.ToString () + ". " + startedBy.name + " was unable to marry.";
			this.DoneEvent();
			return;
		}
	}

	protected void GetGoldForEvent(){
		this.startedBy.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, -500);
		this.goldForEvent = 500;
	}
		
	protected void MoveCitizenToCity(Citizen citizenToMove, City targetCity){
		citizenToMove.city.RemoveCitizenFromCity(citizenToMove);
		targetCity.AddCitizenToCity(citizenToMove);
	}

	internal override void DoneEvent(){
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
