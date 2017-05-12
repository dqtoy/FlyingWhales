using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class GeneralObject : MonoBehaviour {
	public General general;
	public PandaBehaviour pandaBehaviour;
	public TextMesh textMesh;
	public SpriteRenderer kingdomIndicator;
	public GameObject deathIcon;
	public List<HexTile> path;

	public bool isIdle;
	public bool isRoaming = false;
	public bool isPreviousRoaming = false;
	public bool isSearchingForTarget;
	public bool collidedWithHostile;
	public General otherGeneral;

	public float speed;
	bool isMoving = false;
	Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();

//	void Update(){
//		if(isMoving){
//			if(this.targetPosition != null){
//				float step = speed * Time.deltaTime;
//				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
//				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
//					
//				}
//			}
//		}
//	}
	internal void Init(){
		ResetValues ();
//		this.GetComponent<BoxCollider2D>().enabled = true;
		if(this.general != null){
			this.textMesh.text = this.general.GetArmyHP().ToString ();
			this.kingdomIndicator.color = this.general.citizen.city.kingdom.kingdomColor;
			this.path = this.general.roads;
			this.AddBehaviourTree ();
		}
	}
	void OnTriggerEnter2D(Collider2D other){
		if(this.tag == "General" && other.tag == "General"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
//				if (this.general.location == other.gameObject.GetComponent<GeneralObject> ().general.location) {
					if(!Utilities.AreTwoGeneralsFriendly(other.gameObject.GetComponent<GeneralObject>().general, this.general)){
						if(!Utilities.AreTwoGeneralsFriendly(this.general, other.gameObject.GetComponent<GeneralObject>().general)){
							if(this.general.army.hp > 0 && other.gameObject.GetComponent<GeneralObject> ().general.army.hp > 0){
								this.collidedWithHostile = true;
								this.otherGeneral = other.gameObject.GetComponent<GeneralObject> ().general;
							}
						}
					}
//				}
			}
		}


	}
	internal void MoveTo(Vector3 destination){
		this.targetPosition = destination;
		this.isMoving = true;
		this.UpdateUI ();
	}
	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
		this.transform.position = targetTile.transform.position;
		this.UpdateUI ();
	}
	private void StopMoving(){
		this.isMoving = false;
//		this.targetPosition = null;
	}
	internal void UpdateUI(){
		if(this.general != null){
			this.textMesh.text = this.general.GetArmyHP().ToString ();
		}
	}
	[Task]
	public void Idle(){
		if(this.isIdle){
			this.general.inAction = false;
			if(!this.general.citizen.isDead){
				if(this.general.citizen.city.incomingGenerals.Count > 0){
					if(this.general.citizen.city.incomingGenerals.Where(x => x.assignedCampaign.campaignType == CAMPAIGN.OFFENSE && x.assignedCampaign.targetCity.id == this.general.citizen.city.id).ToList().Count > 0){
						this.isRoaming = false;
					}else{
						this.isRoaming = true;
					}
				}else{
					this.isRoaming = true;
				}
			}else{
				this.isRoaming = false;
			}


			if(this.isRoaming){
				if(!this.isPreviousRoaming){
					this.isPreviousRoaming = true;
					this.path.Clear ();
				}
				Roam ();
			}else{
				if(this.isPreviousRoaming){
					this.isPreviousRoaming = false;
					this.path.Clear ();
				}
				GoHome();
			}
//			if(this.general.isHome){
//				if(!this.isMoving){
//					this.isMoving = true;
//					Roam ();
//					RoamTo ();
//				}else{
//					RoamTo ();
//				}
//			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}	
	}
	[Task]
	public void IsThereGeneral(){
		if(this.general != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasCampaign(){
		if(this.general.assignedCampaign == null){
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}
	[Task]
	public void IsCampaignDone(){
		if(this.general.assignedCampaign.isDone){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasCampaignLeader(){
		if(this.general.assignedCampaign.leader == null){
			this.general.UnregisterThisGeneral ();
			Task.current.Fail ();
		}else{
			if (this.general.assignedCampaign.leader.isDead) {
				this.general.UnregisterThisGeneral ();
				Task.current.Fail ();
			}else{
				Task.current.Succeed ();
			}
		}
	}

	[Task]
	public void HasTargetCityOrNotConquered(){
		if(this.general.assignedCampaign.targetCity == null || this.general.assignedCampaign.targetCity.isDead){
			this.general.UnregisterThisGeneral ();
			Task.current.Fail ();
		}else{
			Task.current.Succeed ();
		}
	}

	[Task]
	public void IsGeneralInHome(){
		if(IsGeneralInsideHomeCity()){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void IsGoingHome(){
		if(this.general.isGoingHome){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void ReturnToHome(){
		this.general.RerouteToHome ();
		Task.current.Succeed ();
	}

	[Task]
	public void IsSearchingForTarget(){
		if(this.isSearchingForTarget){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasArrivedHome(){
		if(this.general.isGoingHome){
			if(IsGeneralInsideHomeCity()){
				this.general.inAction = false;
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}

	}
	[Task]
	public void HasArrivedAttackTargetCity(){
		if(this.general.assignedCampaign != null){
			if(this.general.assignedCampaign.campaignType == CAMPAIGN.OFFENSE){
				if (this.general.location == this.general.assignedCampaign.targetCity.hexTile) {
					CombatManager.Instance.CityBattle (this.general.assignedCampaign.targetCity, this.general);
					Task.current.Succeed ();
				}else{
					Task.current.Fail ();
				}
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}

	}
	[Task]
	public void HasArrivedDefenseTargetCity(){
		if (this.general.assignedCampaign != null) {
			if (this.general.assignedCampaign.campaignType == CAMPAIGN.DEFENSE) {
				if (this.general.location == this.general.assignedCampaign.targetCity.hexTile) {
					if (this.general.assignedCampaign.AreAllGeneralsOnDefenseCity ()) {
						if (this.general.assignedCampaign.expiration == -1) {
							this.general.assignedCampaign.expiration = Utilities.defaultCampaignExpiration;
						}
					}
					Task.current.Succeed ();
				} else {
					Task.current.Fail ();
				}
			} else {
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedRallyPoint(){
		if (this.general.assignedCampaign != null) {
			if (this.general.assignedCampaign.campaignType == CAMPAIGN.OFFENSE) {
				if (this.general.location == this.general.assignedCampaign.rallyPoint) {
					if (this.general.targetLocation == this.general.assignedCampaign.rallyPoint) {
						if (this.general.assignedCampaign.AreAllGeneralsOnRallyPoint ()) {
							this.general.assignedCampaign.AttackCityNow ();
						}
						Task.current.Succeed ();
					} else {
						Task.current.Fail ();
					}
				} else {
					Task.current.Fail ();
				}
			} else {
				Task.current.Fail ();
			}
		}else {
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasCollidedWithHostileGeneral(){
		if(this.collidedWithHostile){
			this.collidedWithHostile = false;
			if(this.general.army.hp > 0 && this.otherGeneral.army.hp > 0){
				CombatManager.Instance.BattleMidway (ref this.general, ref this.otherGeneral);
				if(this.otherGeneral.army.hp <= 0){
					this.otherGeneral.citizen.Death(DEATH_REASONS.BATTLE);
				}
				this.otherGeneral = null;
				if(this.general.army.hp <= 0){
					ResetValues ();
					this.general.citizen.Death(DEATH_REASONS.BATTLE);
					Task.current.Succeed ();
				}else{
					Task.current.Fail ();
				}
			}else{
				Task.current.Fail ();
			}

		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void MoveToNextTile(){
		if (this.general.citizen.miscTraits.Contains (MISC_TRAIT.FAST)) {
			if (!this.general.citizen.isDead) {
				for (int i = 0; i < 2; i++) {
					Move ();
				}
			}else{
				Move ();
			}
		}else{
			Move ();
		}
		Task.current.Succeed ();
	}

	[Task]
	public void FindTarget(){
		Debug.Log (this.general.citizen.name + " instructed by " + this.general.assignedCampaign.leader.name + " is searching for " + this.general.target.name);
		this.general.daysCounter += 1;
		if(this.general.daysCounter <= 8){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < (5 * this.general.daysCounter)){
				//FOUND TARGET
				Debug.Log("TARGET FOUND: " + this.general.target.name + ". He/She will be killed.");
				Task.current.Succeed ();
			}else{
				if(this.general.daysCounter == 8){
					//FOUND TARGET
					Debug.Log("TARGET FOUND: " + this.general.target.name + ". He/She will be killed.");
					Task.current.Succeed ();
				}else{
					Task.current.Fail ();
				}
			}

		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void KillTarget(){
		this.general.daysCounter = 0;

		if(this.general.target.isHeir){
			this.general.target.Death (DEATH_REASONS.REBELLION, false, this.general.assignedCampaign.leader, false);
		}else{
			this.general.target.Death (DEATH_REASONS.TREACHERY, false,  this.general.assignedCampaign.leader, false);
		}

//		Campaign campaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID(this.general.assignedCampaign.id);
		if(this.general.assignedCampaign != null){
			if (this.general.assignedCampaign.leader != null) {
				this.general.assignedCampaign.leader.campaignManager.CampaignDone (this.general.assignedCampaign);
			}
		}

		this.isSearchingForTarget = false;
		Task.current.Succeed ();
	}
	private void ResetValues(){
		this.isSearchingForTarget = false;
		this.collidedWithHostile = false;
		this.otherGeneral = null;
	}
	public bool IsGeneralInsideHomeCity(){
		if (this.general.location == this.general.citizen.city.hexTile) {
			this.general.isHome = true;
			this.general.isGoingHome = false;
			IsMyGeneralDead ();
			return true;
		}else{
			return false;
		}
	}
	private void IsMyGeneralDead(){
		if(this.general.citizen.isDead){
			this.general.citizen.city.LookForNewGeneral (this.general);
		}else{
			this.general.citizen.city.LookForLostArmy (this.general);
			this.general.inAction = false;
			this.isIdle = true;
			this.isMoving = false;
		}
	}
	private void Move(){
		if(this.general.targetLocation != null){
			if(this.general.roads != null){
				if(this.general.roads.Count > 0){
					this.general.daysBeforeMoving -= 1;
					if(this.general.daysBeforeMoving <= 0){
						this.general.generalAvatar.GetComponent<GeneralObject>().MakeCitizenMove (this.general.location, this.general.roads [0]);
						this.general.daysBeforeMoving = this.general.roads [0].movementDays;
						this.general.location = this.general.roads[0];
//						if(this.general.roads[0] != this.general.citizen.city.hexTile){
//							this.general.isHome = false;
//						}
						this.general.roads.RemoveAt (0);
						this.general.daysBeforeArrival -= 1;

					}
				}
			}
		}
	}

	internal void Victory(){
//		Campaign campaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.general.assignedCampaign.id);
		if (this.general.assignedCampaign != null) {
			if (this.general.assignedCampaign.warType == WAR_TYPE.INTERNATIONAL) {
				City targetCity = this.general.assignedCampaign.targetCity;
				CombatManager.Instance.ConquerCity (this.general.citizen.city.kingdom, targetCity);
				this.general.assignedCampaign.leader.campaignManager.CampaignDone (this.general.assignedCampaign);
			}else if (this.general.assignedCampaign.warType == WAR_TYPE.SUCCESSION) {
				this.general.target = this.general.assignedCampaign.leader.GetTargetSuccessionWar (this.general.assignedCampaign.targetCity);
				this.isSearchingForTarget = true;
			}

		} 
	}
	private void RoamTo(){
		if(this.general.citizen.city.ownedTiles.Count > 0){
			HexTile roamTile = this.general.citizen.city.ownedTiles [UnityEngine.Random.Range (0, this.general.citizen.city.ownedTiles.Count)];
			this.path.Clear ();
			this.path = PathGenerator.Instance.GetPath (this.general.location, roamTile, PATHFINDING_MODE.COMBAT);
//			this.MoveTo (roamTile.transform.position);

		}
	}
	private void Roam(){
		if(this.path.Count <= 0 || this.path == null){
			RoamTo ();
		}
		if(this.path != null && this.path.Count > 0){
			this.MakeCitizenMove (this.general.location, this.path [0]);
			this.general.location = this.path[0];
			this.path.RemoveAt (0);
			if(this.general.location.city != null){
				if(this.general.location.city.id == this.general.citizen.city.id){
					this.general.citizen.city.LookForLostArmy (this.general);
				}
			}
		}
		this.UpdateUI ();
	}
	private void GoHome(){
		if(this.general.location != this.general.citizen.city.hexTile){
			if(this.path.Count <= 0 || this.path == null){
				this.path.Clear ();
				this.path = PathGenerator.Instance.GetPath (this.general.location, this.general.citizen.city.hexTile, PATHFINDING_MODE.COMBAT);
			}
			if(this.path != null && this.path.Count > 0){
				this.MakeCitizenMove (this.general.location, this.path [0]);
				this.general.location = this.path[0];
				this.path.RemoveAt (0);
				if(!this.general.citizen.isDead){
					if(this.general.location.city != null){
						if(this.general.location.city.id == this.general.citizen.city.id){
							this.general.citizen.city.LookForLostArmy (this.general);
						}
					}
				}

			}
		}

		this.UpdateUI ();
	}
	internal void AddBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Add (this.pandaBehaviour);
	}

	internal void RemoveBehaviourTree(){
		bool removed = BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
		Debug.Log ("REMOVED?: " + this.general.citizen.name + " BT = " + removed);
	}
	internal void GhostGeneral(){
		this.deathIcon.SetActive (true);
	}

//	void OnMouseEnter(){
//		if (!UIManager.Instance.IsMouseOnUI()) {
//			if(this.general.assignedCampaign != null){
//				if (this.general.assignedCampaign.leader != null) {
//					Campaign chosenCampaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.general.assignedCampaign.id);
//					if (chosenCampaign != null) {
//						string info = this.CampaignInfo (chosenCampaign);
//						UIManager.Instance.ShowSmallInfo (info, UIManager.Instance.transform);
//						this.HighlightPath ();
//					}
//				}
//			}
//		}
//	}
//
//	void OnMouseExit(){
//		UIManager.Instance.HideSmallInfo();
//		this.UnHighlightPath ();
//	}
//
//	void HighlightPath(){
//		if (this.general.assignedCampaign != null) {
//			this.pathToUnhighlight.Clear ();
//			for (int i = 0; i < this.general.roads.Count; i++) {
//				this.general.roads [i].highlightGO.SetActive (true);
//				this.pathToUnhighlight.Add (this.general.roads [i]);
//			}
//		}
//	}
//
//	void UnHighlightPath(){
//		if (this.general.assignedCampaign != null) {
//			for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
//				this.pathToUnhighlight[i].highlightGO.SetActive(false);
//			}
//		}
//	}
//	private string CampaignInfo(Campaign campaign){
//		string info = string.Empty;
//		info += "id: " + campaign.id;
//		info += "\n";
//
//		info += "campaign type: " + campaign.campaignType.ToString ();
//		info += "\n";
//
//		info += "general: " + this.general.citizen.name;
//		info += "\n";
//
//		info += "target city: " + campaign.targetCity.name;
//		info += "\n";
//		if (campaign.rallyPoint == null) {
//			info += "rally point: N/A";
//		} else {
//			info += "rally point: " + campaign.rallyPoint.name; 
//		}
//		info += "\n";
//
//		info += "leader: " + campaign.leader.name;
//		info += "\n";
//
//		info += "war type: " + campaign.warType.ToString ();
//		info += "\n";
//
//		info += "needed army: " + campaign.neededArmyStrength.ToString ();
//		info += "\n";
//
//		info += "army: " + campaign.GetArmyStrength ().ToString ();
//		info += "\n";
//
//		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
//			if (campaign.expiration == -1) {
//				info += "expiration: none";
//			} else {
//				info += "will expire in " + campaign.expiration + " days";
//			}
//		} else {
//			info += "expiration: none";
//
//		}
//
//		return info;
//	}


}
