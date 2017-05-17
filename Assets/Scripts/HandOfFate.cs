using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class HandOfFate : MonoBehaviour {
	public Kingdom firstKingdom;
	public Kingdom secondKingdom;
	public int compatibilityValue;
	public bool isAdjacent;
	public int raidChance;
	public int borderConflictChance;
	public int diplomaticCrisisChance;
	public int admirationChance;
	public List<GameEvent> allUnwantedEvents;

	void Awake(){
		this.firstKingdom = null;
		this.secondKingdom = null;
		this.compatibilityValue = 0;
		this.isAdjacent = false;
		this.raidChance = 0;
		this.borderConflictChance = 0;
		this.diplomaticCrisisChance = 0;
		this.admirationChance = 0;
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
		this.compatibilityValue = 0;
		this.isAdjacent = false;
		this.raidChance = 0;
		this.borderConflictChance = 0;
		this.diplomaticCrisisChance = 0;
		this.admirationChance = 0;
		this.allUnwantedEvents.Clear ();
		Task.current.Succeed ();
	}
	[Task]
	public void SetFirstRandomKingdom(){
		this.firstKingdom = KingdomManager.Instance.allKingdoms [UnityEngine.Random.Range (0, KingdomManager.Instance.allKingdoms.Count)];
		if(this.firstKingdom == null){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}

	[Task]
	public void SetSecondRandomKingdom(){
		List<Kingdom> adjacentKingdoms = this.firstKingdom.GetAdjacentKingdoms ();
		if(adjacentKingdoms.Count > 0){
//			int chance = UnityEngine.Random.Range (0, 100);
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
//			if(chance < 60){
				this.secondKingdom = adjacentKingdoms [UnityEngine.Random.Range (0, adjacentKingdoms.Count)];
			}else{
				this.secondKingdom = KingdomManager.Instance.GetRandomKingdomExcept (this.firstKingdom);
			}
			Task.current.Succeed ();
		}else{
			this.secondKingdom = KingdomManager.Instance.GetRandomKingdomExcept (this.firstKingdom);
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
	[Task]
	public void SetCompatibilityValue(){
		int[] firstKingdomHoroscope = this.firstKingdom.horoscope.Concat (this.firstKingdom.king.horoscope).ToArray();
		int[] secondKingdomHoroscope = this.secondKingdom.horoscope.Concat (this.secondKingdom.king.horoscope).ToArray();
		int count = firstKingdomHoroscope.Length;

		for(int i = 0; i < count; i++){
			if(firstKingdomHoroscope[i] == secondKingdomHoroscope[i]){
				this.compatibilityValue += 1;
			}
		}
		Task.current.Succeed ();
	}
	[Task]
	public void SetAdjacencyValue(){
		this.isAdjacent = this.firstKingdom.IsKingdomAdjacentTo (this.secondKingdom);
		Task.current.Succeed ();
	}

	[Task]
	public void SetEventChances(){
		if(this.isAdjacent){
			if(this.compatibilityValue == 0 || this.compatibilityValue == 1){
				this.raidChance = 50;
				this.borderConflictChance = 35;
				this.diplomaticCrisisChance = 15;
				this.admirationChance = 0;
			}else if(this.compatibilityValue == 2 || this.compatibilityValue == 3){
				this.raidChance = 35;
				this.borderConflictChance = 25;
				this.diplomaticCrisisChance = 5;
				this.admirationChance = 35;
			}else if(this.compatibilityValue == 4 || this.compatibilityValue == 5){
				this.raidChance = 20;
				this.borderConflictChance = 10;
				this.diplomaticCrisisChance = 0;
				this.admirationChance = 70;
			}
		}else{
			if(this.compatibilityValue == 0 || this.compatibilityValue == 1){
				this.raidChance = 80;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 20;
				this.admirationChance = 0;
			}else if(this.compatibilityValue == 2 || this.compatibilityValue == 3){
				this.raidChance = 55;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 10;
				this.admirationChance = 35;
			}else if(this.compatibilityValue == 4 || this.compatibilityValue == 5){
				this.raidChance = 25;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 5;
				this.admirationChance = 70;
			}
		}
		Task.current.Succeed ();
	}
	[Task]
	public void IsEligibleForEvent(){
		List<GameEvent> allRaids = EventManager.Instance.GetEventsOfType (EVENT_TYPES.RAID).Where (x => x.isActive).ToList ();
		List<GameEvent> allBorderConflicts = EventManager.Instance.GetEventsOfType (EVENT_TYPES.BORDER_CONFLICT).Where (x => x.isActive).ToList ();
		List<GameEvent> allDiplomaticCrisis = EventManager.Instance.GetEventsOfType (EVENT_TYPES.DIPLOMATIC_CRISIS).Where (x => x.isActive).ToList ();
		List<GameEvent> allAdmiration = EventManager.Instance.GetEventsOfType (EVENT_TYPES.ADMIRATION).Where (x => x.isActive).ToList ();
		this.allUnwantedEvents = allRaids.Concat (allBorderConflicts).Concat (allDiplomaticCrisis).Concat (allAdmiration).ToList ();

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
		int chance = UnityEngine.Random.Range (0, 100);
		int raid = this.raidChance;
		int borderConflict = this.raidChance + this.borderConflictChance;
		int diplomaticCrisis = this.raidChance + this.borderConflictChance + this.diplomaticCrisisChance;
		int admiration = this.raidChance + this.borderConflictChance + this.diplomaticCrisisChance + this.admirationChance;

		if(chance < raid){
			Debug.Log ("CREATING RAID EVENT...");
			CreateRaidEvent ();
		}else if(chance >= raid && chance < borderConflict){
			Debug.Log ("CREATING BORDER CONFLICT EVENT...");
			CreateBorderConflictEvent ();
		}else if(chance >= borderConflict && chance < diplomaticCrisis){
			Debug.Log ("CREATING DIPLOMATIC CRISIS EVENT...");
			CreateDiplomaticCrisisEvent ();
		}else if(chance >= diplomaticCrisis && chance < admiration){
			Debug.Log ("CREATING ADMIRATION EVENT...");
			CreateAdmirationEvent ();
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
	private void CreateBorderConflictEvent(){
		BorderConflict();
	}

	private void CreateDiplomaticCrisisEvent(){
		DiplomaticCrisis ();
	}

	private void CreateAdmirationEvent(){
		Admiration ();
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
	private void Admiration(){
		Debug.Log ("Admiration FROM HAND OF FATE");
		Citizen startedBy = this.secondKingdom.king;
		Admiration admiration = new Admiration(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
		EventManager.Instance.AddEventToDictionary(admiration);
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
		}else if(gameEvent is Admiration) {
			if(((Admiration)gameEvent).kingdom1.id == kingdom1.id || ((Admiration)gameEvent).kingdom2.id == kingdom1.id){
				counter += 1;
			}
			if(((Admiration)gameEvent).kingdom1.id == kingdom2.id || ((Admiration)gameEvent).kingdom2.id == kingdom2.id){
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
}
