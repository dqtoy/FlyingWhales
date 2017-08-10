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

    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused = true;

    private const float X1_SPEED = 2f;
    private const float X2_SPEED = 1f;
    private const float X4_SPEED = 0.3f;

    private float timeElapsed;

    private void Awake(){
		Instance = this;
		this.days = 1;
		this.month = 1;
		this.timeElapsed = 0f;
	}

	private void FixedUpdate(){
		if (!isPaused) {
			this.timeElapsed += Time.deltaTime * 1f;
			if(this.timeElapsed >= this.progressionSpeed){
				this.timeElapsed = 0f;
				this.WeekEnded ();
			}
		}
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
		UIManager.Instance.SetProgressionSpeed1X();
		UIManager.Instance.x1Btn.SetAsClicked();
		EventManager.Instance.onUpdateUI.Invoke();
        SetPausedState(false);
	}

	public void TogglePause(){
		this.isPaused = !this.isPaused;
	}

	public void SetPausedState(bool isPaused){
		this.isPaused = isPaused;
	}

    /*
     * Set day progression speed to 1x, 2x of 4x
     * */
	public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        currProgressionSpeed = progSpeed;
        float speed = X1_SPEED;
        if (progSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		this.progressionSpeed = speed;
	}

    /*
     * Function that triggers daily actions
     * */
	public void WeekEnded(){
//		TriggerRequestPeace();
		EventManager.Instance.onCitizenTurnActions.Invoke ();
		EventManager.Instance.onCityEverydayTurnActions.Invoke ();
		EventManager.Instance.onWeekEnd.Invoke();
		BehaviourTreeManager.Instance.Tick ();
		EventManager.Instance.onUpdateUI.Invoke();

		this.days += 1;
		if (days > daysInMonth[this.month]) {
			this.days = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}
	}

	/*private void TriggerRequestPeace(){
		List<GameEvent> allWars = EventManager.Instance.GetEventsOfType(EVENT_TYPES.KINGDOM_WAR).Where(x => x.isActive).ToList();
		for (int i = 0; i < allWars.Count; i++) {
			War currentWar = (War)allWars[i];
			//For Kingdom 1
			if (currentWar.kingdom1Rel.monthToMoveOnAfterRejection == MONTH.NONE 
				&& KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currentWar.kingdom1, currentWar.kingdom2) == null) {
				int kingdom1ChanceToRequestPeace = 0;
				if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 100) {
					kingdom1ChanceToRequestPeace = 6;
					if (currentWar.kingdom1.king.hasTrait(TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 8;
					} else if (currentWar.kingdom1.king.hasTrait(TRAIT.WARMONGER)) {
						kingdom1ChanceToRequestPeace = 4;
					}
				} else if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 75) {
					kingdom1ChanceToRequestPeace = 4;
					if (currentWar.kingdom1.king.hasTrait(TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 6;
					} else if (currentWar.kingdom1.king.hasTrait(TRAIT.WARMONGER)) {
						kingdom1ChanceToRequestPeace = 2;
					}
				} else if (currentWar.kingdom1Rel.kingdomWar.exhaustion >= 50) {
					kingdom1ChanceToRequestPeace = 2;
					if (currentWar.kingdom1.king.hasTrait(TRAIT.PACIFIST)) {
						kingdom1ChanceToRequestPeace = 4;
					} else if (currentWar.kingdom1.king.hasTrait(TRAIT.WARMONGER)) {
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
							RelationshipKings rel = currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom1.king);
							if (rel.lordRelationship == RELATIONSHIP_STATUS.ENEMY || rel.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
								kingdom1Enemies.Add(currentKingdom);
							}
						}
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
									if (kingdom1Enemies [j].king.hasTrait(TRAIT.SCHEMING)) {
										chanceToSabotage = 10;
									} else if (kingdom1Enemies [j].king.hasTrait(TRAIT.HONEST)) {
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
									if (kingdom1Enemies [j].king.hasTrait(TRAIT.SCHEMING)) {
										chanceToAssassinate = 6;
									} else if (kingdom1Enemies [j].king.hasTrait(TRAIT.HONEST)) {
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
					if (currentWar.kingdom2.king.hasTrait(TRAIT.PACIFIST)) {
						kingdom2ChanceToRequestPeace = 8;
					} else if (currentWar.kingdom2.king.hasTrait(TRAIT.WARMONGER)) {
						kingdom2ChanceToRequestPeace = 4;
					}
				} else if (currentWar.kingdom2Rel.kingdomWar.exhaustion >= 75) {
					kingdom2ChanceToRequestPeace = 4;
					if (currentWar.kingdom2.king.hasTrait(TRAIT.PACIFIST)) {
						kingdom2ChanceToRequestPeace = 6;
					} else if (currentWar.kingdom2.king.hasTrait(TRAIT.WARMONGER)) {
						kingdom2ChanceToRequestPeace = 2;
					}
				} else if (currentWar.kingdom2Rel.kingdomWar.exhaustion >= 50) {
					kingdom2ChanceToRequestPeace = 2;
					if (currentWar.kingdom2.king.hasTrait(TRAIT.PACIFIST)) {
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
							RelationshipKings rel = currentKingdom.king.GetRelationshipWithCitizen (currentWar.kingdom2.king);
							if (rel.lordRelationship == RELATIONSHIP_STATUS.ENEMY || rel.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
								kingdom2Enemies.Add(currentKingdom);
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
									if (kingdom2Enemies [j].king.hasTrait(TRAIT.SCHEMING)) {
										chanceToSabotage = 10;
									} else if (kingdom2Enemies [j].king.hasTrait(TRAIT.HONEST)) {
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

					//Check For Assassination Event
					for (int j = 0; j < kingdom2Enemies.Count; j++) {
						if (kingdom2Enemies [j].id != currentWar.kingdom1.id) {
							RelationshipKings relationshipOfEnemyWithWarEnemy = kingdom2Enemies [j].king.GetRelationshipWithCitizen (currentWar.kingdom1.king);
							if (relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.ALLY ||
							   relationshipOfEnemyWithWarEnemy.lordRelationship != RELATIONSHIP_STATUS.FRIEND) {
								List<Citizen> assassins = kingdom2Enemies [j].GetAllCitizensOfType (ROLE.SPY).Where (x => !((Spy)x.assignedRole).inAction).ToList ();
								if (assassins.Count > 0) {
									int chanceToAssassinate = 3;
									if (kingdom2Enemies [j].king.hasTrait(TRAIT.SCHEMING)) {
										chanceToAssassinate = 6;
									} else if (kingdom2Enemies [j].king.hasTrait(TRAIT.HONEST)) {
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
	}*/

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

	/*private General GetGeneral(Kingdom kingdom){
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
//			Debug.Log (kingdom.king.name + " CAN'T SEND GENERAL BECAUSE THERE IS NONE!");
			return null;
		}
	}*/
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
