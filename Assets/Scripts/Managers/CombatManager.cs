using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour {
	public static CombatManager Instance;

	void Awake(){
		Instance = this;
	}
	internal void CityBattle(City city){
		Citizen victoriousGeneral = null;
		Citizen friendlyGeneral = null;
		List<Citizen> attackers = city.incomingGenerals.Where (x => ((General)x.assignedRole).assignedCampaign == CAMPAIGN.OFFENSE && ((General)x.assignedRole).location == city.hexTile).ToList();
		List<Citizen> defenders = new List<Citizen>();
		defenders.AddRange (city.incomingGenerals.Where (x => ((General)x.assignedRole).assignedCampaign == CAMPAIGN.DEFENSE && ((General)x.assignedRole).location == city.hexTile).ToList ());
		defenders.AddRange (city.incomingGenerals.Where (x => ((General)x.assignedRole).assignedCampaign == CAMPAIGN.OFFENSE && ((General)x.assignedRole).location == city.hexTile && ((General)x.assignedRole).rallyPoint == city.hexTile).ToList ());
		defenders.AddRange (city.GetAllGenerals ());
		defenders = defenders.OrderByDescending (x => ((General)x.assignedRole).army.hp).ToList();
		for(int i = 0; i < attackers.Count; i++){
			Citizen attackerGeneral = attackers [i];
			if(defenders.Count > 0){
				List<Citizen> newDefenders = new List<Citizen> (defenders);
				GoToBattle (ref newDefenders, ref attackerGeneral);
				attackers [i] = attackerGeneral;
				defenders = newDefenders;
				if (defenders.Count <= 0 && ((General)attackers[i].assignedRole).army.hp > 0) {
					//CITY DEFEATED AND CONQUERED --- IF ENEMY IS STILL ALIVE, AND YOU HAVE NO GENERALS LEFT
					victoriousGeneral = attackers[i];
					Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.city.name);
				}
			}else{
				if(victoriousGeneral == null){
					victoriousGeneral = attackers [i];
				}else{
					friendlyGeneral = victoriousGeneral;
					if(friendlyGeneral.city.id != attackers[i].city.id){
						if(((General)attackers [i].assignedRole).warType == WAR_TYPE.INTERNATIONAL){
							if (attackers [i].city.kingdom.CheckForSpecificWar(friendlyGeneral.city.kingdom)) {
								Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
								Battle (ref attackerGeneral, ref friendlyGeneral);
								attackers[i] = attackerGeneral;
								if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
									victoriousGeneral = null;
								} else if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp > 0) {
									victoriousGeneral = friendlyGeneral;
								} else if (((General)attackers[i].assignedRole).army.hp > 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
									victoriousGeneral = attackers[i];
								}
								Debug.Log ("WINNER: " + victoriousGeneral.city.name);
							}
						}else if(((General)attackers [i].assignedRole).warType == WAR_TYPE.SUCCESSION){
							if(((General)attackers [i].assignedRole).warLeader.id != ((General)friendlyGeneral.assignedRole).warLeader.id){
								if (((General)attackers [i].assignedRole).warLeader.SearchForSuccessionWar(((General)friendlyGeneral.assignedRole).warLeader)) {
									Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
									Battle (ref attackerGeneral, ref friendlyGeneral);
									attackers[i] = attackerGeneral;
									if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
										victoriousGeneral = null;
									} else if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp > 0) {
										victoriousGeneral = friendlyGeneral;
									} else if (((General)attackers[i].assignedRole).army.hp > 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
										victoriousGeneral = attackers[i];
									}
									Debug.Log ("WINNER: " + victoriousGeneral.city.name);
								}else{
									if (attackers [i].city.kingdom.CheckForSpecificWar (friendlyGeneral.city.kingdom)) {
										Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
										Battle (ref attackerGeneral, ref friendlyGeneral);
										attackers[i] = attackerGeneral;
										if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
											victoriousGeneral = null;
										} else if (((General)attackers[i].assignedRole).army.hp <= 0 && ((General)friendlyGeneral.assignedRole).army.hp > 0) {
											victoriousGeneral = friendlyGeneral;
										} else if (((General)attackers[i].assignedRole).army.hp > 0 && ((General)friendlyGeneral.assignedRole).army.hp <= 0) {
											victoriousGeneral = attackers[i];
										}
										Debug.Log ("WINNER: " + victoriousGeneral.city.name);
									}
								}
							}

						}
					}
				}
			}
		}
		city.incomingGenerals.RemoveAll(x => ((General)x.assignedRole).army.hp <= 0);
		for(int i = 0; i < city.incomingGenerals.Count; i++){
			if(victoriousGeneral != null){
				if(((General)victoriousGeneral.assignedRole).warType == WAR_TYPE.INTERNATIONAL){
					if(((General)city.incomingGenerals[i].assignedRole).warType == WAR_TYPE.INTERNATIONAL){
						Campaign campaign = ((General)city.incomingGenerals[i].assignedRole).warLeader.campaignManager.SearchCampaignByID (((General)city.incomingGenerals[i].assignedRole).campaignID);
						if(campaign != null){
							campaign.leader.campaignManager.CampaignDone (campaign);
						}
					}
				}
			}
		}

		EventManager.Instance.onDeathArmy.Invoke ();
		if(victoriousGeneral != null){
			Debug.Log (city.name + " IS CONQUERED BY " + victoriousGeneral.city.name);
//			if(((General)victoriousGeneral.assignedRole).warLeader.city.kingdom.id == city.kingdom.id){
//				//CIVIL WAR OR SUCCESSION WAR, SEARCH FOR TARGET
//			}
			Campaign campaign = ((General)victoriousGeneral.assignedRole).warLeader.campaignManager.SearchCampaignByID (((General)victoriousGeneral.assignedRole).campaignID);
			if(campaign != null){
				campaign.leader.campaignManager.CampaignDone (campaign);
			}

			for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
				KingdomManager.Instance.allKingdoms[i].king.campaignManager.intlWarCities.RemoveAll(x => x.city.id == city.id);
				KingdomManager.Instance.allKingdoms[i].king.campaignManager.defenseWarCities.RemoveAll(x => x.city.id == city.id);
			}

			int countCitizens = city.citizens.Count;
			for(int i = 0; i < countCitizens; i++){
				city.citizens[0].Death(DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
			}
			city.incomingGenerals.Clear();
			city.isDead = true;
			ConquerCity (victoriousGeneral.city.kingdom, city);
//			for(int i = 0; i < city.incomingGenerals.Count; i++){
//				Campaign campaign = ((General)city.incomingGenerals[i].assignedRole).warLeader.campaignManager.SearchCampaignByID (((General)city.incomingGenerals[i].assignedRole).campaignID);
//				campaign.leader.campaignManager.CampaignDone (campaign);
//			}

//			victoriousGeneral.city.kingdomTile.kingdom.lord.RemoveMilitaryData (BATTLE_MOVE.ATTACK, this, null);
//			this.kingdomTile.kingdom.lord.RemoveMilitaryData (BATTLE_MOVE.DEFEND, null, victoriousGeneral);
//			victoriousGeneral.taskID = 0;
//			victoriousGeneral.task = GENERAL_TASK.NONE;
//			victoriousGeneral.targetCity = null;
//			victoriousGeneral.city.targetCity = null;
//			this.generals.Clear ();
//			this.isDead = true;
//			ConquerCity (victoriousGeneral.city.kingdomTile, this.visitingGenerals);
		}
	}
	internal void GoToBattle(ref List<Citizen> friendlyGenerals, ref Citizen enemyGeneral){
		List<Citizen> deadFriendlies = new List<Citizen> ();

		Citizen friendlyGeneral = null;

		for(int j = 0; j < friendlyGenerals.Count; j++){

			friendlyGeneral = friendlyGenerals [j];

			Battle (ref enemyGeneral, ref friendlyGeneral);
			if (((General)friendlyGeneral.assignedRole).army.hp <= 0) {
				friendlyGenerals.Remove (friendlyGeneral);
				j--;
			}
			if(((General)enemyGeneral.assignedRole).army.hp <= 0){
				break;
			}
		}
	}
	internal void DeathByBattle(Citizen general, City city){
		city.incomingGenerals.Remove (general);
		Campaign campaign = ((General)general.assignedRole).warLeader.campaignManager.SearchCampaignByID (((General)general.assignedRole).campaignID);
		if(campaign != null){
			campaign.registeredGenerals.Remove (general);
			if(campaign.registeredGenerals.Count <= 0){
				campaign.leader.campaignManager.CampaignDone (campaign);
			}
		}
	}
	internal void ConquerCity(Kingdom conqueror, City city){
		conqueror.ConquerCity(city);
	}
	internal void Battle(ref Citizen general1, ref Citizen general2){
		Debug.Log ("BATTLE: (" + general1.city.name + ") " + general1.name + " and (" + general2.city.name + ") " + general2.name);
		Debug.Log ("enemy general army: " + ((General)general1.assignedRole).army.hp);
		Debug.Log ("friendly general army: " + ((General)general2.assignedRole).army.hp);

		float general1HPmultiplier = 1f;
		float general2HPmultiplier = 1f;

		if(((General)general1.assignedRole).assignedCampaign == CAMPAIGN.DEFENSE){
			general1HPmultiplier = 1.20f; //Utilities.defenseBuff
		}
		if(((General)general2.assignedRole).assignedCampaign == CAMPAIGN.DEFENSE){
			general2HPmultiplier = 1.20f; //Utilities.defenseBuff
		}

		int general1TotalHP = (int)(((General)general1.assignedRole).GetArmyHP() * general1HPmultiplier);
		int general2TotalHP = (int)(((General)general2.assignedRole).GetArmyHP() * general2HPmultiplier);

		if(general1TotalHP > general2TotalHP){
			((General)general1.assignedRole).army.hp -= ((General)general2.assignedRole).army.hp;
			((General)general2.assignedRole).army.hp = 0;
		}else{
			((General)general2.assignedRole).army.hp -= ((General)general1.assignedRole).army.hp;
			((General)general1.assignedRole).army.hp = 0;
		}
		Debug.Log ("RESULTS: " + general1.name + " army hp left: " + ((General)general1.assignedRole).army.hp + "\n" + general2.name + " army hp left: " + ((General)general2.assignedRole).army.hp);
	}

	internal void BattleMidway(Citizen general1, Citizen general2){
		//MID WAY BATTLE IF supported is not the same
		Debug.Log("BATTLE MIDWAY!");

		Battle(ref general1, ref general2);

		if(((General)general1.assignedRole).army.hp <= 0){
			general1.Death(DEATH_REASONS.BATTLE);
		}

		if(((General)general2.assignedRole).army.hp <= 0){
			general2.Death(DEATH_REASONS.BATTLE);
		}
	}
}
