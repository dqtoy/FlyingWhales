using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
//	public City targetCity;
	public HexTile location;
	public HexTile targetLocation;
//	public HexTile rallyPoint;
	public List<HexTile> roads;
	public Army army;
//	public Citizen warLeader;
//	public int campaignID;
//	public CAMPAIGN assignedCampaign;
	public Campaign assignedCampaign;
//	public WAR_TYPE warType;
	public int daysBeforeArrival;
	public int daysBeforeReleaseTask;
	public bool isOnTheWay;
	public GameObject generalAvatar;
	public int battlesWon;
	public int citiesInvaded;
	public int successfulRaids;
	public int unsuccessfulRaids;
	public bool inAction;
	public bool isGoingHome;
	public bool isHome;

	internal Citizen target;
	public int daysCounter = 0;
	public int daysBeforeMoving;

	public General(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.daysBeforeMoving = citizen.city.hexTile.movementDays;
		this.targetLocation = null;
//		this.targetCity = null;
//		this.rallyPoint = null;
//		this.warLeader = null;
		this.army = new Army (GetInitialArmyHp());
//		this.campaignID = 0;
		this.assignedCampaign = null;
//		this.warType = WAR_TYPE.NONE;
		this.isOnTheWay = false;
		this.daysBeforeArrival = 0;
		this.daysBeforeReleaseTask = 0;
		this.roads = new List<HexTile> ();
		this.battlesWon = 0;
		this.citiesInvaded = 0;
		this.successfulRaids = 0;
		this.unsuccessfulRaids = 0;
		this.inAction = false;
		this.isGoingHome = false;
		this.isHome = true;
//		EventManager.Instance.onCitizenMove.AddListener (Move);
		EventManager.Instance.onRegisterOnCampaign.AddListener (RegisterOnCampaign);
		EventManager.Instance.onLookForLostArmies.AddListener (JoinArmyTo);
		InitializeGeneral ();
	}
	internal void InitializeGeneral(){
		if(this.generalAvatar == null){
			this.generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.location.transform) as GameObject;
			this.generalAvatar.transform.localPosition = Vector3.zero;
			this.generalAvatar.GetComponent<GeneralObject>().general = this;
			this.generalAvatar.GetComponent<GeneralObject> ().Init();
		}
	}
	internal int GetArmyHP(){
		float multiplier = 1f;
		if(this.citizen.miscTraits.Contains(MISC_TRAIT.STRONG)){
			multiplier = 1.10f;
		}
		return (int)(this.army.hp * multiplier);
	}
	private int GetInitialArmyHp(){
		switch (this.citizen.city.kingdom.race) {
		case RACE.HUMANS:
			return 120;
		case RACE.ELVES:
			return 100;
		case RACE.MINGONS:
			return 120;
		case RACE.CROMADS:
			return 160;
		}

		return 0;
	}
	internal void DeathArmy(){
		if(this.army.hp <= 0){
			this.citizen.Death (DEATH_REASONS.BATTLE);
		}
	}
	internal void RerouteToHome(){
		if(this.location != this.citizen.city.hexTile){
			List<HexTile> path = PathGenerator.Instance.GetPath (this.location, this.citizen.city.hexTile, PATHFINDING_MODE.COMBAT);
			if(path != null){
				this.roads.Clear ();
				this.roads = path;
				this.targetLocation = this.citizen.city.hexTile;
				this.isGoingHome = true;
				this.inAction = true;
//				if (this.generalAvatar == null) {
//					this.generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.location.transform) as GameObject;
//					this.generalAvatar.transform.localPosition = Vector3.zero;
//					this.generalAvatar.GetComponent<GeneralObject> ().general = this;
//					this.generalAvatar.GetComponent<GeneralObject> ().Init ();
//				} else {
//					
//				}
				this.generalAvatar.transform.parent = this.location.transform;
				this.generalAvatar.transform.localPosition = Vector3.zero;
				this.generalAvatar.GetComponent<GeneralObject> ().path.Clear ();
				this.generalAvatar.GetComponent<GeneralObject> ().path = path;

				Debug.Log (this.citizen.name + " IS GOING HOME!");
			}
		}
	}
	internal void UnregisterThisGeneral(Campaign campaign, bool isRerouteToHome = true, bool isBulk = false){
		if(campaign == null){
			if (this.assignedCampaign != null) {
				if (this.assignedCampaign.leader != null) {
					campaign = this.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.assignedCampaign.id);
				}
			}
		}

		if(campaign != null){
			campaign.registeredGenerals.Remove (this);
//			if (this.targetLocation != null) {
//				if (this.targetLocation.isOccupied) {
//					this.targetLocation.city.incomingGenerals.Remove (this);
//				}
//			}
			this.targetLocation = null;
			this.assignedCampaign = null;
//			this.targetCity = null;
//			this.rallyPoint = null;
			this.daysBeforeArrival = 0;
			this.inAction = false;
			if(campaign.targetCity != null){
				campaign.targetCity.incomingGenerals.Remove (this);
			}
			if(isRerouteToHome){
				RerouteToHome ();
			}
			if(!isBulk){
				if(campaign.registeredGenerals.Count <= 0){
					campaign.leader.campaignManager.CampaignDone (campaign);
				}else{
					if(campaign.campaignType == CAMPAIGN.OFFENSE){
						if(campaign.AreAllGeneralsOnRallyPoint()){
							Debug.Log ("Will attack city now " + campaign.targetCity.name);
							campaign.AttackCityNow ();
						}
					}else if(campaign.campaignType == CAMPAIGN.DEFENSE){
						if(campaign.AreAllGeneralsOnDefenseCity()){
							Debug.Log ("ALL GENERALS ARE ON DEFENSE CITY " + campaign.targetCity.name + ". START EXPIRATION.");
							campaign.expiration = Utilities.defaultCampaignExpiration;
						}
					}
				}
			}
		}
	}
	internal void RegisterOnCampaign(Campaign campaign){
		if(this.inAction){
			return;
		}
		if(this.citizen.isDead){
			return;
		}

		if(campaign.campaignType == CAMPAIGN.OFFENSE){
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " REGISTERING ON OFFENSE CAMPAIGN OF " + campaign.leader.name + " of " + campaign.leader.city.kingdom.name + " W/ TARGET " + campaign.targetCity.name + " " + campaign.warType.ToString() + "...");
			if(campaign.warType == WAR_TYPE.INTERNATIONAL){
				if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id) {
					if (this.citizen.city.governor.supportedCitizen == null) {
						if (campaign.leader.isKing) {
							if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (this.location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
								if (path != null) {
									List<HexTile> path2 = PathGenerator.Instance.GetPath (campaign.rallyPoint, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
									if (path2 == null) {
										Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE THERE'S NO PATH FROM RALLY POINT TO TARGET (" + campaign.targetCity.name + ")");
										path = null;
									}
								}else{
									Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO RALLY POINT (" + campaign.targetCity.name + ")");
								}
								if (path != null) {
									AssignCampaign (campaign, path);
								}else{
									Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO RALLY POINT (" + campaign.targetCity.name + ")");
								}
							}else{
								Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE IT'S ALREADY FULL (" + campaign.targetCity.name + ")");
							}
						}else{
							Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE CAMPAIGN LEADER IS NOT KING (" + campaign.targetCity.name + ")");
						}
					}else{
						Debug.Log (this.citizen.name + " CAN'T REGISTER ON OFFENSE CAMPAIGN BECAUSE GOVERNOR IS NOT SUPPORTING THE KING (" + campaign.targetCity.name + ")");
					}
				}
			}else if(campaign.warType == WAR_TYPE.SUCCESSION){
				if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id){
					if(this.citizen.city.governor.supportedCitizen == null){
						if(campaign.leader.isHeir){
							if(campaign.GetArmyStrength() < campaign.neededArmyStrength){
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (this.location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
								if (path != null) {
									List<HexTile> path2 = PathGenerator.Instance.GetPath (campaign.rallyPoint, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
									if (path2 == null) {
										path = null;
									}
								}
								if(path != null){
									AssignCampaign (campaign, path);
								}
							}
						}
					}else{
						if(this.citizen.city.governor.supportedCitizen.id == campaign.leader.id){
							if(campaign.GetArmyStrength() < campaign.neededArmyStrength){
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (this.location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
								if (path != null) {
									List<HexTile> path2 = PathGenerator.Instance.GetPath (campaign.rallyPoint, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
									if (path2 == null) {
										path = null;
									}
								}
								if(path != null){
									AssignCampaign (campaign, path);
								}
							}
						}
					}
				}else{
					if(this.citizen.city.kingdom.king.supportedCitizen != null){
						if (this.citizen.city.kingdom.king.supportedCitizen.id == campaign.leader.id) {
							if(campaign.GetArmyStrength() < campaign.neededArmyStrength){
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (this.location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
								if (path != null) {
									List<HexTile> path2 = PathGenerator.Instance.GetPath (campaign.rallyPoint, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
									if (path2 == null) {
										path = null;
									}
								}
								if(path != null){
									AssignCampaign (campaign, path);
								}
							}
						}
					}
				}
			}else if(campaign.warType == WAR_TYPE.CIVIL){

			}
		}else{
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " REGISTERING ON DEFENSE CAMPAIGN OF " + campaign.leader.name + " of " + campaign.leader.city.kingdom.name + " W/ TARGET " + campaign.targetCity.name + " " + campaign.warType.ToString() + "...");
			List<HexTile> path = null;
			if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id) {
				if (this.citizen.city.governor.supportedCitizen == null) {
					if (campaign.leader.isKing || campaign.leader.isHeir) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (this.location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if(path != null){
									if (path.Count > (campaign.expiration - 1)) {
										Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE PATH COUNT EXCEEDS THE EXPIRATION WEEKS (" + campaign.targetCity.name + ")");
										path = null;
									}
								}else{
									Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
								}
							}
							if (path != null) {
								AssignCampaign (campaign, path);
							}else{
								Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
							}
						}
					}
				} else {
					if (this.citizen.city.governor.supportedCitizen.id == campaign.leader.id) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (this.location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if (path != null) {
									if (path.Count > (campaign.expiration - 1)) {
										Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE PATH COUNT EXCEEDS THE EXPIRATION WEEKS (" + campaign.targetCity.name + ")");
										path = null;
									}
								}else{
									Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
								}
							}
							if (path != null) {
								AssignCampaign (campaign, path);
							}else{
								Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
							}
						}
					}
				}

			} else {
				if (this.citizen.city.kingdom.king.supportedCitizen != null) {
					if (this.citizen.city.kingdom.king.supportedCitizen.id == campaign.leader.id) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (this.location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if (path != null) {
									if (path.Count > (campaign.expiration - 1)) {
										Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE PATH COUNT EXCEEDS THE EXPIRATION WEEKS (" + campaign.targetCity.name + ")");
										path = null;
									}
								}else{
									Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
								}
							}
							if (path != null) {
								AssignCampaign (campaign, path);
							}else{
								Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + campaign.targetCity.name + ")");
							}
						}
					}
				}
			}

		}



	}
	internal void AssignCampaign(Campaign chosenCampaign, List<HexTile> path){
		if(chosenCampaign.campaignType == CAMPAIGN.OFFENSE){
			this.targetLocation = chosenCampaign.rallyPoint;
		}else{
			this.targetLocation = chosenCampaign.targetCity.hexTile;
		}
		Debug.Log (this.citizen.name +  " Target Location: " + this.targetLocation.tileName + " Campaign Type: " + chosenCampaign.campaignType.ToString());

//		this.warLeader = chosenCampaign.leader;
//		this.campaignID = chosenCampaign.id;
		this.assignedCampaign = chosenCampaign;
//		this.warType = chosenCampaign.warType;
//		this.targetCity = chosenCampaign.targetCity;
//		this.rallyPoint = chosenCampaign.rallyPoint;
		this.daysBeforeArrival = path.Sum(x => x.movementDays);
		this.roads.Clear ();
		this.roads = path;
		this.inAction = true;

		chosenCampaign.registeredGenerals.Add (this);
//		chosenCampaign.targetCity.incomingGenerals.Add (this);
		if(chosenCampaign.rallyPoint != null){
			if(chosenCampaign.rallyPoint.isOccupied){
				chosenCampaign.rallyPoint.city.incomingGenerals.Add(this);
			}
		}

//		if(this.generalAvatar == null){
//			this.generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.location.transform) as GameObject;
//			this.generalAvatar.transform.localPosition = Vector3.zero;
//			this.generalAvatar.GetComponent<GeneralObject>().general = this;
//			this.generalAvatar.GetComponent<GeneralObject> ().Init();
//
//		}else{
//			
//		}
		this.generalAvatar.transform.parent = this.location.transform;
		this.generalAvatar.transform.localPosition = Vector3.zero;
		this.generalAvatar.GetComponent<GeneralObject> ().isIdle = false;
		this.generalAvatar.GetComponent<GeneralObject> ().path.Clear ();
		this.generalAvatar.GetComponent<GeneralObject> ().path = path;
	}
