using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class HandOfFate : MonoBehaviour {
	public Kingdom firstKingdom;
	public Kingdom secondKingdom;
	public EventRate eventToCreate;
//	public int compatibilityValue;
//	public bool isAdjacent;
//	public int raidChance;
//	public int borderConflictChance;
//	public int diplomaticCrisisChance;
//	public int stateVisitChance;
	public List<GameEvent> allUnwantedEvents;

	void Awake(){
		this.firstKingdom = null;
		this.secondKingdom = null;
//		this.compatibilityValue = 0;
//		this.isAdjacent = false;
//		this.raidChance = 0;
//		this.borderConflictChance = 0;
//		this.diplomaticCrisisChance = 0;
//		this.stateVisitChance = 0;
		this.allUnwantedEvents = new List<GameEvent> ();
	}

	[Task]
	public void CanCreateEvent(){
		Task.current.Succeed();
	}

	[Task]
	public void CannotCreateEvent(){
		Task.current.Fail();
	}

	[Task]
	public void ResetValues(){
		this.firstKingdom = null;
		this.secondKingdom = null;
//		this.compatibilityValue = 0;
//		this.isAdjacent = false;
//		this.raidChance = 0;
//		this.borderConflictChance = 0;
//		this.diplomaticCrisisChance = 0;
//		this.stateVisitChance = 0;
		this.eventToCreate.DefaultValues();
		this.allUnwantedEvents.Clear ();
		Task.current.Succeed ();
	}

	[Task]
	public void SetFirstRandomKingdom(){
		int total = 0;
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			total += KingdomManager.Instance.allKingdoms [i].kingdomTypeData.eventStartRate;
		}
		int lowerLimit = 0;
		int upperLimit = 0;
		int chance = UnityEngine.Random.Range (0, total + 1);
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			upperLimit += KingdomManager.Instance.allKingdoms [i].kingdomTypeData.eventStartRate;
			if(chance >= lowerLimit && chance < upperLimit){
				this.firstKingdom = KingdomManager.Instance.allKingdoms [i];
				break;
			}else{
				lowerLimit = upperLimit;
			}
		}

		if(this.firstKingdom == null){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}
	[Task]
	public void SetEventToCreate(){
		int total = this.firstKingdom.kingdomTypeData.eventRates.Sum(x => x.rate);
		int chance = UnityEngine.Random.Range (0, total + 1);
		int lowerLimit = 0;
		int upperLimit = 0;

		for(int i = 0; i < this.firstKingdom.kingdomTypeData.eventRates.Length; i++){
			upperLimit += this.firstKingdom.kingdomTypeData.eventRates[i].rate;
			if(chance >= lowerLimit && chance < upperLimit){
				this.eventToCreate =  this.firstKingdom.kingdomTypeData.eventRates[i];
				Debug.Log ("CREATING " + this.eventToCreate.eventType.ToString() + " EVENT...");
				break;
			}else{
				lowerLimit = upperLimit;
			}
		}
		if(this.eventToCreate.eventType == EVENT_TYPES.NONE){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}

	[Task]
	public void SetSecondRandomKingdom(){
		List<Kingdom> allKingdomCandidates = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.firstKingdom.id){
				if(IsCompatibleRelationship(KingdomManager.Instance.allKingdoms[i]) && IsCompatibleKingdomType(KingdomManager.Instance.allKingdoms[i]) 
					&& IsCompatibleMilitaryStrength(KingdomManager.Instance.allKingdoms[i])){

					allKingdomCandidates.Add (KingdomManager.Instance.allKingdoms [i]);

				}
			}
		}
		if(allKingdomCandidates.Count > 0){
			this.secondKingdom = allKingdomCandidates [UnityEngine.Random.Range (0, allKingdomCandidates.Count)];
		}

		if(this.secondKingdom == null){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}

	[Task]
	public void AreTheTwoKingdomsNotAtWar(){
		RelationshipKingdom relationship = this.firstKingdom.GetRelationshipWithOtherKingdom (this.secondKingdom);
		if(!relationship.isAtWar){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
//	[Task]
//	public void SetCompatibilityValue(){
//		int[] firstKingdomHoroscope = this.firstKingdom.horoscope.Concat (this.firstKingdom.king.horoscope).ToArray();
//		int[] secondKingdomHoroscope = this.secondKingdom.horoscope.Concat (this.secondKingdom.king.horoscope).ToArray();
//		int count = firstKingdomHoroscope.Length;
//
//		for(int i = 0; i < count; i++){
//			if(firstKingdomHoroscope[i] == secondKingdomHoroscope[i]){
//				this.compatibilityValue += 1;
//			}
//		}
//		Task.current.Succeed ();
//	}
//
//	[Task]
//	public void SetAdjacencyValue(){
//		this.isAdjacent = this.firstKingdom.IsKingdomAdjacentTo (this.secondKingdom);
//		Task.current.Succeed ();
//	}
//
//	[Task]
//	public void SetEventChances(){
//		if(this.isAdjacent){
//			if(this.compatibilityValue == 0 || this.compatibilityValue == 1){
//				this.raidChance = 50;
//				this.borderConflictChance = 35;
//				this.diplomaticCrisisChance = 15;
//				this.stateVisitChance = 0;
//			}else if(this.compatibilityValue == 2 || this.compatibilityValue == 3){
//				this.raidChance = 35;
//				this.borderConflictChance = 25;
//				this.diplomaticCrisisChance = 5;
//				this.stateVisitChance = 35;
//			}else if(this.compatibilityValue == 4 || this.compatibilityValue == 5){
//				this.raidChance = 20;
//				this.borderConflictChance = 10;
//				this.diplomaticCrisisChance = 0;
//				this.stateVisitChance = 70;
//			}
//		}else{
//			if(this.compatibilityValue == 0 || this.compatibilityValue == 1){
//				this.raidChance = 80;
//				this.borderConflictChance = 0;
//				this.diplomaticCrisisChance = 20;
//				this.stateVisitChance = 0;
//			}else if(this.compatibilityValue == 2 || this.compatibilityValue == 3){
//				this.raidChance = 55;
//				this.borderConflictChance = 0;
//				this.diplomaticCrisisChance = 10;
//				this.stateVisitChance = 35;
//			}else if(this.compatibilityValue == 4 || this.compatibilityValue == 5){
//				this.raidChance = 25;
//				this.borderConflictChance = 0;
//				this.diplomaticCrisisChance = 5;
//				this.stateVisitChance = 70;
//			}
//		}
//		Task.current.Succeed ();
//	}
	[Task]
	public void IsEligibleForEvent(){
		List<GameEvent> allRaids = EventManager.Instance.GetEventsOfType (EVENT_TYPES.RAID).Where (x => x.isActive).ToList ();
		List<GameEvent> allBorderConflicts = EventManager.Instance.GetEventsOfType (EVENT_TYPES.BORDER_CONFLICT).Where (x => x.isActive).ToList ();
		List<GameEvent> allDiplomaticCrisis = EventManager.Instance.GetEventsOfType (EVENT_TYPES.DIPLOMATIC_CRISIS).Where (x => x.isActive).ToList ();
		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
		this.allUnwantedEvents = allRaids.Concat (allBorderConflicts).Concat (allDiplomaticCrisis).Concat (allStateVisit).ToList ();

		if (this.allUnwantedEvents.Count > 0) {
			if (SearchForEligibility (this.firstKingdom, this.secondKingdom, this.allUnwantedEvents)) {
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Succeed ();
		}

	}
	[Task]
	public void StartAnEvent(){
		switch(this.eventToCreate.eventType){
		case EVENT_TYPES.RAID:
			CreateRaidEvent ();
			break;
		case EVENT_TYPES.BORDER_CONFLICT:
			CreateBorderConflictEvent ();
			break;
		case EVENT_TYPES.DIPLOMATIC_CRISIS:
			CreateDiplomaticCrisisEvent ();
			break;
		case EVENT_TYPES.STATE_VISIT:
			CreateStateVisitEvent ();
			break;
		}
		Task.current.Succeed ();
	}

	private void CreateRaidEvent(){
//		General general = GetGeneral(this.firstKingdom);
		City city = GetRaidedCity();
		if(city != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.firstKingdom.king, city);
			EventManager.Instance.AddEventToDictionary (raid);
		}
	}

	private void CreateBorderConflictEvent(){
		BorderConflict();
	}

	private void CreateDiplomaticCrisisEvent(){
		DiplomaticCrisis ();
	}

	private void CreateStateVisitEvent(){
		StateVisit ();
	}
	private General GetGeneral(Kingdom kingdom){
		List<Citizen> unwantedGovernors = Utilities.GetUnwantedGovernors (kingdom.king);
		List<General> generals = new List<General> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!Utilities.IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.GENERAL) {
							if(kingdom.cities [i].citizens [j].assignedRole is General){
								if (!((General)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									generals.Add (((General)kingdom.cities [i].citizens [j].assignedRole));
								}
							}
						}
					}
				}
			}
		}

		if(generals.Count > 0){
			int random = UnityEngine.Random.Range (0, generals.Count);
			generals [random].inAction = true;
			return generals [random];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SEND GENERAL BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private City GetRaidedCity(){
		if(this.secondKingdom.cities.Count > 0){
			return this.secondKingdom.cities [UnityEngine.Random.Range (0, this.secondKingdom.cities.Count)];
		}else{
			return null;
		}
		//		if(general == null){
		//			return null;
		//		}
		//		if(this.isAdjacent){
		//			List<City> adjacentCities = general.citizen.city.kingdom.adjacentCitiesFromOtherKingdoms.Where (x => x.kingdom.id == this.secondKingdom.id).ToList();
		//			if(adjacentCities.Count > 0){
		//				return adjacentCities [UnityEngine.Random.Range (0, adjacentCities.Count)];
		//			}else{
		//				if(this.secondKingdom.cities.Count > 0){
		//					return this.secondKingdom.cities [UnityEngine.Random.Range (0, this.secondKingdom.cities.Count)];
		//				}else{
		//					return null;
		//				}
		//			}
		//		}else{
		//			if(this.secondKingdom.cities.Count > 0){
		//				return this.secondKingdom.cities [UnityEngine.Random.Range (0, this.secondKingdom.cities.Count)];
		//			}else{
		//				return null;
		//			}
		//		}

	}
	private void BorderConflict(){
		Debug.Log ("Border Conflict FROM HAND OF FATE");
		Citizen startedBy = this.firstKingdom.king;
		BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.firstKingdom, this.secondKingdom);
		EventManager.Instance.AddEventToDictionary(borderConflict);
	}
	private void DiplomaticCrisis(){
		Debug.Log ("Diplomatic Crisis FROM HAND OF FATE");
		Citizen startedBy = this.secondKingdom.king;
		DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
		EventManager.Instance.AddEventToDictionary(diplomaticCrisis);
	}
