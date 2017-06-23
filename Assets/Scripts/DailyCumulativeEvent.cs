using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class DailyCumulativeEvent : MonoBehaviour {
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
	public void HasDiscoveredKingdoms(){
		if(this.firstKingdom.discoveredKingdoms != null && this.firstKingdom.discoveredKingdoms.Count > 0){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void SetEventsToCreate(){
		for(int i = 0; i < this.firstKingdom.dailyCumulativeEventRate.Length; i++){
			if(!CanCreateEvent(this.firstKingdom.dailyCumulativeEventRate[i].eventType)){
				continue;
			}
			int chance = UnityEngine.Random.Range (0, 200);
			if(chance < this.firstKingdom.dailyCumulativeEventRate [i].rate){
				this.eventToCreate = this.firstKingdom.dailyCumulativeEventRate [i];
				SetSecondRandomKingdom (ref this.firstKingdom.dailyCumulativeEventRate [i]);
			}else{
				this.firstKingdom.dailyCumulativeEventRate [i].rate += this.firstKingdom.dailyCumulativeEventRate [i].interval;
			}
		}
		Task.current.Succeed ();
//		if(this.eventToCreate.eventType == EVENT_TYPES.NONE){
//			Task.current.Fail ();
//		}else{
//			Task.current.Succeed ();
//		}
	}

//	[Task]
	public void SetSecondRandomKingdom(ref EventRate eventRate){
		List<Kingdom> allKingdomCandidates = new List<Kingdom> ();
		for(int i = 0; i < this.firstKingdom.discoveredKingdoms.Count; i++){
			if(IsCompatibleRelationship(this.firstKingdom.discoveredKingdoms[i]) && IsCompatibleKingdomType(this.firstKingdom.discoveredKingdoms[i]) 
				&& IsCompatibleMilitaryStrength(this.firstKingdom.discoveredKingdoms[i])){

				allKingdomCandidates.Add (this.firstKingdom.discoveredKingdoms[i]);

			}
		}
		if(allKingdomCandidates.Count > 0){
			this.secondKingdom = allKingdomCandidates [UnityEngine.Random.Range (0, allKingdomCandidates.Count)];
		}

		if(this.secondKingdom != null){
			if(!AreTheTwoKingdomsAtWar() && IsEligibleForEvent()){
				StartAnEvent (ref eventRate);
			}
		}
	}

//	[Task]
	public bool AreTheTwoKingdomsAtWar(){
		RelationshipKingdom relationship = this.firstKingdom.GetRelationshipWithOtherKingdom (this.secondKingdom);
		return relationship.isAtWar;
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
//	[Task]
	public bool IsEligibleForEvent(){
		List<GameEvent> allEventsOfType = EventManager.Instance.GetEventsOfType (this.eventToCreate.eventType).Where (x => x.isActive).ToList ();
//		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
//		List<GameEvent> allTrade = EventManager.Instance.GetEventsOfType (EVENT_TYPES.TRADE).Where (x => x.isActive).ToList ();
//		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
//		this.allUnwantedEvents = allRaids.Concat (allStateVisit).Concat (allTrade).Concat (allStateVisit).ToList ();

		if (allEventsOfType.Count > 0) {
			if (SearchForEligibility (this.firstKingdom, this.secondKingdom, allEventsOfType)) {
				return true;
			}else{
				return false;
			}
		}else{
			return true;
		}

	}
//	[Task]
	public void StartAnEvent(ref EventRate eventRate){
		eventRate.ResetRateAndMultiplier ();
		switch(this.eventToCreate.eventType){
		case EVENT_TYPES.RAID:
			CreateRaidEvent ();
			break;
		case EVENT_TYPES.STATE_VISIT:
			CreateStateVisitEvent ();
			break;
		case EVENT_TYPES.TRADE:
			CreateTradeEvent ();
			break;
		case EVENT_TYPES.SCOURGE_CITY:
			CreateScourgeCityEvent ();
			break;
		}
//		Task.current.Succeed ();
	}
	[Task]
	public void Control(){
		Task.current.Succeed();
	}
	private void CreateRaidEvent(){
//		General general = GetGeneral(this.firstKingdom);
		EventCreator.Instance.CreateRaidEvent(this.firstKingdom, this.secondKingdom);
	}
	private void CreateStateVisitEvent(){
		EventCreator.Instance.CreateStateVisitEvent(this.firstKingdom, this.secondKingdom);
	}
	private void CreateTradeEvent(){
        //Create Trade Event
        RelationshipKingdom relWithOtherKingdom = this.firstKingdom.GetRelationshipWithOtherKingdom(this.secondKingdom);
        City randomSourceCity = this.firstKingdom.cities[Random.Range(0, this.firstKingdom.cities.Count)];
        City randomTargetCity = this.secondKingdom.cities[Random.Range(0, this.secondKingdom.cities.Count)];
        List<HexTile> path = PathGenerator.Instance.GetPath(randomSourceCity.hexTile, randomTargetCity.hexTile, PATHFINDING_MODE.NORMAL).ToList();
        List<RESOURCE> resourcesSourceKingdomCanOffer = this.firstKingdom.GetResourcesOtherKingdomDoesNotHave(this.secondKingdom)
			.Where(x => Utilities.resourceBenefits[x].FirstOrDefault().Key == RESOURCE_BENEFITS.GROWTH_RATE ||
				Utilities.resourceBenefits[x].FirstOrDefault().Key == RESOURCE_BENEFITS.TECH_LEVEL).ToList();
        /*
         * There should be no active trade event between the two kingdoms (started by this kingdom), the 2 kingdoms should not be at war, 
         * there should be a path from this kingdom's capital city to the otherKingdom's capital city, the otherKingdom should not be part of this kingdom's embargo list
         * and this kingdom should have a resource that the otherKingdom does not.
         * */
        if (!relWithOtherKingdom.isAtWar && path != null && !this.firstKingdom.embargoList.ContainsKey(this.secondKingdom) 
            && resourcesSourceKingdomCanOffer.Count > 0) {
            EventCreator.Instance.CreateTradeEvent(randomSourceCity, randomTargetCity);

        }
    }
	private void CreateScourgeCityEvent(){
		EventCreator.Instance.CreateScourgeCityEvent (this.firstKingdom, this.secondKingdom);
	}
//	private void CreateBorderConflictEvent(){
//		EventCreator.Instance.CreateBorderConflictEvent(this.firstKingdom, this.secondKingdom);
//	}
//
//	private void CreateDiplomaticCrisisEvent(){
//		EventCreator.Instance.CreateDiplomaticCrisisEvent(this.firstKingdom, this.secondKingdom);
//	}


	/*internal void StateVisit(){
//		Debug.Log ("State Visit FROM HAND OF FATE");
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
//			EventManager.Instance.AddEventToDictionary(stateVisit);
		}else{
//			Debug.Log ("FAILURE TO START STATE VISIT BECAUSE THERE IS NO AVAIALABLE VISITOR!");
		}

	}*/

	private bool SearchForEligibility (Kingdom kingdom1, Kingdom kingdom2, List<GameEvent> gameEvents){
		for(int i = 0; i < gameEvents.Count; i++){
			if(!IsEligibleForConflict(kingdom1, kingdom2, gameEvents[i])){
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

//		if(gameEvent is BorderConflict){
//			if(((BorderConflict)gameEvent).kingdom1.id == kingdom1.id || ((BorderConflict)gameEvent).kingdom2.id == kingdom1.id){
//				counter += 1;
//			}
//			if(((BorderConflict)gameEvent).kingdom1.id == kingdom2.id || ((BorderConflict)gameEvent).kingdom2.id == kingdom2.id){
//				counter += 1;
//			}
//		}else if(gameEvent is DiplomaticCrisis) {
//			if(((DiplomaticCrisis)gameEvent).kingdom1.id == kingdom1.id || ((DiplomaticCrisis)gameEvent).kingdom2.id == kingdom1.id){
//				counter += 1;
//			}
//			if(((DiplomaticCrisis)gameEvent).kingdom1.id == kingdom2.id || ((DiplomaticCrisis)gameEvent).kingdom2.id == kingdom2.id){
//				counter += 1;
//			}
//		}else 
		if(gameEvent is StateVisit) {
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
		}else if(gameEvent is Trade) {
			if(((Trade)gameEvent).sourceCity.kingdom.id == kingdom1.id || ((Trade)gameEvent).targetCity.kingdom.id == kingdom1.id){
				counter += 1;
			}
			if(((Trade)gameEvent).sourceCity.kingdom.id == kingdom2.id || ((Trade)gameEvent).targetCity.kingdom.id == kingdom2.id){
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
//			Debug.Log ("RELATIONSHIP: " + relationship.lordRelationship.ToString());
			if(relationship == null){
				return false;
			}
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
//			Debug.Log ("MILITAR STRENGTH: " + milStrength);
			if (this.eventToCreate.militaryStrength.Contains (milStrength)) {
				return true;
			}else{
				return false;
			}
		}
		return true;
	}
	private bool CanCreateEvent(EVENT_TYPES eventType){
		if(eventType == EVENT_TYPES.SCOURGE_CITY){
			if(this.firstKingdom.hasBioWeapon){
				return true;
			}
			return false;
		}else{
			return true;
		}
	}
}
