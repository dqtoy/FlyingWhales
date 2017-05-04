﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;

public class GeneralObject : MonoBehaviour {
	public General general;
	public TextMesh textMesh;
	public List<HexTile> path;

	public bool isIdle;
	public bool isSearchingForTarget;
	public bool collidedWithHostile;
	public General otherGeneral;

	internal void Init(){
		ResetValues ();
		this.GetComponent<BoxCollider2D>().enabled = true;
		if(this.general != null){
			this.textMesh.text = this.general.GetArmyHP().ToString ();
			this.path = this.general.roads;
		}
	}
	void OnTriggerEnter2D(Collider2D other){
		if(this.tag == "General" && other.tag == "General"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
				if(!Utilities.AreTwoGeneralsFriendly(other.gameObject.GetComponent<GeneralObject>().general, this.general)){
					if(!Utilities.AreTwoGeneralsFriendly(this.general, other.gameObject.GetComponent<GeneralObject>().general)){
						if(this.general.army.hp > 0 && other.gameObject.GetComponent<GeneralObject> ().general.army.hp > 0){
							this.collidedWithHostile = true;
							this.otherGeneral = other.gameObject.GetComponent<GeneralObject> ().general;
						}
					}
				}
			}
		}


	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
		this.transform.position = targetTile.transform.position;
		this.UpdateUI ();
	}
	internal void UpdateUI(){
		if(this.general != null){
			this.textMesh.text = this.general.GetArmyHP().ToString ();
		}
	}

	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			if(this.general.assignedCampaign != null){
				if (this.general.assignedCampaign.leader != null) {
					Campaign chosenCampaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.general.assignedCampaign.id);
					if (chosenCampaign != null) {
						string info = this.CampaignInfo (chosenCampaign);
						UIManager.Instance.ShowSmallInfo (info, UIManager.Instance.transform);
						HighlightPath ();
					}
				}
			}
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
		UnHighlightPath ();
	}
	void HighlightPath(){
		for (int i = 0; i < this.general.roads.Count; i++) {
			if (!this.general.roads [i].isHabitable) {
				this.general.roads [i].SetTileColor (Color.gray);
			}
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.path.Count; i++) {
			if (!this.path [i].isHabitable) {
				this.path [i].SetTileColor (Color.white);
			}
		}
	}
	private string CampaignInfo(Campaign campaign){
		string info = string.Empty;
		info += "id: " + campaign.id;
		info += "\n";

		info += "campaign type: " + campaign.campaignType.ToString ();
		info += "\n";

		info += "general: " + this.general.citizen.name;
		info += "\n";

		info += "target city: " + campaign.targetCity.name;
		info += "\n";
		if(campaign.rallyPoint == null){
			info += "rally point: N/A";
		}else{
			info += "rally point: " + campaign.rallyPoint.city.name; 
		}
		info += "\n";

		info += "leader: " + campaign.leader.name;
		info += "\n";

		info += "war type: " + campaign.warType.ToString ();
		info += "\n";

		info += "needed army: " + campaign.neededArmyStrength.ToString ();
		info += "\n";

		info += "army: " + campaign.GetArmyStrength ().ToString ();
		info += "\n";

		if(campaign.campaignType == CAMPAIGN.DEFENSE){
			if(campaign.expiration == -1){
				info += "expiration: none";
			}else{
				info += "will expire in " + campaign.expiration + " days";
			}
		}else{
			info += "expiration: none";

		}

		return info;
	}
	[Task]
	public void Idle(){
		if(this.isIdle){
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
	public void HasCampaignLeader(){
		if(this.general.assignedCampaign.leader == null){
			this.general.UnregisterThisGeneral (null, false);
			Task.current.Fail ();
		}else{
			if (this.general.assignedCampaign.leader.isDead) {
				this.general.UnregisterThisGeneral (null, false);
				Task.current.Fail ();
			}else{
				Task.current.Succeed ();
			}
		}
	}

	[Task]
	public void HasTargetCityOrNotConquered(){
		if(this.general.targetCity == null || this.general.targetCity.isDead){
			this.general.UnregisterThisGeneral (null, false);
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
		if(IsGeneralInsideHomeCity()){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasArrivedAttackTargetCity(){
		if(this.general.assignedCampaign.campaignType == CAMPAIGN.OFFENSE){
			if (this.general.location == this.general.targetCity.hexTile) {
				CombatManager.Instance.CityBattle (this.general.targetCity, ref this.general);
				if(this.general != null){
					if(this.general.army.hp > 0){
						Victory ();
					}
				}

				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasArrivedDefenseTargetCity(){
		if(this.general.assignedCampaign.campaignType == CAMPAIGN.DEFENSE){
			if (this.general.location == this.general.targetCity.hexTile) {
				if(this.general.assignedCampaign.AreAllGeneralsOnDefenseCity()){
					if (this.general.assignedCampaign.expiration == -1) {
						this.general.assignedCampaign.expiration = Utilities.defaultCampaignExpiration;
					}
				}
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedRallyPoint(){
		if(this.general.assignedCampaign.campaignType == CAMPAIGN.OFFENSE){
			if (this.general.location == this.general.rallyPoint) {
				if(this.general.assignedCampaign.AreAllGeneralsOnRallyPoint()){
					this.general.assignedCampaign.AttackCityNow ();
				}
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasCollidedWithHostileGeneral(){
		if(this.collidedWithHostile){
			this.collidedWithHostile = false;
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
		if(this.general.target.isHeir){
			this.general.target.Death (DEATH_REASONS.REBELLION, false, this.general.assignedCampaign.leader, false);
		}else{
			this.general.target.Death (DEATH_REASONS.TREACHERY, false,  this.general.assignedCampaign.leader, false);
		}

		if(this.general.daysCounter == 8){
			this.general.daysCounter = 0;
			Campaign campaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID(this.general.assignedCampaign.id);
			if(campaign != null){
				campaign.leader.campaignManager.CampaignDone (campaign);
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
			this.general.isGoingHome = false;
			this.general.generalAvatar.transform.parent = this.general.citizen.workLocation.transform;
			this.general.generalAvatar.transform.localPosition = Vector3.zero;
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
						this.general.roads.RemoveAt (0);
						this.general.daysBeforeArrival -= 1;
					}
				}
			}
		}
	}

	internal void Victory(){
		Campaign campaign = this.general.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.general.assignedCampaign.id);
		if (campaign != null) {
			if (campaign.warType == WAR_TYPE.INTERNATIONAL) {
				campaign.leader.campaignManager.CampaignDone (campaign);
				int countCitizens = this.general.assignedCampaign.targetCity.citizens.Count;
				for (int i = 0; i < countCitizens; i++) {
					this.general.assignedCampaign.targetCity.citizens [0].Death (DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
				}
				this.general.assignedCampaign.targetCity.incomingGenerals.Clear ();
				this.general.assignedCampaign.targetCity.isDead = true;
				CombatManager.Instance.ConquerCity (this.general.citizen.city.kingdom, this.general.assignedCampaign.targetCity);
			}else if (campaign.warType == WAR_TYPE.SUCCESSION) {
				this.general.target = this.general.assignedCampaign.leader.GetTargetSuccessionWar (campaign.targetCity);
				this.isSearchingForTarget = true;
			}

		} 
	}
}
