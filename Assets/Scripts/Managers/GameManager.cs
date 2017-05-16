using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public static int[] daysInMonth = {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
	public int month;
	public int days;
	public int year;

	public float progressionSpeed = 1f;
	public bool isPaused = false;

	void Awake(){
		Instance = this;
		this.days = 1;
		this.month = 1;
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
//		InvokeRepeating ("WeekEnded", 0f, 1f);
		UIManager.Instance.SetProgressionSpeed1X();
		UIManager.Instance.x1Btn.SetAsClicked();
//		this.WeekEnded();
		EventManager.Instance.onUpdateUI.Invoke();
		StartCoroutine(WeekProgression());
	}

	public void TogglePause(){
		this.isPaused = !this.isPaused;
	}

	public void SetPausedState(bool isPaused){
		this.isPaused = isPaused;
	}

	public void SetProgressionSpeed(float speed){
		this.progressionSpeed = speed;
	}

	IEnumerator WeekProgression(){
		while (true) {
			yield return new WaitForSeconds (progressionSpeed);
			if (!isPaused) {
				this.WeekEnded ();
			}
		}
	}

	public void WeekEnded(){
		this.days += 1;
		if (days > daysInMonth[this.month]) {
			this.days = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}

//		TriggerBorderConflict ();
//		TriggerRaid();
		TriggerRequestPeace();
		EventManager.Instance.onCitizenTurnActions.Invoke ();
		EventManager.Instance.onCityEverydayTurnActions.Invoke ();
//		EventManager.Instance.onCitizenMove.Invoke (false);
		EventManager.Instance.onWeekEnd.Invoke();
		BehaviourTreeManager.Instance.Tick ();
		EventManager.Instance.onUpdateUI.Invoke();
	}
	private void TriggerRaid(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 15){
//		if(chance < 3){
			Raid ();
		}
	}
	private void Raid(){
		Debug.Log ("Raid");
		Kingdom raiderOfTheLostArc = KingdomManager.Instance.allKingdoms [UnityEngine.Random.Range (0, KingdomManager.Instance.allKingdoms.Count)];
		General general = GetGeneral(raiderOfTheLostArc);
		City city = GetRaidedCity(general);
		if(general != null && city != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, raiderOfTheLostArc.king, city, general);
			EventManager.Instance.AddEventToDictionary (raid);
		}
	}
	private void TriggerBorderConflict(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 15){
//		if(chance < 1){
			BorderConflict ();
		}
	}
	private void BorderConflict(){
		Debug.Log ("Border Conflict");
		List<GameEvent> allBorderConflicts = EventManager.Instance.GetEventsOfType(EVENT_TYPES.BORDER_CONFLICT);
		List<Kingdom> shuffledKingdoms = Utilities.Shuffle (KingdomManager.Instance.allKingdoms);

		bool isEligible = false;
		for(int i = 0; i < shuffledKingdoms.Count; i++){
			for(int j = 0; j < shuffledKingdoms[i].relationshipsWithOtherKingdoms.Count; j++){
				if(!shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].isAtWar && shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].isAdjacent){
					if(allBorderConflicts != null){
						if(SearchForEligibility(shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom, allBorderConflicts)){
							//Add BorderConflict
							Citizen startedBy = null;
							if (Random.Range (0, 2) == 0) {
								startedBy = shuffledKingdoms[i].king;
							} else {
								startedBy = shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom.king;
							}
							BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom);
							EventManager.Instance.AddEventToDictionary(borderConflict);
							isEligible = true;
							break;
						}
					}else{
						//Add BorderConflict
						Citizen startedBy = null;
						if (Random.Range (0, 2) == 0) {
							startedBy = shuffledKingdoms[i].king;
						} else {
							startedBy = shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom.king;
						}
						BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].targetKingdom);
						EventManager.Instance.AddEventToDictionary(borderConflict);
						isEligible = true;
						break;
					}

				}
			}
			if(isEligible){
				break;
			}
		}
	}

	private void TriggerRequestPeace(){
		List<GameEvent> allWars = EventManager.Instance.GetEventsOfType(EVENT_TYPES.KINGDOM_WAR).Where(x => x.isActive).ToList();
		for (int i = 0; i < allWars.Count; i++) {
			War currentWar = (War)allWars[i];
			//For Kingdom 1
			if (currentWar.kingdom1Rel.monthToMoveOnAfterRejection == MONTH.NONE 
				&& KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currentWar.kingdom1, currentWar.kingdom2) == null) {
				int kingdom1ChanceToRequestPeace = 0;
				if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 100) {
					kingdom1ChanceToRequestPeace = 6;
					if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 8;
					} else if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
						kingdom1ChanceToRequestPeace = 4;
					}
				} else if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 75) {
					kingdom1ChanceToRequestPeace = 4;
					if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 6;
					} else if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
						kingdom1ChanceToRequestPeace = 2;
					}
				} else if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 50) {
					kingdom1ChanceToRequestPeace = 2;
					if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 4;
					} else if (currentWar.kingdom1.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
						kingdom1ChanceToRequestPeace = 0;
					}
				}

				int chance = Random.Range (0, 100);
				if (chance < kingdom1ChanceToRequestPeace) {
					List<Citizen> kingdom1Saboteurs = new List<Citizen>();
					List<Kingdom> kingdom1Enemies = new List<Kingdom>();
					for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
						Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
						if (currentKingdom.id != currentWar.kingdom1.id) {
							try{
								RelationshipKings rel = currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom1.king);
								if (rel.lordRelationship == RELATIONSHIP_STATUS.ENEMY || rel.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
									kingdom1Enemies.Add(currentKingdom);
								}
							}catch(System.Exception e){
								Debug.LogError ("CurrentKingdom King " + currentKingdom.king);
								Debug.LogError ("Current War Kingdom 1 King: " + currentWar.kingdom1.king);
								Debug.LogError ("Relationship: " + currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom1.king));
							}
						}

