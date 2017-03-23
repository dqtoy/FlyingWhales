using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class General : Role {
	public City targetCity;
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> roads;
	public Army army;
	public Citizen warLeader;
	public int campaignID;
	public CAMPAIGN assignedCampaign;
	public int daysBeforeArrival;
	public int daysBeforeReleaseTask;
	public bool isOnTheWay;
	public GameObject generalAvatar;

	public General(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.targetLocation = null;
		this.targetCity = null;
		this.warLeader = null;
		this.army = new Army (GetInitialArmyHp());
		this.campaignID = 0;
		this.assignedCampaign = CAMPAIGN.NONE;
		this.isOnTheWay = false;
		this.daysBeforeArrival = 0;
		this.daysBeforeReleaseTask = 0;
		this.roads = new List<HexTile> ();
		EventManager.Instance.onCitizenMove.AddListener (Move);
	}
	internal int GetArmyHP(){
		float multiplier = 1f;
		if(this.citizen.miscTraits.Contains(MISC_TRAIT.STRONG)){
			multiplier = 1.10f;
		}
		return this.army.hp * multiplier;
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
	internal void Move(){
		if(this.targetLocation != null){
			if(this.roads.Count > 0){

				//					this.generalAvatar.GetComponent<CitizenAvatar>().MakeCitizenMove (this.location, this.roads [0].hexTile);
				this.location = this.roads[0];
				this.roads.RemoveAt (0);
				if(this.location == this.targetLocation){
					this.warLeader.campaignManager.GeneralHasArrived (this.citizen);
					return;
				}else{
					if(this.location.city != null && this.location.isOccupied){
						if(this.location.city.id != this.citizen.city.id){
							bool isDead = false;
							int armyNumber = this.army.hp;
							this.location.city.CheckBattleMidwayCity ();
							//							this.location.city.CheckBattleMidwayCity (this, ref armyNumber, ref isDead);
							if(!isDead){
								this.army.hp = armyNumber;
							}
						}
					}
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
