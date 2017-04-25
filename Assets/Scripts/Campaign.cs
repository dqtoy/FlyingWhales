using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Campaign {
	public int id;
	public Citizen leader;
	public City targetCity;
	public List<General> registeredGenerals;
	public CAMPAIGN campaignType;
	public WAR_TYPE warType;
	public HexTile rallyPoint;
	public bool isFull;
	public bool hasStarted;
	public int neededArmyStrength;
	public int expiration;
	public bool isGhost;

	public Campaign(Citizen leader, City targetCity, CAMPAIGN campaignType, WAR_TYPE warType, int neededArmyStrength = 0, int expiration = 8){
		this.id = Utilities.SetID (this);
		this.leader = leader;
		this.targetCity = targetCity;
		this.campaignType = campaignType;
		this.warType = warType;
		this.registeredGenerals = new List<General> ();
		this.isFull = false;
		this.hasStarted = false;
		this.rallyPoint = null;
		this.neededArmyStrength = neededArmyStrength;
		this.expiration = expiration;
		this.isGhost = false;
		EventManager.Instance.onWeekEnd.AddListener (this.CheckExpiration);
	}

	internal int GetArmyStrength(){
		int total = 0;
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			total += this.registeredGenerals[i].GetArmyHP();
		}
		return total;
	}
	internal void GoToRallyPoint(){
		
	}

	internal void CheckExpiration(){
		if(this.campaignType == CAMPAIGN.DEFENSE){
			if(this.expiration >= 0){
				AdjustExpiration (-1);
			}
		}else{
			if (this.registeredGenerals.Count <= 0) {
				AdjustExpiration (-1);
			} else if (this.targetCity != null) {
				if (this.targetCity.isDead) {
					AdjustExpiration (-1);
				}
			} else if (this.targetCity == null) {
				AdjustExpiration (-1);
			}	
		}

	}

	private void AdjustExpiration(int amount){
		this.expiration += amount;
		if(this.expiration <= 0){
			if(!this.isGhost){
				Debug.Log (this.leader.name + " " + this.campaignType.ToString () + " campaign for " + this.targetCity.name + " has expired!");
			}else{
				Debug.Log (this.leader.name + " " + this.campaignType.ToString () + " campaign has expired!");
			}
			this.expiration = 0;
			this.leader.campaignManager.CampaignDone (this);
		}
	}

	internal bool AreAllGeneralsOnRallyPoint(){
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			if(this.registeredGenerals[i].location != this.rallyPoint){
				return false;
			}
		}
		return true;
	}
	internal bool AreAllGeneralsOnDefenseCity(){
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			if(this.registeredGenerals[i].location != this.targetCity.hexTile){
				return false;
			}
		}
		return true;
	}
	internal void AttackCityNow(){
		List<HexTile> path = PathGenerator.Instance.GetPath (this.rallyPoint, this.targetCity.hexTile, PATHFINDING_MODE.COMBAT);

		for(int i = 0; i < this.registeredGenerals.Count; i++){
			if (path != null) {
				this.registeredGenerals[i].targetLocation = this.targetCity.hexTile;
				this.registeredGenerals [i].roads.Clear ();
				this.registeredGenerals [i].roads = path;
				this.registeredGenerals [i].daysBeforeArrival = path.Count;

				if(this.registeredGenerals[i].generalAvatar == null){
					this.registeredGenerals [i].generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.registeredGenerals [i].location.transform) as GameObject;
					this.registeredGenerals [i].generalAvatar.transform.localPosition = Vector3.zero;
					this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject>().general = this.registeredGenerals [i];
					this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject> ().Init();
				}else{
					this.registeredGenerals [i].generalAvatar.transform.parent = this.registeredGenerals [i].location.transform;
					this.registeredGenerals [i].generalAvatar.transform.localPosition = Vector3.zero;
				}


			}
		}

		//remove from rally point
		if(this.rallyPoint != null){
			if(this.rallyPoint.isOccupied){
				for(int i = 0; i < this.registeredGenerals.Count; i++){
					this.targetCity.incomingGenerals.Add (this.registeredGenerals[i]);
					this.rallyPoint.city.incomingGenerals.Remove(this.registeredGenerals[i]);
				}

			}
		}
	}
}