//	private void Admiration(){
//		Debug.Log ("Admiration FROM HAND OF FATE");
//		Citizen startedBy = this.secondKingdom.king;
//		Admiration admiration = new Admiration(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
//		EventManager.Instance.AddEventToDictionary(admiration);
//	}
	internal void StateVisit(){
		Debug.Log ("State Visit FROM HAND OF FATE");
		Citizen targetKing = this.secondKingdom.king;
		Citizen visitor = null;
		if(targetKing.spouse != null && !targetKing.spouse.isDead){
			visitor = targetKing.spouse;
		}else{
			if(targetKing.children != null && targetKing.children.Count > 0){
				for(int i = 0; i < targetKing.children.Count; i++){
					if(targetKing.children[i] != null && !targetKing.children[i].isDead && targetKing.children[i].age >= 16){
						visitor = targetKing.children [i];
						break;
					}
				}
			}
		}

		if(visitor == null){
			List<Citizen> siblings = targetKing.GetSiblings();
			for (int i = 0; i < siblings.Count; i++) {
				if(siblings[i] != null && !siblings[i].isDead && siblings[i].age >= 16){
					visitor = siblings[i];
					break;
				}
			}
		}

		if(visitor != null){
			StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.firstKingdom.king, this.secondKingdom, visitor);
			EventManager.Instance.AddEventToDictionary(stateVisit);
		}else{
			Debug.Log ("FAILURE TO START STATE VISIT BECAUSE THERE IS NO AVAIALABLE VISITOR!");
		}

	}

	private bool SearchForEligibility (Kingdom kingdom1, Kingdom kingdom2, List<GameEvent> gameEvents){
		for(int i = 0; i < gameEvents.Count; i++){
			if(!IsEligibleForConflict(kingdom1,kingdom2, gameEvents[i])){
				return false;
			}
		}
		return true;
	}
	private bool IsEligibleForConflict(Kingdom kingdom1, Kingdom kingdom2, GameEvent gameEvent){
		if (!gameEvent.isActive) {
			return true;
		}

		int counter = 0;

		if(gameEvent is BorderConflict){
			if(((BorderConflict)gameEvent).kingdom1.id == kingdom1.id || ((BorderConflict)gameEvent).kingdom2.id == kingdom1.id){
				counter += 1;
			}
			if(((BorderConflict)gameEvent).kingdom1.id == kingdom2.id || ((BorderConflict)gameEvent).kingdom2.id == kingdom2.id){
				counter += 1;
			}
		}else if(gameEvent is DiplomaticCrisis) {
			if(((DiplomaticCrisis)gameEvent).kingdom1.id == kingdom1.id || ((DiplomaticCrisis)gameEvent).kingdom2.id == kingdom1.id){
				counter += 1;
			}
			if(((DiplomaticCrisis)gameEvent).kingdom1.id == kingdom2.id || ((DiplomaticCrisis)gameEvent).kingdom2.id == kingdom2.id){
				counter += 1;
			}
		}else if(gameEvent is StateVisit) {
			if(((StateVisit)gameEvent).invitedKingdom.id == kingdom1.id || ((StateVisit)gameEvent).inviterKingdom.id == kingdom1.id){
				counter += 1;
			}
			if(((StateVisit)gameEvent).invitedKingdom.id == kingdom2.id || ((StateVisit)gameEvent).inviterKingdom.id == kingdom2.id){
				counter += 1;
			}
		}else if(gameEvent is Raid) {
			if(((Raid)gameEvent).startedBy.city.kingdom.id == kingdom1.id || ((Raid)gameEvent).raidedCity.kingdom.id == kingdom1.id){
				counter += 1;
			}
			if(((Raid)gameEvent).startedBy.city.kingdom.id == kingdom2.id || ((Raid)gameEvent).raidedCity.kingdom.id == kingdom2.id){
				counter += 1;
			}
		}

		if(counter == 2){
			return false;
		}else{
			return true;
		}
	}

	private bool IsCompatibleRelationship(Kingdom targetKingdom){
		if (this.eventToCreate.relationshipTargets.Length > 0) {
			RelationshipKings relationship = this.firstKingdom.king.GetRelationshipWithCitizen (targetKingdom.king);
			Debug.Log ("RELATIONSHIP: " + relationship.lordRelationship.ToString());
			if (this.eventToCreate.relationshipTargets.Contains (relationship.lordRelationship)) {
				return true;
			}else{
				return false;
			}
		}
		return true;
	}

	private bool IsCompatibleKingdomType(Kingdom targetKingdom){
		if (this.eventToCreate.kingdomTypes.Length > 0) {
			if (this.eventToCreate.kingdomTypes.Contains (targetKingdom.kingdomType)) {
				return true;
			}else{
				return false;
			}
		}
		return true;
	}

	private bool IsCompatibleMilitaryStrength(Kingdom targetKingdom){
		if (this.eventToCreate.militaryStrength.Length > 0) {
			MILITARY_STRENGTH milStrength = targetKingdom.GetMilitaryStrengthAgainst (this.firstKingdom);
			Debug.Log ("MILITAR STRENGTH: " + milStrength);
			if (this.eventToCreate.militaryStrength.Contains (milStrength)) {
				return true;
			}else{
				return false;
			}
		}
		return true;
	}
}
