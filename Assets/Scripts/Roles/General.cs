using UnityEngine;
using System.Collections;

public class General : Role {
	public City targetCity;
	public HexTile location;
	public HexTile targetLocation;
//	public List<Tile> roads;
	public Army army;
	public Citizen warLeader;
	public int campaignID;
	public CAMPAIGN assignedCampaign;
	public int daysBeforeArrival;
	public int daysBeforeReleaseTask;
	public bool isOnTheWay;
	public GameObject generalAvatar;

	public General(Citizen citizen): base(citizen){
		this.location = citizen.assignedTile;
		this.targetLocation = null;
		this.targetCity = null;
		this.warLeader = null;
		this.army = new Army (GetInitialArmyHp());
		this.campaignID = 0;
		this.assignedCampaign = CAMPAIGN.NONE;
		this.isOnTheWay = false;
		this.daysBeforeArrival = 0;
		this.daysBeforeReleaseTask = 0;
//		this.roads = new List<Tile> ();
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
			if(this.location == this.targetLocation){
				this.warLeader.campaignManager.GeneralHasArrived (this.citizen);
			}
		}
	}
}