//						if (currentKingdom.id != currentWar.kingdom1.id && currentKingdom.id != currentWar.kingdom2.id) {
//							if (currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom1.king).lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
//								currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom1.king).lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
//								kingdom1Enemies.Add(currentKingdom);
//							}
//						}
					}
					List<Citizen> envoys = null;
					for (int j = 0; j < kingdom1Enemies.Count; j++) {
						if (kingdom1Enemies [j].id != currentWar.kingdom2.id) {
							RelationshipKings relationshipOfEnemyWithWarEnemy = kingdom1Enemies [j].king.GetRelationshipWithCitizen (currentWar.kingdom2.king);
							if (relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.ALLY ||
							   relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.FRIEND) {
								envoys = kingdom1Enemies [j].GetAllCitizensOfType (ROLE.ENVOY).Where (x => !((Envoy)x.assignedRole).inAction).ToList ();
								if (envoys.Count > 0) {
									int chanceToSabotage = 5;
									if (kingdom1Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
										chanceToSabotage = 10;
									} else if (kingdom1Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
										chanceToSabotage = 0;
									}
									int chanceForSabotage = Random.Range (0, 100);
									if (chanceForSabotage < chanceToSabotage) {
										kingdom1Saboteurs.Add (envoys [0]);
									}
								}
							}
						}
					}
					Citizen citizenToSend = null;
					envoys = currentWar.kingdom1.GetAllCitizensOfType(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
					if (envoys.Count > 0) {
						citizenToSend = envoys[0];
					} else {
						citizenToSend = currentWar.kingdom1.king;
					}

					currentWar.CreateRequestPeaceEvent(currentWar.kingdom1, citizenToSend, kingdom1Saboteurs);
//					RequestPeace newRequestPeace = new RequestPeace (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, currentWar.kingdom1.king,
//						citizenToSend, currentWar.kingdom2, kingdom1Saboteurs);

					//Check For Assassination Event
					for (int j = 0; j < kingdom1Enemies.Count; j++) {
						if (kingdom1Enemies [j].id != currentWar.kingdom2.id) {
							RelationshipKings relationshipOfEnemyWithWarEnemy = kingdom1Enemies [j].king.GetRelationshipWithCitizen (currentWar.kingdom2.king);
							if (relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.ALLY ||
							   relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.FRIEND) {
								List<Citizen> assassins = kingdom1Enemies [j].GetAllCitizensOfType (ROLE.SPY).Where (x => !((Spy)x.assignedRole).inAction).ToList ();
								if (assassins.Count > 0) {
									int chanceToAssassinate = 3;
									if (kingdom1Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
										chanceToAssassinate = 6;
									} else if (kingdom1Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
										chanceToAssassinate = 0;
									}
									int chanceForAssassination = Random.Range (0, 100);
									if (chance < chanceToAssassinate) {
										Assassination newAssassination = new Assassination (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
											kingdom1Enemies [j].king, citizenToSend, assassins [0], 
											KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currentWar.kingdom1, currentWar.kingdom2));
									}
								}
							}
						}
					}

				}
			}

			//For Kingdom 2
			if (currentWar.kingdom2Rel.monthToMoveOnAfterRejection == MONTH.NONE
				&& KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currentWar.kingdom2, currentWar.kingdom1) == null) {
				int kingdom2ChanceToRequestPeace = 0;
				if (currentWar.kingdom2Rel.kingdomWar.exhaustion >= 100) {
					kingdom2ChanceToRequestPeace = 6;
					if (currentWar.kingdom2.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom2ChanceToRequestPeace = 8;
					} else if (currentWar.kingdom2.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
						kingdom2ChanceToRequestPeace = 4;
					}
				} else if (currentWar.kingdom2Rel.kingdomWar.exhaustion >= 75) {
					kingdom2ChanceToRequestPeace = 4;
					if (currentWar.kingdom2.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom2ChanceToRequestPeace = 6;
					} else if (currentWar.kingdom2.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
						kingdom2ChanceToRequestPeace = 2;
					}
				} else if (currentWar.kingdom2Rel.kingdomWar.exhaustion >= 50) {
					kingdom2ChanceToRequestPeace = 2;
					if (currentWar.kingdom2.king.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
						kingdom2ChanceToRequestPeace = 4;
					}
				}
				int chance = Random.Range (0, 100);
				if (chance < kingdom2ChanceToRequestPeace) {
					List<Citizen> kingdom2Saboteurs = new List<Citizen>();
					List<Kingdom> kingdom2Enemies = new List<Kingdom>();
					for (int j = 0; j < KingdomManager.Instance.allKingdoms.Count; j++) {
						Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[j];
						if (currentKingdom.id != currentWar.kingdom2.id) {
							try{
								RelationshipKings rel = currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom2.king);
								if (rel.lordRelationship == RELATIONSHIP_STATUS.ENEMY || rel.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
									kingdom2Enemies.Add(currentKingdom);
								}
							}catch(System.Exception e){
								Debug.LogError ("CurrentKingdom King " + currentKingdom.king);
								Debug.LogError ("Current War Kingdom 2 King: " + currentWar.kingdom2.king);
								Debug.LogError ("Relationship: " + currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom2.king));
							}
						}
					}

					List<Citizen> envoys = null;
					for (int j = 0; j < kingdom2Enemies.Count; j++) {
						if (kingdom2Enemies [j].id != currentWar.kingdom1.id) {
							RelationshipKings relationshipOfEnemyWithWarEnemy = kingdom2Enemies [j].king.GetRelationshipWithCitizen (currentWar.kingdom1.king);
							if (relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.ALLY ||
							   relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.FRIEND) {
								envoys = kingdom2Enemies [j].GetAllCitizensOfType (ROLE.ENVOY).Where (x => !((Envoy)x.assignedRole).inAction).ToList ();
								if (envoys.Count > 0) {
									int chanceToSabotage = 5;
									if (kingdom2Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
										chanceToSabotage = 10;
									} else if (kingdom2Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
										chanceToSabotage = 0;
									}
									int chanceForSabotage = Random.Range (0, 100);
									if (chanceForSabotage < chanceToSabotage) {
										kingdom2Saboteurs.Add (envoys [0]);
									}
								}
							}
						}
					}
					Citizen citizenToSend = null;
					envoys = currentWar.kingdom2.GetAllCitizensOfType(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
					if (envoys.Count > 0) {
						citizenToSend = envoys[0];
					} else {
						citizenToSend = currentWar.kingdom2.king;

					}

					currentWar.CreateRequestPeaceEvent(currentWar.kingdom2, citizenToSend, kingdom2Saboteurs);
//					RequestPeace newRequestPeace = new RequestPeace (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, currentWar.kingdom2.king,
//						citizenToSend, currentWar.kingdom1, kingdom2Saboteurs);

					//Check For Assassination Event
					for (int j = 0; j < kingdom2Enemies.Count; j++) {
						if (kingdom2Enemies [j].id != currentWar.kingdom1.id) {
							RelationshipKings relationshipOfEnemyWithWarEnemy = kingdom2Enemies [j].king.GetRelationshipWithCitizen (currentWar.kingdom1.king);
							if (relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.ALLY ||
							   relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.FRIEND) {
								List<Citizen> assassins = kingdom2Enemies [j].GetAllCitizensOfType (ROLE.SPY).Where (x => !((Spy)x.assignedRole).inAction).ToList ();
								if (assassins.Count > 0) {
									int chanceToAssassinate = 3;
									if (kingdom2Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
										chanceToAssassinate = 6;
									} else if (kingdom2Enemies [j].king.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
										chanceToAssassinate = 0;
									}
									int chanceForAssassination = Random.Range (0, 100);
									if (chance < chanceToAssassinate) {
										Assassination newAssassination = new Assassination (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
											kingdom2Enemies [j].king, citizenToSend, assassins [0], 
											KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currentWar.kingdom1, currentWar.kingdom2));
									}
								}
							}
						}
					}

				}
			}
		}
	}

	internal bool SearchForEligibility (Kingdom kingdom1, Kingdom kingdom2, List<GameEvent> borderConflicts){
		for(int i = 0; i < borderConflicts.Count; i++){
			if(!IsEligibleForConflict(kingdom1,kingdom2,((BorderConflict)borderConflicts[i]))){
				return false;
			}
		}
		return true;
	}
	private bool IsEligibleForConflict(Kingdom kingdom1, Kingdom kingdom2, BorderConflict borderConflict){
		int counter = 0;
		if(borderConflict.kingdom1.id == kingdom1.id || borderConflict.kingdom2.id == kingdom1.id){
			counter += 1;
		}
		if(borderConflict.kingdom1.id == kingdom2.id || borderConflict.kingdom2.id == kingdom2.id){
			counter += 1;
		}
		if(counter == 2){
			if(borderConflict.isActive){
				return false;
			}
			return true;
		}else{
			return true;
		}
	}

	public List<Citizen> GetAllCitizensOfType(ROLE role){
		List<Citizen> allCitizensOfType = new List<Citizen>();
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			allCitizensOfType.AddRange (KingdomManager.Instance.allKingdoms[i].GetAllCitizensOfType (role));
		}
		return allCitizensOfType;
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
//		List<City> adjacentCities = new List<City> ();
//		for(int i = 0; i < general.citizen.city.hexTile.connectedTiles.Count; i++){
//			if(general.citizen.city.hexTile.connectedTiles[i].isOccupied){
//				if(general.citizen.city.hexTile.connectedTiles[i].city.kingdom.id != general.citizen.city.kingdom.id){
//					adjacentCities.Add (general.citizen.city.hexTile.connectedTiles[i].city);
//				}
//			}
//		}

		if(general.citizen.city.kingdom.adjacentCitiesFromOtherKingdoms.Count > 0){
			return general.citizen.city.kingdom.adjacentCitiesFromOtherKingdoms [UnityEngine.Random.Range (0, general.citizen.city.kingdom.adjacentCitiesFromOtherKingdoms.Count)];
		}else{
			return null;
		}
	}
}