//	internal void SearchForTarget(){
//		Debug.Log (this.citizen.name + " instructed by " + this.warLeader.name + " is searching for " + this.target.name);
//		this.daysCounter += 1;
//		if(this.daysCounter <= 8){
//			int chance = UnityEngine.Random.Range (0, 100);
//			if(chance < (5 * this.daysCounter)){
//				//FOUND TARGET
//				Debug.Log("TARGET FOUND: " + target.name + ". He/She will be killed.");
//				if(target.isHeir){
//					target.Death (DEATH_REASONS.REBELLION, false, this.warLeader, false);
//				}else{
//					target.Death (DEATH_REASONS.TREACHERY, false,  this.warLeader, false);
//				}
//
//			}else{
//				if(this.daysCounter == 8){
//					//FOUND TARGET
//					Debug.Log("TARGET FOUND: " + target.name + ". He/She will be killed.");
//					if(target.isHeir){
//						target.Death (DEATH_REASONS.REBELLION, false, this.warLeader, false);
//					}else{
//						target.Death (DEATH_REASONS.TREACHERY, false, this.warLeader, false);
//					}
//
//				}
//			}
//			if(this.daysCounter == 8){
//				this.daysCounter = 0;
//				Campaign campaign = this.warLeader.campaignManager.SearchCampaignByID (this.campaignID);
//				if(campaign != null){
//					campaign.leader.campaignManager.CampaignDone (campaign);
//				}
//			}
//		}
//	}
//	internal void Move(bool isFast = false){
//		if(this.targetLocation != null){
//			if(this.roads != null){
//				if(this.roads.Count > 0){
//					if (this.generalAvatar != null) {
//						this.generalAvatar.GetComponent<GeneralObject>().MakeCitizenMove (this.location, this.roads [0]);
//					}
//					this.location = this.roads[0];
//					this.roads.RemoveAt (0);
//					this.daysBeforeArrival -= 1;
//					if(this.location == this.targetLocation){
//						if(this.warLeader != null){
//							this.warLeader.campaignManager.GeneralHasArrived (this);
//						}
//						return;
//					}
//				}
//			}
//			if(!isFast){
//				if(!this.citizen.isDead){
//					if(this.citizen.miscTraits.Contains(MISC_TRAIT.FAST)){
//						Move (true);
//					}
//				}
//			}
//
//
//		}
//	}

	internal void GeneralDeath(){
//		EventManager.Instance.onCitizenMove.RemoveListener (Move);
		EventManager.Instance.onRegisterOnCampaign.RemoveListener (RegisterOnCampaign);
		EventManager.Instance.onLookForLostArmies.RemoveListener (JoinArmyTo);

		//					((General)this.assignedRole) = null;

		if (this.generalAvatar != null) {
			this.generalAvatar.GetComponent<GeneralObject>().RemoveBehaviourTree();
			GameObject.Destroy (this.generalAvatar);
			this.generalAvatar = null;
		}
		this.UnregisterThisGeneral (null, false);
		this.inAction = false;
		this.citizen.role = ROLE.UNTRAINED;
		this.citizen.assignedRole = null;
		this.citizen.city.citizens.Remove (this.citizen);
	}
	internal void UntrainGeneral(){
//		EventManager.Instance.onCitizenMove.RemoveListener (Move);
		EventManager.Instance.onRegisterOnCampaign.RemoveListener (RegisterOnCampaign);
		EventManager.Instance.onLookForLostArmies.RemoveListener (JoinArmyTo);


		if (this.generalAvatar != null) {
			this.generalAvatar.GetComponent<GeneralObject>().RemoveBehaviourTree();
			GameObject.Destroy (this.generalAvatar);
			this.generalAvatar = null;
		}
		this.UnregisterThisGeneral (null, false);
	}
	internal void JoinArmyTo(General general){
		if(this.citizen.isGhost && this.citizen.isDead){
			if (this.location == general.location) {
				Debug.Log (this.citizen.name + " of " + this.citizen.city.name + " IS JOINING ARMY OF " + general.citizen.name + " of " + general.citizen.city.name);
				general.army.hp += this.army.hp;
				this.army.hp = 0;
				this.UpdateUI ();
				this.GeneralDeath ();
			}
		}

	}
	internal void CreateGhostCitizen(){
		Citizen newCitizen = new Citizen (this.citizen.city, 0, GENDER.MALE, 0, true);
		newCitizen.isDead = true;
		this.citizen = newCitizen;
	}
	internal void UpdateUI(){
		if(this.generalAvatar != null){
			this.generalAvatar.GetComponent<GeneralObject> ().UpdateUI ();
		}
	}
	internal bool IsDefense(General enemy){
		if(this.assignedCampaign != null){
			if(this.assignedCampaign.campaignType == CAMPAIGN.DEFENSE){
				return true;
			}else{
				if (enemy.assignedCampaign != null) {
					if (enemy.assignedCampaign.campaignType != CAMPAIGN.DEFENSE) {
						if (this.location == enemy.assignedCampaign.targetCity.hexTile) {
							return true;
						}
					}else{
						if (this.location == this.citizen.city.hexTile) {
							return true;
						}
					}
				}else{
					if (this.location == this.citizen.city.hexTile) {
						return true;
					}
				}
			}
		}else{
			if (enemy.assignedCampaign != null) {
				if (enemy.assignedCampaign.campaignType != CAMPAIGN.DEFENSE) {
					if (this.location == enemy.assignedCampaign.targetCity.hexTile) {
						return true;
					}
				}else{
					if (this.location == this.citizen.city.hexTile) {
						return true;
					}
				}
			}else{
				if (this.location == this.citizen.city.hexTile) {
					return true;
				}
			}
		}

		return false;
	}
}
