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

	void Awake(){
		ResetValues ();
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
			this.secondKingdom = adjacentKingdoms [UnityEngine.Random.Range (0, adjacentKingdoms.Count)];
			Task.current.Succeed ();
		}else{
			this.secondKingdom = KingdomManager.Instance.GetRandomKingdomExcept (this.firstKingdom);
			if(this.secondKingdom == null){
				Task.current.Fail ();
			}else{
				Task.current.Succeed ();
			}
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
			switch(this.compatibilityValue){
			case 0:
				this.raidChance = 50;
				this.borderConflictChance = 35;
				this.diplomaticCrisisChance = 15;
				this.admirationChance = 0;
				break;
			case 1:
				this.raidChance = 45;
				this.borderConflictChance = 35;
				this.diplomaticCrisisChance = 10;
				this.admirationChance = 10;
				break;
			case 2:
				this.raidChance = 40;
				this.borderConflictChance = 30;
				this.diplomaticCrisisChance = 5;
				this.admirationChance = 25;
				break;
			case 3:
				this.raidChance = 25;
				this.borderConflictChance = 20;
				this.diplomaticCrisisChance = 5;
				this.admirationChance = 50;
				break;
			case 4:
				this.raidChance = 20;
				this.borderConflictChance = 10;
				this.diplomaticCrisisChance = 0;
				this.admirationChance = 70;
				break;
			case 5:
				this.raidChance = 7;
				this.borderConflictChance = 3;
				this.diplomaticCrisisChance = 0;
				this.admirationChance = 90;
				break;
			}	
		}else{
			switch(this.compatibilityValue){
			case 0:
				this.raidChance = 80;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 20;
				this.admirationChance = 0;
				break;
			case 1:
				this.raidChance = 75;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 15;
				this.admirationChance = 10;
				break;
			case 2:
				this.raidChance = 65;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 10;
				this.admirationChance = 25;
				break;
			case 3:
				this.raidChance = 40;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 10;
				this.admirationChance = 50;
				break;
			case 4:
				this.raidChance = 25;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 5;
				this.admirationChance = 70;
				break;
			case 5:
				this.raidChance = 7;
				this.borderConflictChance = 0;
				this.diplomaticCrisisChance = 3;
				this.admirationChance = 90;
				break;
			}	
		}
		Task.current.Succeed ();
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
		General general = GetGeneral(this.firstKingdom);
		City city = GetRaidedCity(general);
		if(general != null && city != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.firstKingdom.king, city, general);
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
	private City GetRaidedCity(General general){
		if(general == null){
			return null;
		}
		if(this.isAdjacent){
			List<City> adjacentCities = general.citizen.city.kingdom.adjacentCitiesFromOtherKingdoms.Where (x => x.kingdom.id == this.secondKingdom.id).ToList();
			if(adjacentCities.Count > 0){
				return adjacentCities [UnityEngine.Random.Range (0, adjacentCities.Count)];
			}else{
				if(this.secondKingdom.cities.Count > 0){
					return this.secondKingdom.cities [UnityEngine.Random.Range (0, this.secondKingdom.cities.Count)];
				}else{
					return null;
				}
			}
		}else{
			if(this.secondKingdom.cities.Count > 0){
				return this.secondKingdom.cities [UnityEngine.Random.Range (0, this.secondKingdom.cities.Count)];
			}else{
				return null;
			}
		}

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
		List<GameEvent> allBorderConflicts = EventManager.Instance.GetEventsOfType(EVENT_TYPES.BORDER_CONFLICT);

		if(allBorderConflicts != null){
			if(SearchForEligibility(this.firstKingdom, this.secondKingdom, allBorderConflicts)){
				//Add BorderConflict
				Citizen startedBy = this.firstKingdom.king;
				BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.firstKingdom, this.secondKingdom);
				EventManager.Instance.AddEventToDictionary(borderConflict);
			}
		}else{
			//Add BorderConflict
			Citizen startedBy = this.firstKingdom.king;
			BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.firstKingdom, this.secondKingdom);
			EventManager.Instance.AddEventToDictionary(borderConflict);
		}

	}
	private void DiplomaticCrisis(){
		Debug.Log ("Diplomatic Crisis FROM HAND OF FATE");
		List<GameEvent> allDiplomaticCrisis = EventManager.Instance.GetEventsOfType(EVENT_TYPES.DIPLOMATIC_CRISIS);

		if(allDiplomaticCrisis != null){
			if(SearchForEligibility(this.firstKingdom, this.secondKingdom, allDiplomaticCrisis)){
				Citizen startedBy = this.secondKingdom.king;
				DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
				EventManager.Instance.AddEventToDictionary(diplomaticCrisis);
			}
		}else{
			Citizen startedBy = this.secondKingdom.king;
			DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
			EventManager.Instance.AddEventToDictionary(diplomaticCrisis);
		}

	}
	private void Admiration(){
		Debug.Log ("Admiration FROM HAND OF FATE");
		List<GameEvent> allAdmiration = EventManager.Instance.GetEventsOfType(EVENT_TYPES.ADMIRATION);

		if(allAdmiration != null){
			if(SearchForEligibility(this.firstKingdom, this.secondKingdom, allAdmiration)){
				Citizen startedBy = this.secondKingdom.king;
				Admiration admiration = new Admiration(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
				EventManager.Instance.AddEventToDictionary(admiration);
			}
		}else{
			Citizen startedBy = this.secondKingdom.king;
			Admiration admiration = new Admiration(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, this.secondKingdom, this.firstKingdom);
			EventManager.Instance.AddEventToDictionary(admiration);
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
		}

		if(counter == 2){
			if(gameEvent.isActive){
				return false;
			}
			return true;
		}else{
			return true;
		}
	}
}
