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
	private List<Kingdom> allKingdomCandidates = new List<Kingdom> ();

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

    private void Start() {
        Messenger.AddListener(Signals.DAY_END, this.GetComponent<PandaBehaviour>().Tick);
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
//		int total = this.firstKingdom.kingdomTypeData.eventRates.Sum(x => x.rate);
//		int chance = UnityEngine.Random.Range (0, total + 1);
//		int lowerLimit = 0;
//		int upperLimit = 0;
//
//		for(int i = 0; i < this.firstKingdom.kingdomTypeData.eventRates.Length; i++){
//			upperLimit += this.firstKingdom.kingdomTypeData.eventRates[i].rate;
//			if(chance >= lowerLimit && chance < upperLimit){
//				this.eventToCreate =  this.firstKingdom.kingdomTypeData.eventRates[i];
//				break;
//			}else{
//				lowerLimit = upperLimit;
//			}
//		}
//		if(this.eventToCreate.eventType == EVENT_TYPES.NONE){
//			Task.current.Fail ();
//		}else{
//			Task.current.Succeed ();
//		}
	}

	[Task]
	public void SetSecondRandomKingdom(){
		allKingdomCandidates.Clear ();
        List<Kingdom> kingdomsToChooseFrom = this.firstKingdom.discoveredKingdoms;

		for(int i = 0; i < kingdomsToChooseFrom.Count; i++){
            Kingdom currKingdom = kingdomsToChooseFrom[i];
            if (currKingdom.id != this.firstKingdom.id){
				if(IsCompatibleRelationship(currKingdom) && IsCompatibleKingdomType(currKingdom) 
                    && IsCompatibleMilitaryStrength(currKingdom)){

					allKingdomCandidates.Add (currKingdom);
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
	public void IsEligibleForEvent(){
		if(this.firstKingdom.HasActiveEvent(this.eventToCreate.eventType) && this.secondKingdom.HasActiveEvent(this.eventToCreate.eventType)){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
//		List<GameEvent> allRaids = EventManager.Instance.GetEventsOfType (EVENT_TYPES.RAID).Where (x => x.isActive).ToList ();
//		List<GameEvent> allBorderConflicts = EventManager.Instance.GetEventsOfType (EVENT_TYPES.BORDER_CONFLICT).Where (x => x.isActive).ToList ();
//		List<GameEvent> allDiplomaticCrisis = EventManager.Instance.GetEventsOfType (EVENT_TYPES.DIPLOMATIC_CRISIS).Where (x => x.isActive).ToList ();
//		List<GameEvent> allStateVisit = EventManager.Instance.GetEventsOfType (EVENT_TYPES.STATE_VISIT).Where (x => x.isActive).ToList ();
//		this.allUnwantedEvents = allBorderConflicts.Concat (allDiplomaticCrisis).Concat(allRaids).Concat(allStateVisit).ToList ();
//
//		if (this.allUnwantedEvents.Count > 0) {
//			if (SearchForEligibility (this.firstKingdom, this.secondKingdom, this.allUnwantedEvents)) {
//				Task.current.Succeed ();
//			}else{
//				Task.current.Fail ();
//			}
//		}else{
//			Task.current.Succeed ();
//		}

	}
	[Task]
	public void StartAnEvent(){

		Task.current.Succeed ();
	}

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
		if(counter == 2){
			return false;
		}else{
			return true;
		}
	}

	private bool IsCompatibleRelationship(Kingdom targetKingdom){
		if (this.eventToCreate.relationshipTargets.Length > 0) {
			KingdomRelationship relationship = this.firstKingdom.GetRelationshipWithKingdom (targetKingdom);
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
}
