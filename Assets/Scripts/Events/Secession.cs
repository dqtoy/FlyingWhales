using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Secession : GameEvent {

	internal Governor governor;
	internal Kingdom sourceKingdom;
	internal List<City> alreadyVisitedCities;
	internal List<City> joiningCities;
	internal List<Governor> candidateGovernors;
	internal Envoy convincer;
	internal City targetCity;
	public Secession(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SECESSION;
		this.name = "Secession";
		//		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.governor = (Governor)startedBy.assignedRole;
		this.sourceKingdom = startedBy.city.kingdom;
		this.alreadyVisitedCities = new List<City> ();
		this.joiningCities = new List<City> ();
		this.candidateGovernors = new List<Governor> ();
		this.convincer = null;
		this.targetCity = null;
		this.joiningCities.Add (startedBy.city);
        this.alreadyVisitedCities.Add(startedBy.city);
		this.sourceKingdom.SetSecession (true);

		Debug.Log (startedBy.name + " wants to split from " + this.sourceKingdom.name + " because his/her loyalty is " + this.governor.loyalty);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "event_title");
		newLogTitle.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "start");
		newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
		newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		EventManager.Instance.AddEventToDictionary (this);
		this.EventIsCreated (this.sourceKingdom, true);
		Messenger.AddListener("OnDayEnd", this.PerformAction);

	}

	#region Overrides
	internal override void PerformAction (){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			DoneEvent ();
		}else{
			if(this.remainingDays % 5 == 0 ){
//				SendEnvoy ();
				AskToJoinSecession();
			}
		}
	}
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction(citizen);
		if(this.convincer != null){
			if(citizen.id == this.convincer.citizen.id){
				//Convince governor to join
				ConvinceGovernor();
			}
		}
		this.targetCity = null;
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent();
        Messenger.RemoveListener("OnDayEnd", this.PerformAction);
		this.sourceKingdom.SetSecession (false);
        this.SplitKingdom (); //Generate new kingdom
    }
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.targetCity = null;
		this.DoneEvent ();
	}
	#endregion

	private void SendEnvoy(){
		List<City> candidateCities = this.governor.citizen.city.kingdom.cities.Where (x => !this.alreadyVisitedCities.Contains (x)).ToList();
		if(candidateCities != null && candidateCities.Count > 0){
			this.targetCity = candidateCities [UnityEngine.Random.Range (0, candidateCities.Count)];
		}
		if (this.targetCity == null) {
			return;
		}

		Citizen chosenCitizen = this.governor.citizen.city.CreateAgent (ROLE.ENVOY, this.eventType, this.targetCity.hexTile, this.remainingDays);
		if(chosenCitizen == null){
			this.targetCity = null;
			return;
		}
		this.alreadyVisitedCities.Add (this.targetCity);
		chosenCitizen.assignedRole.Initialize (this);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "envoy_send");
		newLog.AddToFillers (chosenCitizen, chosenCitizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.targetCity, this.targetCity.name, LOG_IDENTIFIER.CITY_2);
		newLog.AddToFillers (this.targetCity.governor, this.targetCity.governor.name, LOG_IDENTIFIER.GOVERNOR_2);
		newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);

	}
	private void ConvinceGovernor(){
		if(this.targetCity.governor != null){
			if(((Governor)this.targetCity.governor.assignedRole).loyalty < 0){
				this.joiningCities.Add (this.targetCity);

				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "convince_success");
				newLog.AddToFillers (this.convincer.citizen, this.convincer.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this.targetCity.governor, this.targetCity.governor.name, LOG_IDENTIFIER.GOVERNOR_2);
				newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
				newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			}
		}
	}
	private void AskToJoinSecession(){
		this.candidateGovernors.Clear ();
		for (int i = 0; i < this.sourceKingdom.cities.Count; i++) {
			if(!this.joiningCities.Contains(this.sourceKingdom.cities[i])){
				Governor governor = (Governor)this.sourceKingdom.cities [i].governor.assignedRole;
				if(governor.loyalty < 0){
					this.candidateGovernors.Add (governor);
				}
			}
		}

		if(this.candidateGovernors.Count > 0){
			Governor chosenGovernor = this.candidateGovernors [UnityEngine.Random.Range (0, this.candidateGovernors.Count)];

			int chance = UnityEngine.Random.Range (0, 100);
			int value = chosenGovernor.loyalty * -1;
			if(chance < value){
				JoinSecession (chosenGovernor.citizen);
			}
		}
	}
	private void JoinSecession(Citizen governorToJoin){
		this.joiningCities.Add (governorToJoin.city);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "governor_joined");
		newLog.AddToFillers (governorToJoin, governorToJoin.name, LOG_IDENTIFIER.GOVERNOR_2);
		newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
	}
	private void SplitKingdom(){
		bool kingFled = false;
		if(this.joiningCities.Count == this.sourceKingdom.cities.Count){
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "entire_kingdom_secedes");
			newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
			newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
			this.sourceKingdom.king.Death (DEATH_REASONS.ASSASSINATION, true, this.governor.citizen);
		}else{
			if (this.joiningCities.Contains (this.sourceKingdom.king.city)) {
				//Move King To Another City
				this.sourceKingdom.king.city.hasKing = false;
				List<City> cityCandidates = this.sourceKingdom.cities.Where (x => !this.joiningCities.Contains (x)).ToList ();
				if (cityCandidates != null && cityCandidates.Count > 0) {
					City newCityForRoyalties = cityCandidates [0];
					for (int i = 0; i < this.sourceKingdom.king.city.citizens.Count; i++) {
						if (this.sourceKingdom.king.city.citizens [i].isDirectDescendant && !this.sourceKingdom.king.city.citizens [i].isGovernor) {
							newCityForRoyalties.MoveCitizenToThisCity (this.sourceKingdom.king.city.citizens [i], true);
						}
						if (this.sourceKingdom.king.isMarried) {
							newCityForRoyalties.MoveCitizenToThisCity (this.sourceKingdom.king.spouse, true);
						}
					}
					this.sourceKingdom.capitalCity = newCityForRoyalties;
					newCityForRoyalties.hasKing = true;
					kingFled = true;
				} else {
					int countCitizens = this.sourceKingdom.king.city.citizens.Count;
					for (int i = 0; i < countCitizens; i++) {
						if (this.sourceKingdom.king.city.citizens [0].isDirectDescendant && !this.sourceKingdom.king.city.citizens [0].isGovernor) {
							this.sourceKingdom.king.city.citizens [0].Death (DEATH_REASONS.ASSASSINATION, false, null, true);
						}
					}
					if (this.sourceKingdom.king.isMarried) {
						this.sourceKingdom.king.spouse.Death (DEATH_REASONS.ASSASSINATION, false, null, true);
					}
					this.sourceKingdom.capitalCity = null;
				}
			}

			Kingdom newKingdom = KingdomManager.Instance.SplitKingdom (this.sourceKingdom, this.joiningCities);
			if (newKingdom != null) {
				string newCitiesText = string.Empty;

				if(this.joiningCities.Count == 1){
					newCitiesText += this.joiningCities [0].name;
				}else{
					for (int i = 0; i < this.joiningCities.Count; i++) {
						if (i != this.joiningCities.Count - 1) {
							newCitiesText += this.joiningCities [i].name + ", ";
						} else {
							newCitiesText += "and " + this.joiningCities [i].name;
						}
					}
				}


				if(kingFled){
					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "king_flees");
					newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
					newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
					newLog.AddToFillers (newKingdom, newKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
					newLog.AddToFillers (null, newCitiesText, LOG_IDENTIFIER.SECESSION_CITIES);
					newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
					newLog.AddToFillers (this.sourceKingdom.capitalCity, this.sourceKingdom.capitalCity.name, LOG_IDENTIFIER.CITY_1);

				}else{
					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Secession", "secession_success");
					newLog.AddToFillers (this.governor.citizen, this.governor.citizen.name, LOG_IDENTIFIER.GOVERNOR_1);
					newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
					newLog.AddToFillers (newKingdom, newKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
					newLog.AddToFillers (null, newCitiesText, LOG_IDENTIFIER.SECESSION_CITIES);
				}

				newKingdom.AssignNewKing (this.governor.citizen);



			}
		}
	}
}
