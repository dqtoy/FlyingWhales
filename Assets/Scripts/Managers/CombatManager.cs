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
		General victoriousGeneral = null;
		General friendlyGeneral = null;
		List<General> attackers = city.incomingGenerals.Where (x => x.assignedCampaign == CAMPAIGN.OFFENSE && x.location == city.hexTile && x.targetCity.id == city.id).ToList();
		List<General> defenders = new List<General>();

		for(int i = 0; i < attackers.Count; i++){
			General attackerGeneral = attackers [i];
			defenders.Clear ();
			defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign == CAMPAIGN.DEFENSE && x.location == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
			defenders.AddRange (city.incomingGenerals.Where (x => x.assignedCampaign == CAMPAIGN.OFFENSE && x.location == city.hexTile && x.rallyPoint == city.hexTile && x.citizen.city.governor.id != attackerGeneral.citizen.city.governor.id).ToList ());
			defenders.AddRange (city.GetAllGenerals (attackerGeneral));
			defenders = defenders.OrderByDescending (x => x.army.hp).ToList();
			if(defenders.Count > 0){
				List<General> newDefenders = new List<General> (defenders);
				GoToBattle (ref newDefenders, ref attackerGeneral);
				attackers [i] = attackerGeneral;
				defenders = newDefenders;
				if (defenders.Count <= 0 && attackers[i].army.hp > 0) {
					//CITY DEFEATED AND CONQUERED --- IF ENEMY IS STILL ALIVE, AND YOU HAVE NO GENERALS LEFT
					victoriousGeneral = attackers[i];
					Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.citizen.city.name);
				}
			}else{
				if(victoriousGeneral == null){
					victoriousGeneral = attackers [i];
				}else{
					friendlyGeneral = victoriousGeneral;
					if(friendlyGeneral.citizen.city.id != attackers[i].citizen.city.id){
						if(attackers [i].warType == WAR_TYPE.INTERNATIONAL){
							if (!Utilities.AreTwoGeneralsFriendly (attackers [i], friendlyGeneral)) {
								if (!Utilities.AreTwoGeneralsFriendly (friendlyGeneral, attackers [i])) {
									Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
									Battle (ref friendlyGeneral, ref attackerGeneral);
									attackers[i] = attackerGeneral;
									if (attackers[i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
										victoriousGeneral = null;
									} else if (attackers[i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
										victoriousGeneral = friendlyGeneral;
									} else if (attackers[i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
										victoriousGeneral = attackers[i];
									}
									Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
								}
							}
						}else if(attackers [i].warType == WAR_TYPE.SUCCESSION){
							if(attackers [i].warLeader.id != friendlyGeneral.warLeader.id){
								if(!Utilities.AreTwoGeneralsFriendly(attackers [i], friendlyGeneral)){
									if(!Utilities.AreTwoGeneralsFriendly(friendlyGeneral, attackers [i])){
										Debug.Log ("CITY IS FOR TAKING! NO MORE GENERALS! BATTLE FOR OWNERSHIP!");
										Battle (ref friendlyGeneral, ref attackerGeneral);
										attackers [i] = attackerGeneral;
										if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp <= 0) {
											victoriousGeneral = null;
										} else if (attackers [i].army.hp <= 0 && friendlyGeneral.army.hp > 0) {
											victoriousGeneral = friendlyGeneral;
										} else if (attackers [i].army.hp > 0 && friendlyGeneral.army.hp <= 0) {
											victoriousGeneral = attackers [i];
										}
										Debug.Log ("WINNER: " + victoriousGeneral.citizen.city.name);
									}
								}
							}
						}
					}
				}
			}
		}
		if (victoriousGeneral != null) {
			if (victoriousGeneral.warType == WAR_TYPE.INTERNATIONAL) {
				for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
					KingdomManager.Instance.allKingdoms [i].king.campaignManager.intlWarCities.RemoveAll (x => x.city.id == city.id);
					KingdomManager.Instance.allKingdoms [i].king.campaignManager.defenseWarCities.RemoveAll (x => x.city.id == city.id);
				}
				EventManager.Instance.onRemoveSuccessionWarCity.Invoke (city);
			}
		}

		EventManager.Instance.onDeathArmy.Invoke ();
		city.incomingGenerals.RemoveAll(x => x.army.hp <= 0);
		for(int i = 0; i < city.incomingGenerals.Count; i++){
			if(victoriousGeneral != null){
				if(victoriousGeneral.warType == WAR_TYPE.INTERNATIONAL){
					if(city.incomingGenerals[i].warType == WAR_TYPE.INTERNATIONAL){
						if (city.incomingGenerals[i].citizen.id != victoriousGeneral.citizen.id) {
							if (city.incomingGenerals [i].campaignID != victoriousGeneral.campaignID) {
								Campaign campaign = city.incomingGenerals [i].warLeader.campaignManager.SearchCampaignByID (city.incomingGenerals [i].campaignID);
								if (campaign != null) {
									campaign.leader.campaignManager.CampaignDone (campaign);
								}
							}
						}
					}
				}
			}
		}


		if(victoriousGeneral != null){
			Debug.Log (city.name + " IS DEFEATED BY " + victoriousGeneral.citizen.city.name);
//			if(((General)victoriousGeneral.assignedRole).warLeader.city.kingdom.id == city.kingdom.id){
//				//CIVIL WAR OR SUCCESSION WAR, SEARCH FOR TARGET
//			}
			Campaign campaign = victoriousGeneral.warLeader.campaignManager.SearchCampaignByID (victoriousGeneral.campaignID);

			if (campaign.warType == WAR_TYPE.INTERNATIONAL) {
				if (campaign != null) {
					campaign.leader.campaignManager.CampaignDone (campaign);
				}


				int countCitizens = city.citizens.Count;
				for (int i = 0; i < countCitizens; i++) {
					city.citizens [0].Death (DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
				}
				city.incomingGenerals.Clear ();
				city.isDead = true;
				ConquerCity (victoriousGeneral.citizen.city.kingdom, city);

			} else if (campaign.warType == WAR_TYPE.SUCCESSION) {
				victoriousGeneral.target = victoriousGeneral.warLeader.GetTargetSuccessionWar (campaign.targetCity);
				EventManager.Instance.onWeekEnd.AddListener (victoriousGeneral.SearchForTarget);
			}

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
	internal void GoToBattle(ref List<General> friendlyGenerals, ref General enemyGeneral){
		List<General> deadFriendlies = new List<General> ();

		General friendlyGeneral = null;

		for(int j = 0; j < friendlyGenerals.Count; j++){

			friendlyGeneral = friendlyGenerals [j];

			Battle (ref friendlyGeneral, ref enemyGeneral);
			if (friendlyGeneral.army.hp <= 0) {
				friendlyGenerals.Remove (friendlyGeneral);
				j--;
			}
			if(enemyGeneral.army.hp <= 0){
				break;
			}
		}
	}
	internal void DeathByBattle(General general, City city){
		city.incomingGenerals.Remove (general);
		Campaign campaign = general.warLeader.campaignManager.SearchCampaignByID (general.campaignID);
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
	internal void Battle(ref General general1, ref General general2, bool isMidway = false){
		Debug.Log ("BATTLE: (" + general1.citizen.city.name + ") " + general1.citizen.name + " and (" + general2.citizen.city.name + ") " + general2.citizen.name);
		Debug.Log ("enemy general army: " + general1.army.hp);
		Debug.Log ("friendly general army: " + general2.army.hp);

		float general1HPmultiplier = 1f;
		float general2HPmultiplier = 1f;

		if(general1.assignedCampaign == CAMPAIGN.DEFENSE){
			general1HPmultiplier = 1.20f; //Utilities.defenseBuff
		}
		if(general2.assignedCampaign == CAMPAIGN.DEFENSE){
			general2HPmultiplier = 1.20f; //Utilities.defenseBuff
		}

		int general1TotalHP = (int)(general1.GetArmyHP() * general1HPmultiplier);
		int general2TotalHP = (int)(general2.GetArmyHP() * general2HPmultiplier);

		if(general1TotalHP > general2TotalHP){
			general1.army.hp -= general2.army.hp;
			general2.army.hp = 0;
		}else{
			general2.army.hp -= general1.army.hp;
			general1.army.hp = 0;
		}

		RelationshipKingdom kingdomRelationshipToGeneral2 = general1.citizen.city.kingdom.GetRelationshipWithOtherKingdom(general2.citizen.city.kingdom);
		RelationshipKingdom kingdomRelationshipToGeneral1 = general2.citizen.city.kingdom.GetRelationshipWithOtherKingdom(general1.citizen.city.kingdom);


		if(general1.army.hp <= 0){
			//BATTLE LOST
			kingdomRelationshipToGeneral2.kingdomWar.battlesLost += 1;
			if(isMidway){
				kingdomRelationshipToGeneral2.AdjustExhaustion (10);
			}else{
				kingdomRelationshipToGeneral2.AdjustExhaustion (10);
			}


		}else{
			//BATTLE WON
			kingdomRelationshipToGeneral2.kingdomWar.battlesWon += 1;
			if(isMidway){
				kingdomRelationshipToGeneral2.AdjustExhaustion (-5);
			}else{
				kingdomRelationshipToGeneral2.AdjustExhaustion (-5);
			}
		}

		if(general2.army.hp <= 0){
			//BATTLE LOST
			kingdomRelationshipToGeneral1.kingdomWar.battlesLost += 1;
			if(isMidway){
				kingdomRelationshipToGeneral1.AdjustExhaustion (10);
			}else{
				kingdomRelationshipToGeneral1.AdjustExhaustion (10);
			}
		}else{
			//BATTLE WON
			kingdomRelationshipToGeneral1.kingdomWar.battlesWon += 1;
			if(isMidway){
				kingdomRelationshipToGeneral1.AdjustExhaustion (-5);
			}else{
				kingdomRelationshipToGeneral1.AdjustExhaustion (-10);
			}
		}

		int chanceToTriggerSendSpy = Random.Range (0, 100);
		if (chanceToTriggerSendSpy < 10) {
			List<Kingdom> kingdom1Enemies = general1.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.ENEMY);
			kingdom1Enemies.Union (general1.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.RIVAL));

			List<Kingdom> kingdom2Enemies = general2.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.ENEMY);
			kingdom2Enemies.Union (general2.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.RIVAL));

			List<Kingdom> possibleKingdomsToSendSpy = kingdom1Enemies.Except (kingdom2Enemies).Union(kingdom2Enemies.Except (kingdom1Enemies)).ToList();

			for (int i = 0; i < possibleKingdomsToSendSpy.Count; i++) {
				int chance = Random.Range(0, 100);
				Kingdom possibleKingdomToTrigger = possibleKingdomsToSendSpy[i];
				List<Citizen> spies = possibleKingdomToTrigger.GetAllCitizensOfType (ROLE.SPY).Where(x => !((Spy)x.assignedRole).inAction).ToList();
				if (spies.Count > 0 && chance < 5) {
					//Send spy to kingdom that is not enemy
					if (possibleKingdomToTrigger.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
						possibleKingdomToTrigger.king.GetRelationshipWithCitizen (general1.citizen.city.kingdom.king).lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						((Spy)spies [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral2);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent a spy(" + spies [0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));
						
						Debug.Log(possibleKingdomToTrigger.name + " sent a spy(" + spies[0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name);
					} else {
						((Spy)spies [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral1);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent a spy(" + spies [0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent a spy(" + spies[0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name);
					}
				}
			}
		}

		int chanceToTriggerSendEnvoy = Random.Range (0, 100);
		if (chanceToTriggerSendEnvoy < 10) {
			List<Kingdom> kingdom1Friends = general1.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.FRIEND);
			kingdom1Friends.Union (general1.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.ALLY));

			List<Kingdom> kingdom2Friends = general2.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.FRIEND);
			kingdom2Friends.Union (general2.citizen.city.kingdom.GetKingdomsByRelationship (RELATIONSHIP_STATUS.ALLY));

			List<Kingdom> commonFriends = kingdom1Friends.Intersect(kingdom2Friends).ToList();
			for (int i = 0; i < commonFriends.Count; i++) {
				int chance = Random.Range(0, 100);
				Kingdom possibleKingdomToTrigger = commonFriends[i];
				List<Citizen> envoys = possibleKingdomToTrigger.GetAllCitizensOfType (ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
				if (envoys.Count > 0 && chance < 5) {
					if (Random.Range (0, 2) == 0) {
						((Envoy)envoys [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral2);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent an envoy(" + envoys [0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent an envoy(" + envoys[0].name + ") to " + general2.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general1.citizen.city.kingdom.name);
					} else {
						((Envoy)envoys [0].assignedRole).StartDecreaseWarExhaustionTask (kingdomRelationshipToGeneral1);

						possibleKingdomToTrigger.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year,
							possibleKingdomToTrigger.name + " sent an envoy(" + envoys [0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name, HISTORY_IDENTIFIER.NONE));

						Debug.Log(possibleKingdomToTrigger.name + " sent a envoy(" + envoys[0].name + ") to " + general1.citizen.city.kingdom.name + " to decrease exhaustion in his war" +
							" against " + general2.citizen.city.kingdom.name);
					}
				}
			}

		}

		Debug.Log ("RESULTS: " + general1.citizen.name + " army hp left: " + general1.army.hp + "\n" + general2.citizen.name + " army hp left: " + general2.army.hp);
	}

	internal void BattleMidway(General general1, General general2){
		//MID WAY BATTLE IF supported is not the same
		Debug.Log("BATTLE MIDWAY!");

		Battle(ref general1, ref general2, true);

		if(general1.army.hp <= 0){
			if (general1.generalAvatar != null) {
				GameObject.Destroy (general1.generalAvatar);
				general1.generalAvatar = null;
			}
			general1.citizen.Death(DEATH_REASONS.BATTLE);
		}

		if(general2.army.hp <= 0){
			if (general2.generalAvatar != null) {
				GameObject.Destroy (general2.generalAvatar);
				general2.generalAvatar = null;
			}
			general2.citizen.Death(DEATH_REASONS.BATTLE);
		}
	}

}
