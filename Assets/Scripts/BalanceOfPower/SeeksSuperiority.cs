using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SeeksSuperiority {

	public static void Initialize(Kingdom kingdom){
		bool skipPhase2 = false;
		bool skipPhase3 = false;
		bool skipPhase4 = false;
		bool hasAllianceInWar = false;
		Phase1 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
	}

	private static void Phase1(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool skipPhase4, bool hasAllianceInWar){
		Debug.Log("========== " + kingdom.name + " is seeking superiority " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ==========");
		Debug.Log ("========== PHASE 1 ==========");      
		//break any alliances with anyone whose threat value is 100 or above and lose 10 Stability
		if (kingdom.alliancePool != null) {
			for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved [i];
				if (kingdom.id != allyKingdom.id) {
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (allyKingdom);
					if (kr.totalLike <= -100) {
						kingdom.LeaveAlliance ();
						kingdom.AdjustStability (-10);
						Debug.Log (kingdom.name + " broke alliance with " + allyKingdom.name + " because total like is, " + kingdom.name +
							" lost 10 Stability. Stability is now " + kingdom.stability.ToString ());
						skipPhase4 = true;
						break;
					}
				}
			}
		}

		bool mustSeekAlliance = false;
		if(kingdom.alliancePool == null){
			if(kingdom.highestRelativeStrengthAdjacentKingdom != null){
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (kingdom.highestRelativeStrengthAdjacentKingdom);
				if(kr._relativeWeakness < 50){
					Kingdom kingdomToAlly = null;
					int leastLikedToEnemy = 0;
					foreach (KingdomRelationship krToAlly in kingdom.relationships.Values) {
						if(krToAlly.targetKingdom.id != kingdom.highestRelativeStrengthAdjacentKingdom.id){
							KingdomRelationship krFromAlly = krToAlly.targetKingdom.GetRelationshipWithKingdom (kingdom);
							KingdomRelationship krEnemy = krToAlly.targetKingdom.GetRelationshipWithKingdom (kingdom.highestRelativeStrengthAdjacentKingdom);
							if(krToAlly.totalLike > 0 && krFromAlly.totalLike > 0 && krEnemy.isAdjacent 
								&& krToAlly.targetKingdom.king.balanceType == PURPOSE.SUPERIORITY && KingdomManager.Instance.kingdomRankings[0].id != krToAlly.targetKingdom.id && !krToAlly.cantAlly){
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
						skipPhase4 = true;
					}
				}else{
					mustSeekAlliance = true;
				}
			}else{
				mustSeekAlliance = true;
			}

			if(mustSeekAlliance){
				foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
					if(relationship.isDiscovered && relationship.targetKingdomThreatLevel >= 100f && !relationship.AreAllies()){
						Debug.Log(kingdom.name + " is seeking alliance because it has no allies, there is a kingdom that has 100 or above threat and a less than 75 invasion value");
						kingdom.SeekAlliance ();
						if(kingdom.alliancePool != null){
							skipPhase2 = true;
						}
						skipPhase4 = true;
						break;
					}
				}

			}

		}
		if(!skipPhase2){
			Phase2 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
		}else{
			if(!skipPhase3){
				Phase3 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
			}else{
				if(!skipPhase4){
					Phase4 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
				}else{
					Debug.Log("==========SKIPPED PHASE 4 END SEEKS SUPERIORITY " + kingdom.name + " ==========");
				}
			}
		}

	}

	private static void Phase2(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool skipPhase4, bool hasAllianceInWar){
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
							if (!kingdom.warfareInfo.ContainsKey (info.warfare.id) && !warsToJoin.Contains (info) && !kingdom.checkedWarfareID.Contains(info.warfare.id)) {
								skipPhase4 = true;
								kingdom.checkedWarfareID.Add (info.warfare.id);
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
												kingdom.LeaveAlliance ();
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
				skipPhase4 = true;
			}
		}

		if(!skipPhase3){
			Phase3 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
		}else{
			if(!skipPhase4){
				Phase4 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
			}else{
				Debug.Log("==========SKIPPED PHASE 4 END SEEKS SUPERIORITY " + kingdom.name + " ==========");
			}
		}
	}

	private static void Phase3(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool skipPhase4, bool hasAllianceInWar){
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
					bool hasOver100InvasionValue = false;
					bool hasOver50InvasionValue = false;
//					int overPopulationReduction = 0;
//					int overpopulation = kingdom.GetOverpopulationPercentage();

//					if(overpopulation > 0) {
//						if (overpopulation <= 10) {
//							overPopulationReduction = 10;
//						} else if (overpopulation > 10 && overpopulation <= 20) {
//							overPopulationReduction = 20;
//						} else if (overpopulation > 20 && overpopulation <= 40) {
//							overPopulationReduction = 35;
//						} else if (overpopulation > 40 && overpopulation <= 60) {
//							overPopulationReduction = 50;
//						} else if (overpopulation > 60) {
//							overPopulationReduction = 65;
//						}
//					}

					Kingdom targetKingdom = null;
					int leastLike = 0;
					foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
						if(relationship.totalLike < 0 && relationship.isAdjacent && relationship.isDiscovered && !relationship.AreAllies() && relationship.warfare == null && !relationship.isRecentWar){
							if (relationship._relativeWeakness >= 100) {
								if(targetKingdom == null || !hasOver100InvasionValue){
									targetKingdom = relationship.targetKingdom;
									leastLike = relationship.totalLike;
								}else{
									if(relationship.totalLike < leastLike){
										targetKingdom = relationship.targetKingdom;
										leastLike = relationship.totalLike;
									}
								}
								hasOver100InvasionValue = true;
							} 
							if(!hasOver100InvasionValue){
								if (relationship._relativeWeakness > 0 && relationship._relativeWeakness < 100) {
									if(targetKingdom == null){
										targetKingdom = relationship.targetKingdom;
										leastLike = relationship.totalLike;
									}else{
										if(relationship.totalLike < leastLike){
											targetKingdom = relationship.targetKingdom;
											leastLike = relationship.totalLike;
										}
									}
									hasOver50InvasionValue = true;
								}
							}
						}
					}
					if(targetKingdom != null){
						Debug.Log(kingdom.name + " has " + leastLike.ToString() + " total like towards " + targetKingdom.name);
						if(hasOver100InvasionValue){
							int stabilityModifier = (int)((float)kingdom.stability / 10f);
							int chance = UnityEngine.Random.Range (0, 100);
							int value = (int)(kingdom.king.GetWarmongerWarPercentage100());
							value += (int)(kingdom.king.GetWarmongerWarPercentage100() * (float)stabilityModifier);
							if(chance < value){
								//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
								Debug.Log(kingdom.name + " decided to have war with " + targetKingdom.name);
								Warfare warfare = new Warfare (kingdom, targetKingdom);
								kingdom.checkedWarfareID.Add (warfare.id);
								targetKingdom.checkedWarfareID.Add (warfare.id);
								skipPhase4 = true;
							}
						}else{
							if(hasOver50InvasionValue){
								KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (targetKingdom);
								int stabilityModifier = (int)((float)kingdom.stability / 10f);
								int chance = UnityEngine.Random.Range (0, 100);
								int value = (int)(kingdom.king.GetWarmongerWarPercentage50() * (float)stabilityModifier);
								int threshold = 50;
								int totalValue = ((int)kr.targetKingdomInvasionValue - threshold) * value;
								if(chance < totalValue){
									//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
									Debug.Log(kingdom.name + " decided to have war with " + targetKingdom.name);
									Warfare warfare = new Warfare (kingdom, targetKingdom);
									kingdom.checkedWarfareID.Add (warfare.id);
									targetKingdom.checkedWarfareID.Add (warfare.id);
									skipPhase4 = true;
								}
							}
						}
					}
				}
			}
		}
		if(!skipPhase4){
			Phase4 (kingdom, skipPhase2, skipPhase3, skipPhase4, hasAllianceInWar);
		}else{
			Debug.Log("==========SKIPPED PHASE 4 END SEEKS SUPERIORITY " + kingdom.name + " ==========");
		}
	}

	private static void Phase4(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool skipPhase4, bool hasAllianceInWar){
		//Subterfuge
		Debug.Log ("========== PHASE 4 ==========");
		kingdom.Subterfuge ();
		Debug.Log("========== END SEEKS SUPERIORITY " + kingdom.name + " ==========");

	}
}
