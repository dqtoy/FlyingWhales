using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class DailyCumulativeEvent : MonoBehaviour {
	public int interval;
	public Kingdom firstKingdom;
	public Kingdom secondKingdom;
	public EventRate eventToCreate;
//	public int compatibilityValue;
//	public bool isAdjacent;
//	public int raidChance;
//	public int borderConflictChance;
//	public int diplomaticCrisisChance;
//	public int stateVisitChance;
//	public List<GameEvent> allUnwantedEvents;
	private int counter;

	void Awake(){
		this.firstKingdom = null;
		this.secondKingdom = null;
		this.counter = 0;
	}

    private void Start() {
        Messenger.AddListener("OnDayEnd", this.GetComponent<PandaBehaviour>().Tick);
    }

    [Task]
	public void ResetValues(){
		this.firstKingdom = null;
		this.secondKingdom = null;
		this.eventToCreate.DefaultValues();
		Task.current.Succeed ();
	}
	private void Reset(){
		this.firstKingdom = null;
		this.secondKingdom = null;
		this.eventToCreate.DefaultValues();

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
	public void DailyInterval(){
		this.counter += 1;
		Task.current.Succeed ();
	}
	[Task]
	public void CreateDailyCumulativeEvents(){
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Reset ();
			this.firstKingdom = KingdomManager.Instance.allKingdoms [i];
			SetEventsToCreate ();
		}
		Task.current.Succeed ();
	}
	[Task]
	public void HasDiscoveredKingdoms(){
		if(this.firstKingdom.discoveredKingdoms != null && this.firstKingdom.discoveredKingdoms.Count > 0){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
//	[Task]
	public void SetEventsToCreate(){
		for(int i = 0; i < this.firstKingdom.dailyCumulativeEventRate.Length; i++){
			if(!CanCreateEvent(this.firstKingdom.dailyCumulativeEventRate[i].eventType)){
				continue;
			}
			int chance = UnityEngine.Random.Range (0, 400);
			if(chance < this.firstKingdom.dailyCumulativeEventRate [i].rate){
				this.eventToCreate = this.firstKingdom.dailyCumulativeEventRate [i];
				if(!IsSecondKingdomNeeded()){
					StartAnEvent (ref this.eventToCreate);
				}else{
					SetSecondRandomKingdom (ref this.firstKingdom.dailyCumulativeEventRate [i]);
				}
			}else{
				if (this.counter >= this.interval) {
					this.firstKingdom.dailyCumulativeEventRate [i].rate += this.firstKingdom.dailyCumulativeEventRate [i].interval;
				}
			}
		}
		if (this.counter >= this.interval) {
			this.counter = 0;
		}
//		Task.current.Succeed ();
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
		KingdomRelationship relationship = this.firstKingdom.GetRelationshipWithKingdom (this.secondKingdom);
		if (relationship == null) {
			return false;
		}
		return relationship.sharedRelationship.isAtWar;
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
		if(this.firstKingdom.HasActiveEvent(this.eventToCreate.eventType) && this.secondKingdom.HasActiveEvent(this.eventToCreate.eventType)){
			return false;
		}
		return true;
//		List<GameEvent> allEventsOfType = EventManager.Instance.GetEventsOfType (this.eventToCreate.eventType).Where (x => x.isActive).ToList ();
//		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
//		List<GameEvent> allTrade = EventManager.Instance.GetEventsOfType (EVENT_TYPES.TRADE).Where (x => x.isActive).ToList ();
//		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
//		this.allUnwantedEvents = allRaids.Concat (allStateVisit).Concat (allTrade).Concat (allStateVisit).ToList ();

//		if (allEventsOfType.Count > 0) {
//			if (SearchForEligibility (this.firstKingdom, this.secondKingdom, allEventsOfType)) {
//				return true;
//			}else{
//				return false;
//			}
//		}else{
//			return true;
//		}

	}
//	[Task]
	public void StartAnEvent(ref EventRate eventRate){
		eventRate.ResetRateAndMultiplier ();
		switch(this.eventToCreate.eventType){
		case EVENT_TYPES.HUNT_LAIR:
			CreateHuntLairEvent ();
			break;
		}
//		Task.current.Succeed ();
	}


	private void CreateHuntLairEvent(){
		//EventCreator.Instance.CreateHuntLairEvent(this.firstKingdom);
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
	
		if(counter == 2){
			return false;
		}else{
			return true;
		}
	}

	private bool IsCompatibleRelationship(Kingdom targetKingdom){
		if (this.eventToCreate.relationshipTargets.Length > 0) {
			KingdomRelationship relationship = this.firstKingdom.GetRelationshipWithKingdom(targetKingdom);
//			Debug.Log ("RELATIONSHIP: " + relationship.relationshipStatus.ToString());
			if(relationship == null){
				return false;
			}
			if (this.eventToCreate.relationshipTargets.Contains (relationship.relationshipStatus)) {
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
        if (eventType == EVENT_TYPES.SCOURGE_CITY) {
            if (this.firstKingdom.hasBioWeapon) {
                return true;
            }
            return false;
        } else if (eventType == EVENT_TYPES.ADVENTURE) {
			if (!this.firstKingdom.HasActiveEvent(eventType)) {
                return true;
            }
            return false;
        } else {
            return true;
        }
	}
	private bool IsSecondKingdomNeeded(){
		if(this.eventToCreate.eventType == EVENT_TYPES.ADVENTURE){
			return false;
		}else if(this.eventToCreate.eventType == EVENT_TYPES.HUNT_LAIR){
			return false;
		}
		return true;
	}
}
