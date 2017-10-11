using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SeeksSuperiority {

	public static void Initialize(Kingdom kingdom){
		bool skipPhase2 = false;
		bool skipPhase3 = false;
		bool hasAllianceInWar = false;
		Phase1 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
	}

	private static void Phase1(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool hasAllianceInWar){
		Debug.Log("========== " + kingdom.name + " is seeking superiority " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ==========");
		Debug.Log ("========== PHASE 1 ==========");      
		//break any alliances with anyone whose threat value is 100 or above and lose 10 Stability
		if(kingdom.alliancePool != null){
			for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
				if(kingdom.id != allyKingdom.id){
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(allyKingdom);
					if(kr.targetKingdomInvasionValue >= 100f || kr.totalLike <= -100){
						kingdom.LeaveAlliance ();
						kingdom.AdjustStability(-10);
						Debug.Log(kingdom.name + " broke alliance with " + allyKingdom.name +
							" because its invasion value is " + kr.targetKingdomInvasionValue.ToString() + " or total like is " + kr.totalLike.ToString() + "," + kingdom.name + 
							" lost 10 Stability. Stability is now " + kingdom.stability.ToString());
						break;
					}
				}
			}
		}

		//if i am under attack and i am not attacking anyone
		bool isUnderAttack = kingdom.IsUnderAttack();
		bool isAttacking = kingdom.IsAttacking ();
		bool mustSeekAlliance = false;
		Kingdom seekWarKingdom = null;
		Kingdom targetKingdom = null;
		int leastLike = 0;
		int leastLike2 = 0;
		if (isUnderAttack && !isAttacking) {
			if (!kingdom.isFortifying) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					Debug.Log(kingdom.name + " is fortifying because it is under attack and not attacking!");
					kingdom.Fortify (true, isUnderAttack);
				}
			}
		} else if (!isUnderAttack && isAttacking) {
			if (!kingdom.isMilitarize) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					Debug.Log(kingdom.name + " is militarizing because it is attacking and not under attack!");
					kingdom.Militarize (true, isAttacking);
				}
			}
		} else {
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(relationship.isDiscovered){
					if(relationship.targetKingdomThreatLevel >= 100f && relationship.targetKingdomInvasionValue < 75f){
						if(!relationship.AreAllies()){
							mustSeekAlliance = true;
							break;
						}
					}else if(relationship.targetKingdomThreatLevel >= 50f && relationship.targetKingdomInvasionValue >= 25f){
						if (!relationship.AreAllies () && relationship.warfare == null) {
							if(seekWarKingdom == null){
								seekWarKingdom = relationship.targetKingdom;
								leastLike = relationship.totalLike;
							}else{
								if(relationship.totalLike < leastLike){
									seekWarKingdom = relationship.targetKingdom;
									leastLike = relationship.totalLike;
								}
							}
						}
					}
					if(relationship.targetKingdomInvasionValue > 0f){
						if (!relationship.AreAllies ()) {
							if(targetKingdom == null){
								targetKingdom = relationship.targetKingdom;
								leastLike2 = relationship.totalLike;
							}else{
								if(relationship.totalLike < leastLike2){
									targetKingdom = relationship.targetKingdom;
									leastLike2 = relationship.totalLike;
								}
							}
						}
					}
				}
			}

			if(mustSeekAlliance){
				//if i am not part of any alliance, create or join an alliance if possible
				if(kingdom.alliancePool == null){
					Debug.Log(kingdom.name + " is seeking alliance because it has no allies, there is a kingdom that has 100 or above threat and a less than 75 invasion value");
					kingdom.SeekAlliance ();
					if(kingdom.alliancePool != null){
						skipPhase2 = true;
					}
				}
				if (!kingdom.isFortifying) {
					int chance = UnityEngine.Random.Range (0, 2);
					if (chance == 0) {
						Debug.Log(kingdom.name + " is fortifying because it has no allies, there is a kingdom that has 100 or above threat and a less than 75 invasion value");
						kingdom.Fortify (true);
					}
				}
			}else{
				if(seekWarKingdom != null){
					skipPhase3 = true;
					Debug.Log(kingdom.name + " is seeking war with " + seekWarKingdom.name + " because it is the least liked kingdom with threat 50 or above, and 25 or above invasion value");
					Warfare warfare = new Warfare (kingdom, seekWarKingdom);
				}else{
					//if there are kingdoms whose invasion value is 75 or above that is not part of my alliance
					if(targetKingdom != null){
						Debug.Log(kingdom.name + " wants to have alliance with anyone against " + targetKingdom.name + " because it has positive invasion value and is not my ally");
						if(!kingdom.isMilitarize){
							int chance = UnityEngine.Random.Range (0, 2);
							if (chance == 0) {
								Debug.Log(kingdom.name + " militarizes against " + targetKingdom.name + " because it has positive invasion value and is not my ally");
								kingdom.Militarize (true);
							}
						}
						if(kingdom.alliancePool == null){
							Kingdom kingdomToAlly = null;
							int leastLikedToEnemy = 0;
							foreach (KingdomRelationship krToAlly in kingdom.relationships.Values) {
								if(krToAlly.targetKingdom.id != targetKingdom.id){
									KingdomRelationship krFromAlly = krToAlly.targetKingdom.GetRelationshipWithKingdom (kingdom);
									KingdomRelationship krEnemy = krToAlly.targetKingdom.GetRelationshipWithKingdom (targetKingdom);
									if(krToAlly.totalLike > 0 && krFromAlly.totalLike > 0 && krEnemy.isAdjacent 
										&& krToAlly.targetKingdom.king.balanceType == PURPOSE.SUPERIORITY && KingdomManager.Instance.kingdomRankings[0].id != krToAlly.targetKingdom.id){
										if(kingdomToAlly == null){
											kingdomToAlly = krToAlly.targetKingdom;
											leastLikedToEnemy = krEnemy.totalLike;
										}else{
											if(krEnemy.totalLike < leastLikedToEnemy){
												kingdomToAlly = krToAlly.targetKingdom;
												leastLikedToEnemy = krEnemy.totalLike;
											}
										}
									}
								}
							}
							if(kingdomToAlly != null){
								Debug.Log(kingdom.name + " seeks alliance of conquest with " + kingdomToAlly.name);
								kingdom.SeekAllianceWith (kingdomToAlly);
							}
						}
					}
				}
			}
		}

		if(!skipPhase2){
			Phase2 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
		}else{
			if(!skipPhase3){
				Phase3 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
			}else{
				Debug.Log("==========SKIPPED PHASE 3 END SEEKS SUPERIORITY " + kingdom.name + " ==========");
			}
		}

	}

	private static void Phase2(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool hasAllianceInWar){
		Debug.Log ("========== PHASE 2 ==========");
		if(kingdom.alliancePool != null){
			bool hasLeftAlliance = false;
			bool isKingdomHasWar = kingdom.HasWar ();
			List<WarfareInfo> warsToJoin = new List<WarfareInfo> ();
			for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
				if (kingdom.id != allyKingdom.id) {
					if (allyKingdom.warfareInfo.Count > 0) {
						hasAllianceInWar = true;
						foreach (WarfareInfo info in allyKingdom.warfareInfo.Values) {
							if (!kingdom.warfareInfo.ContainsKey (info.warfare.id) && !warsToJoin.Contains (info)) {
								WAR_SIDE allySide = info.side;
								WAR_SIDE enemySide = WAR_SIDE.A;
								if (allySide == WAR_SIDE.A) {
									enemySide = WAR_SIDE.B;
								}
								List<Kingdom> enemySideKingdoms = info.warfare.GetListFromSide (enemySide);

								int totalLikeToAllies = info.warfare.GetTotalLikeOfKingdomToSide (kingdom, allySide) * 2;
								int totalLikeToEnemies = info.warfare.GetTotalLikeOfKingdomToSide (kingdom, enemySide);

								int totalChanceOfJoining = totalLikeToAllies - totalLikeToEnemies;
								int chance = UnityEngine.Random.Range (0, 100);
								if (chance < totalChanceOfJoining) {
									//Join War
									warsToJoin.Add (info);
								} else {
									//Don't join war, lose 10 Stability
									Debug.Log(kingdom.name + " refused to join " + info.warfare.name + " and loses 10 Stability");
									kingdom.AdjustStability (-10);
									if (totalLikeToEnemies > 0) {
										int leavingValue = totalLikeToEnemies * 2;
										int chanceOfLeaving = UnityEngine.Random.Range (0, 100);

										if (totalLikeToAllies >= 0) {
											//Refuse to participate in war and leave alliance
											if (chanceOfLeaving < leavingValue) {
												Debug.Log(kingdom.name + " left " + kingdom.alliancePool.name);
												kingdom.ShowRefuseAndLeaveAllianceLog (kingdom.alliancePool, info.warfare);
												kingdom.LeaveAlliance (true);
												hasLeftAlliance = true;
												break;
											} else {
												kingdom.ShowDoNothingLog (info.warfare);
											}
										} else {
											//Leave alliance and join enemy's side
											if (chanceOfLeaving < leavingValue) {
												Debug.Log(kingdom.name + " betrayed his alliance by joining the other side");

												kingdom.AdjustStability (-10);
												AlliancePool allianceOfSourceKingdom = kingdom.alliancePool;
												kingdom.LeaveAlliance (true);
												hasLeftAlliance = true;
												Kingdom enemyKingdom = enemySideKingdoms [0];
												if (enemyKingdom.alliancePool != null) {
													enemyKingdom.alliancePool.AttemptToJoinAlliance (kingdom, enemyKingdom);
												} else {
													KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms (kingdom, enemyKingdom);
												}

												if (kingdom.alliancePool.HasAdjacentAllianceMember (kingdom)) {
													Debug.Log(kingdom.name + " joined the " + info.warfare.name + " against its former alliance members");
													info.warfare.JoinWar (enemySide, kingdom);
													kingdom.ShowBetrayalWarLog (info.warfare, enemyKingdom);
												} else {
													Debug.Log(kingdom.name + " provided support in " + info.warfare.name + " against its former alliance members");
													float transferPercentage = 0.1f;
													if (isKingdomHasWar) {
														transferPercentage = 0.05f;
													}
													transferPercentage /= (float)enemySideKingdoms.Count;
													for (int j = 0; j < enemySideKingdoms.Count; j++) {
														string logAmount = kingdom.ProvideWeaponsArmorsAidToKingdom (enemySideKingdoms [j], transferPercentage);
														kingdom.ShowBetrayalProvideLog (allianceOfSourceKingdom, logAmount, enemySideKingdoms [j]);
													}
												}
												break;
											} else {
												kingdom.ShowDoNothingLog (info.warfare);
											}
										}
									} else {
										kingdom.ShowDoNothingLog (info.warfare);
									}
								}
							}
						}
						if(hasLeftAlliance){
							break;
						}
					}
				}
			}
			if(!hasLeftAlliance && warsToJoin.Count > 0){
				for (int i = 0; i < warsToJoin.Count; i++) {
					List<Kingdom> allySideKingdoms = warsToJoin[i].warfare.GetListFromSide (warsToJoin[i].side);
					Kingdom allyKingdom = warsToJoin [i].warfare.GetListFromSide(warsToJoin[i].side)[0];
					if(warsToJoin[i].warfare.IsAdjacentToEnemyKingdoms(kingdom, warsToJoin[i].side)){
						Debug.Log(kingdom.name + " decided to join in " + warsToJoin[i].warfare.name + " in " + allyKingdom.name + "'s side");
						warsToJoin [i].warfare.JoinWar (warsToJoin [i].side, kingdom);
						kingdom.ShowJoinWarLog (allyKingdom, warsToJoin [i].warfare);
					}else{
						Debug.Log(kingdom.name + " decided to provide support to his allies in " + warsToJoin[i].warfare.name);
						float transferPercentage = 0.1f;
						if (isKingdomHasWar) {
							transferPercentage = 0.05f;
						}
						transferPercentage /= (float)allySideKingdoms.Count;
						for (int j = 0; j < allySideKingdoms.Count; j++) {
							string logAmount = kingdom.ProvideWeaponsArmorsAidToKingdom (allySideKingdoms[j], transferPercentage);
							kingdom.ShowTransferWeaponsArmorsLog (allySideKingdoms[j], logAmount);
						}

					}

				}
			}
		}

		if(!skipPhase3){
			Phase3 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
		}else{
			Debug.Log("==========SKIPPED PHASE 3 END SEEKS SUPERIORITY " + kingdom.name + " ==========");
		}
	}

	private static void Phase3(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool hasAllianceInWar){
		Debug.Log ("========== PHASE 3 ==========");      
		//if prestige can still accommodate more cities but nowhere to expand and currently not at war and none of my allies are at war
		if(skipPhase2){
			if (kingdom.alliancePool != null){
				for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
					Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
					if(kingdom.id != allyKingdom.id){
						if(allyKingdom.HasWar()){
							hasAllianceInWar = true;
							break;
						}
					}
				}
			}
		}
		if (kingdom.alliancePool == null || !hasAllianceInWar){
			if(!kingdom.HasWar()){
				HexTile hexTile = CityGenerator.Instance.GetExpandableTileForKingdom(kingdom);
				if(hexTile == null){
					//Can no longer expand
					Debug.Log(kingdom.name + " has no war currently, has no alliance or no alliance member is at war, and can no longer expand");
					Kingdom targetKingdom = null;
					bool hasOver100InvasionValue = false;
					bool hasOver50InvasionValue = false;
					float highestInvasionValue = 0f;
					int overPopulationReduction = 0;
					int overpopulation = kingdom.GetOverpopulationPercentage();

					if(overpopulation > 0) {
						if (overpopulation <= 10) {
							overPopulationReduction = 10;
						} else if (overpopulation > 10 && overpopulation <= 20) {
							overPopulationReduction = 20;
						} else if (overpopulation > 20 && overpopulation <= 40) {
							overPopulationReduction = 35;
						} else if (overpopulation > 40 && overpopulation <= 60) {
							overPopulationReduction = 50;
						} else if (overpopulation > 60) {
							overPopulationReduction = 65;
						}
					}

					foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
						if(relationship.isDiscovered && !relationship.AreAllies() && relationship.warfare == null){
							if(relationship.targetKingdomInvasionValue > highestInvasionValue){
								highestInvasionValue = relationship.targetKingdomInvasionValue;
								if (relationship.targetKingdomInvasionValue >= KingdomManager.Instance.GetReducedInvasionValueThreshHold (100f, overPopulationReduction)) {
									hasOver100InvasionValue = true;
									targetKingdom = relationship.targetKingdom;
								} 
								if(!hasOver100InvasionValue){
									if (relationship.targetKingdomInvasionValue >= KingdomManager.Instance.GetReducedInvasionValueThreshHold (50f, overPopulationReduction)
										&& relationship.targetKingdomInvasionValue < KingdomManager.Instance.GetReducedInvasionValueThreshHold (100f, overPopulationReduction)) {
										hasOver50InvasionValue = true;
										targetKingdom = relationship.targetKingdom;
									}
								}
							}
						}
					}
					if(targetKingdom != null){
						Debug.Log(targetKingdom.name + " has " + highestInvasionValue.ToString());
						if(hasOver100InvasionValue){
							int stabilityModifier = (int)((float)kingdom.stability / 10f);
							int chance = UnityEngine.Random.Range (0, 100);
							int value = (int)(kingdom.king.GetWarmongerWarPercentage100());
							value += (int)(kingdom.king.GetWarmongerWarPercentage100() * (float)stabilityModifier);
							if(chance < value){
								//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
								Debug.Log(kingdom.name + " decided to have war with " + targetKingdom.name);
								Warfare warfare = new Warfare (kingdom, targetKingdom);
							}
						}else{
							if(hasOver50InvasionValue){
								KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (targetKingdom);
								int stabilityModifier = (int)((float)kingdom.stability / 5f);
								int chance = UnityEngine.Random.Range (0, 100);
								int value = (int)(kingdom.king.GetWarmongerWarPercentage50() * (float)stabilityModifier);
								int threshold = KingdomManager.Instance.GetReducedInvasionValueThreshHold (50f, overPopulationReduction);
								int totalValue = ((int)kr.targetKingdomInvasionValue - threshold) * value;
								if(chance < totalValue){
									//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
									Debug.Log(kingdom.name + " decided to have war with " + targetKingdom.name);
									Warfare warfare = new Warfare (kingdom, targetKingdom);
								}else{
									if(!kingdom.isFortifying && !kingdom.isMilitarize){
										int newChance = UnityEngine.Random.Range (0, 2);
										if(newChance == 0){
											Debug.Log(kingdom.name + " just decided to militarize");
											kingdom.Militarize (true);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		Debug.Log("========== END SEEKS SUPERIORITY " + kingdom.name + " ==========");
	}
}
