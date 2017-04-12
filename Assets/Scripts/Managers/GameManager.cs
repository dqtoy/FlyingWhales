using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public int month;
	public int week;
	public int year;

	public float progressionSpeed = 1f;
	public bool isPaused = false;

	void Awake(){
		Instance = this;
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
//		InvokeRepeating ("WeekEnded", 0f, 1f);
		UIManager.Instance.SetProgressionSpeed1X();
		UIManager.Instance.x1Btn.OnClick();
		StartCoroutine(WeekProgression());
	}

	public void TogglePause(){
		this.isPaused = !this.isPaused;
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
		this.week += 1;
		if (week > 4) {
			this.week = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}
		TriggerBorderConflict ();
		TriggerRaid();
		EventManager.Instance.onCitizenTurnActions.Invoke ();
		EventManager.Instance.onCityEverydayTurnActions.Invoke ();
		EventManager.Instance.onCitizenMove.Invoke ();
		EventManager.Instance.onWeekEnd.Invoke();
	}
	private void TriggerRaid(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 3){
			Raid ();
		}
	}
	private void Raid(){
		Debug.Log ("Raid");
		Kingdom raiderOfTheLostArc = KingdomManager.Instance.allKingdoms [UnityEngine.Random.Range (0, KingdomManager.Instance.allKingdoms.Count)];
		General general = GetGeneral(raiderOfTheLostArc);
		City city = GetRaidedCity(general);
		if(general != null && city != null){
			Raid raid = new Raid(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, raiderOfTheLostArc.king, city, general);
			EventManager.Instance.AddEventToDictionary (raid);
		}
	}
	private void TriggerBorderConflict(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 1){
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
						if(SearchForEligibility(shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].objectInRelationship, allBorderConflicts)){
							//Add BorderConflict
							BorderConflict borderConflict = new BorderConflict(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, null, shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].objectInRelationship);
							EventManager.Instance.AddEventToDictionary(borderConflict);
							isEligible = true;
							break;
						}
					}else{
						//Add BorderConflict
						BorderConflict borderConflict = new BorderConflict(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, null, shuffledKingdoms[i], shuffledKingdoms[i].relationshipsWithOtherKingdoms[j].objectInRelationship);
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
			return false;
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
		List<City> adjacentCities = new List<City> ();
		for(int i = 0; i < general.citizen.city.hexTile.connectedTiles.Count; i++){
			if(general.citizen.city.hexTile.connectedTiles[i].isOccupied){
				if(general.citizen.city.hexTile.connectedTiles[i].city.kingdom.id != general.citizen.city.kingdom.id){
					adjacentCities.Add (general.citizen.city.hexTile.connectedTiles[i].city);
				}
			}

		}

		if(adjacentCities.Count > 0){
			return adjacentCities [UnityEngine.Random.Range (0, adjacentCities.Count)];
		}else{
			return null;
		}
	}
}
