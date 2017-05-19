using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[System.Serializable]
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
		EventManager.Instance.onCheckGeneralEligibility.AddListener (CheckEligibility);
//		EventManager.Instance.onRegisterOnCampaign.AddListener (RegisterOnCampaign);
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
				this.roads = new List<HexTile>(path);
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
//				this.generalAvatar.GetComponent<GeneralObject> ().path.Clear ();
//				this.generalAvatar.GetComponent<GeneralObject> ().path = new List<HexTile>(path);

				Debug.Log (this.citizen.name + " IS GOING HOME!");
			}
		}
	}
	internal void UnregisterThisGeneral(bool isRerouteToHome = true, bool isBulk = false){
//		if(campaign == null){
//			if (this.assignedCampaign != null) {
//				if (this.assignedCampaign.leader != null) {
//					campaign = this.assignedCampaign;
//				}
//			}
//		}

		if(this.assignedCampaign != null){
			this.assignedCampaign.registeredGenerals.Remove (this);
//			if (this.targetLocation != null) {
//				if (this.targetLocation.isOccupied) {
//					this.targetLocation.city.incomingGenerals.Remove (this);
//				}
//			}
			this.targetLocation = null;
//			this.targetCity = null;
//			this.rallyPoint = null;
			this.daysBeforeArrival = 0;
			this.inAction = false;
			if(this.assignedCampaign.targetCity != null){
				this.assignedCampaign.targetCity.incomingGenerals.Remove (this);
			}
			if(isRerouteToHome){
				RerouteToHome ();
			}
			if(!isBulk){
				if(this.assignedCampaign.registeredGenerals.Count <= 0){
					if (this.assignedCampaign.leader != null) {
						this.assignedCampaign.leader.campaignManager.CampaignDone (this.assignedCampaign);
					}
				}else{
					if(this.assignedCampaign.campaignType == CAMPAIGN.OFFENSE){
						if(this.assignedCampaign.AreAllGeneralsOnRallyPoint()){
							Debug.Log ("Will attack city now " + this.assignedCampaign.targetCity.name);
							this.assignedCampaign.AttackCityNow ();
						}
					}else if(this.assignedCampaign.campaignType == CAMPAIGN.DEFENSE){
						if(this.assignedCampaign.AreAllGeneralsOnDefenseCity()){
							Debug.Log ("ALL GENERALS ARE ON DEFENSE CITY " + this.assignedCampaign.targetCity.name + ". START EXPIRATION.");
							this.assignedCampaign.expiration = Utilities.defaultCampaignExpiration;
						}
					}
				}
			}
			this.assignedCampaign = null;
		}
	}
	internal void CheckEligibility(Citizen leader, HexTile targetLocation){
		if(this.inAction){
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " CAN'T CHECK ELIGIBILITY FOR " + leader.name + " of " + leader.city.kingdom.name + " W/ TARGET " + targetLocation.name + " BECAUSE IN ACTION");
			return;
		}
		if(this.citizen.isDead){
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " CAN'T CHECK ELIGIBILITY FOR " + leader.name + " of " + leader.city.kingdom.name + " W/ TARGET " + targetLocation.name + " BECAUSE IS DEAD");
			return;
		}
		Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " CHECKING ELIGIBILITY FOR " + leader.name + " of " + leader.city.kingdom.name + " W/ TARGET " + targetLocation.name + " ...");
		List<HexTile> path = null;
		if (this.citizen.city.kingdom.id == leader.city.kingdom.id) {
			if (this.citizen.city.governor.supportedCitizen == null) {
				if (leader.isKing || leader.isHeir) {
					path = PathGenerator.Instance.GetPath (this.location, targetLocation, PATHFINDING_MODE.COMBAT);
					if (path != null) {
						leader.campaignManager.AddCandidate (this, path);
					}else{
						Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + targetLocation.name + ")");
					}
				}
			} else {
				if (this.citizen.city.governor.supportedCitizen.id == leader.id) {
					path = PathGenerator.Instance.GetPath (this.location, targetLocation, PATHFINDING_MODE.COMBAT);
					if (path != null) {
						leader.campaignManager.AddCandidate (this, path);
					}else{
						Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + targetLocation.name + ")");
					}
				}
			}

		} else {
			if (this.citizen.city.kingdom.king.supportedCitizen != null) {
				if (this.citizen.city.kingdom.king.supportedCitizen.id == leader.id) {
					path = PathGenerator.Instance.GetPath (this.location, targetLocation, PATHFINDING_MODE.COMBAT);
					if (path != null) {
						leader.campaignManager.AddCandidate (this, path);
					}else{
						Debug.Log (this.citizen.name + " CAN'T REGISTER ON DEFENSE CAMPAIGN BECAUSE THERE'S NO PATH TO TARGET (" + targetLocation.name + ")");
					}
				}
			}
		}
	}
	internal void RegisterOnCampaign(Campaign campaign){
		if(this.inAction){
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " CAN'T REGISTER ON " + campaign.campaignType.ToString() + " CAMPAIGN OF " + campaign.leader.name + " of " + campaign.leader.city.kingdom.name + " W/ TARGET " + campaign.targetCity.name + " " + campaign.warType.ToString() + " BECAUSE IN ACTION");
			return;
		}
		if(this.citizen.isDead){
			Debug.Log (this.citizen.name + " of " + this.citizen.city.kingdom.name + " CAN'T REGISTER ON " + campaign.campaignType.ToString() + " CAMPAIGN OF " + campaign.leader.name + " of " + campaign.leader.city.kingdom.name + " W/ TARGET " + campaign.targetCity.name + " " + campaign.warType.ToString() + " BECAUSE CITIZEN IS DEAD");
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
								if (path != null && path.Count > 0) {
									campaign.leader.campaignManager.AddCandidate (this, path);
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
									campaign.leader.campaignManager.AddCandidate (this, path);
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
									campaign.leader.campaignManager.AddCandidate (this, path);
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
									campaign.leader.campaignManager.AddCandidate (this, path);
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
								campaign.leader.campaignManager.AddCandidate (this, path);
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
								campaign.leader.campaignManager.AddCandidate (this, path);
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
								campaign.leader.campaignManager.AddCandidate (this, path);
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

			Log newLog = chosenCampaign.CreateNewLogForCampaign (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Campaign", "OffensiveCampaign", "register");
			newLog.AddToFillers (this.citizen, this.citizen.name);
		}else{
			this.targetLocation = chosenCampaign.targetCity.hexTile;

			Log newLog = chosenCampaign.CreateNewLogForCampaign (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Campaign", "DefensiveCampaign", "register");
			newLog.AddToFillers (this.citizen, this.citizen.name);
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
		this.roads = new List<HexTile>(path);
		this.inAction = true;

		chosenCampaign.registeredGenerals.Add (this);
//		chosenCampaign.targetCity.incomingGenerals.Add (this);
//		if(chosenCampaign.rallyPoint != null){
//			if(!chosenCampaign.rallyPoint.isOccupied){
//				chosenCampaign.rallyPoint.city.incomingGenerals.Add(this);
//			}
//		}

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
		this.generalAvatar.GetComponent<GeneralObject> ().isRoaming = false;
		this.generalAvatar.GetComponent<GeneralObject> ().isPreviousRoaming = false;
//		this.generalAvatar.GetComponent<GeneralObject> ().path.Clear ();
//		this.generalAvatar.GetComponent<GeneralObject> ().path = new List<HexTile>(path);
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
		EventManager.Instance.onCheckGeneralEligibility.RemoveListener (CheckEligibility);
//		EventManager.Instance.onRegisterOnCampaign.RemoveListener (RegisterOnCampaign);
		EventManager.Instance.onLookForLostArmies.RemoveListener (JoinArmyTo);

		//					((General)this.assignedRole) = null;

		if (this.generalAvatar != null) {
			this.generalAvatar.GetComponent<GeneralObject>().RemoveBehaviourTree();
			GameObject.Destroy (this.generalAvatar);
			this.generalAvatar = null;
		}
		this.UnregisterThisGeneral (false);
		this.inAction = false;
		this.citizen.role = ROLE.UNTRAINED;
		this.citizen.assignedRole = null;
		this.citizen.city.citizens.Remove (this.citizen);
	}
	internal void UntrainGeneral(){
//		EventManager.Instance.onCitizenMove.RemoveListener (Move);
		EventManager.Instance.onCheckGeneralEligibility.RemoveListener (CheckEligibility);
//		EventManager.Instance.onRegisterOnCampaign.RemoveListener (RegisterOnCampaign);
		EventManager.Instance.onLookForLostArmies.RemoveListener (JoinArmyTo);

		if (this.generalAvatar != null) {
			this.generalAvatar.GetComponent<GeneralObject>().RemoveBehaviourTree();
			GameObject.Destroy (this.generalAvatar);
			this.generalAvatar = null;
		}
		this.UnregisterThisGeneral (false);
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
		newCitizen.assignedRole = this;
		if(this.generalAvatar != null){
			this.generalAvatar.GetComponent<GeneralObject> ().GhostGeneral ();
		}
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
