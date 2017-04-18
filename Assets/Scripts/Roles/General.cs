using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
	public City targetCity;
	public HexTile location;
	public HexTile targetLocation;
	public HexTile rallyPoint;
	public List<HexTile> roads;
	public Army army;
	public Citizen warLeader;
	public int campaignID;
	public CAMPAIGN assignedCampaign;
	public WAR_TYPE warType;
	public int daysBeforeArrival;
	public int daysBeforeReleaseTask;
	public bool isOnTheWay;
	public GameObject generalAvatar;
	public int battlesWon;
	public int citiesInvaded;
	public int successfulRaids;
	public int unsuccessfulRaids;
	public bool inAction;

	internal Citizen target;
	private int weekCounter = 0;

	public General(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.targetLocation = null;
		this.targetCity = null;
		this.rallyPoint = null;
		this.warLeader = null;
		this.army = new Army (GetInitialArmyHp());
		this.campaignID = 0;
		this.assignedCampaign = CAMPAIGN.NONE;
		this.warType = WAR_TYPE.NONE;
		this.isOnTheWay = false;
		this.daysBeforeArrival = 0;
		this.daysBeforeReleaseTask = 0;
		this.roads = new List<HexTile> ();
		this.battlesWon = 0;
		this.citiesInvaded = 0;
		this.successfulRaids = 0;
		this.unsuccessfulRaids = 0;
		this.inAction = false;
		EventManager.Instance.onCitizenMove.AddListener (Move);
		EventManager.Instance.onRegisterOnCampaign.AddListener (RegisterOnCampaign);
		EventManager.Instance.onDeathArmy.AddListener (DeathArmy);
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
			this.targetLocation = this.citizen.city.hexTile;
			this.roads.Clear ();
			List<HexTile> path = PathGenerator.Instance.GetPath (this.location, this.targetLocation, PATHFINDING_MODE.NORMAL);
			this.roads = path;
		}
	}
	internal void UnregisterThisGeneral(Campaign campaign){
		if(campaign == null){
			if(this.warLeader != null){
				campaign = this.warLeader.campaignManager.SearchCampaignByID (this.campaignID);
			}
		}

		if(campaign != null){
			campaign.registeredGenerals.Remove (this);
			this.targetLocation = null;
			this.warLeader = null;
			this.campaignID = 0;
			this.assignedCampaign = CAMPAIGN.NONE;
			this.targetCity = null;
			this.rallyPoint = null;
			this.daysBeforeArrival = 0;
			this.inAction = false;
			RerouteToHome ();
			if(campaign.registeredGenerals.Count <= 0){
				campaign.leader.campaignManager.CampaignDone (campaign);
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
			if(campaign.warType == WAR_TYPE.INTERNATIONAL){
				if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id) {
					if (this.citizen.city.governor.supportedCitizen == null) {
						if (campaign.leader.isKing) {
							if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (((General)this).location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
								if (path != null) {
									List<HexTile> path2 = PathGenerator.Instance.GetPath (campaign.rallyPoint, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
									if (path2 == null) {
										path = null;
									}
								}
								if (path != null) {
									AssignCampaign (campaign, path);
								}
							}
						}
					}
				}
			}else if(campaign.warType == WAR_TYPE.SUCCESSION){
				if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id){
					if(this.citizen.city.governor.supportedCitizen == null){
						if(campaign.leader.isHeir){
							if(campaign.GetArmyStrength() < campaign.neededArmyStrength){
								List<HexTile> path = null;
								path = PathGenerator.Instance.GetPath (((General)this).location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
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
								path = PathGenerator.Instance.GetPath (((General)this).location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
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
								path = PathGenerator.Instance.GetPath (((General)this).location, campaign.rallyPoint, PATHFINDING_MODE.COMBAT);
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
			List<HexTile> path = null;
			if (this.citizen.city.kingdom.id == campaign.leader.city.kingdom.id) {
				if (this.citizen.city.governor.supportedCitizen == null) {
					if (campaign.leader.isKing || campaign.leader.isHeir) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (((General)this).location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if(path != null){
									if (path.Count > (campaign.expiration - 1)) {
										path = null;
									}
								}

							}
							if (path != null) {
								AssignCampaign (campaign, path);
							}
						}
					}
				} else {
					if (this.citizen.city.governor.supportedCitizen.id == campaign.leader.id) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (((General)this).location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if (path != null) {
									if (path.Count > (campaign.expiration - 1)) {
										path = null;
									}
								}
							}
							if (path != null) {
								AssignCampaign (campaign, path);
							}
						}
					}
				}

			} else {
				if (this.citizen.city.kingdom.king.supportedCitizen != null) {
					if (this.citizen.city.kingdom.king.supportedCitizen.id == campaign.leader.id) {
						if (campaign.GetArmyStrength () < campaign.neededArmyStrength) {
							path = PathGenerator.Instance.GetPath (((General)this).location, campaign.targetCity.hexTile, PATHFINDING_MODE.COMBAT);
							if (campaign.expiration >= 0) {
								if (path != null) {
									if (path.Count > (campaign.expiration - 1)) {
										path = null;
									}
								}
							}
							if (path != null) {
								AssignCampaign (campaign, path);
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

		this.warLeader = chosenCampaign.leader;
		this.campaignID = chosenCampaign.id;
		this.assignedCampaign = chosenCampaign.campaignType;
		this.warType = chosenCampaign.warType;
		this.targetCity = chosenCampaign.targetCity;
		this.rallyPoint = chosenCampaign.rallyPoint;
		this.daysBeforeArrival = path.Count;
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

		this.generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.location.transform) as GameObject;
		this.generalAvatar.GetComponent<GeneralObject>().general = this;
		this.generalAvatar.GetComponent<GeneralObject> ().Init();


	}
	internal void SearchForTarget(){
		Debug.Log (this.citizen.name + " instructed by " + this.warLeader.name + " is searching for " + this.target.name);
		this.weekCounter += 1;
		if(this.weekCounter <= 8){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < (5 * this.weekCounter)){
				//FOUND TARGET
				Debug.Log("TARGET FOUND: " + target.name + ". He/She will be killed.");
				if(target.isHeir){
					target.Death (DEATH_REASONS.REBELLION, false, this.warLeader, false);
				}else{
					target.Death (DEATH_REASONS.TREACHERY, false,  this.warLeader, false);
				}

			}else{
				if(this.weekCounter == 8){
					//FOUND TARGET
					Debug.Log("TARGET FOUND: " + target.name + ". He/She will be killed.");
					if(target.isHeir){
						target.Death (DEATH_REASONS.REBELLION, false, this.warLeader, false);
					}else{
						target.Death (DEATH_REASONS.TREACHERY, false, this.warLeader, false);
					}

				}
			}
			if(this.weekCounter == 8){
				this.weekCounter = 0;
				Campaign campaign = this.warLeader.campaignManager.SearchCampaignByID (this.campaignID);
				if(campaign != null){
					campaign.leader.campaignManager.CampaignDone (campaign);
				}
				EventManager.Instance.onWeekEnd.RemoveListener (this.SearchForTarget);
			}
		}
	}
	internal void Move(){
		if(this.targetLocation != null){
			if(this.roads.Count > 0){
				if (this.generalAvatar != null) {
					this.generalAvatar.GetComponent<GeneralObject>().MakeCitizenMove (this.location, this.roads [0]);
				}
				this.location = this.roads[0];
				this.roads.RemoveAt (0);
				this.daysBeforeArrival -= 1;
				if(this.location == this.targetLocation){
					if(this.warLeader != null){
						this.warLeader.campaignManager.GeneralHasArrived (this.citizen);
					}
					return;
				}else{
					//CHECK FOR BATTLE MIDWAY
//					if(this.location.city != null && this.location.isOccupied){
//						if(this.location.city.id != this.citizen.city.id){
//							bool isDead = false;
//							int armyNumber = this.army.hp;
//							this.location.city.CheckBattleMidwayCity ();
//							//							this.location.city.CheckBattleMidwayCity (this, ref armyNumber, ref isDead);
//							if(!isDead){
//								this.army.hp = armyNumber;
//							}
//						}
//					}
				}

			}
			if(!this.citizen.isDead){
				if(this.citizen.miscTraits.Contains(MISC_TRAIT.FAST)){
					Move ();
				}
			}

		}
	}
}
