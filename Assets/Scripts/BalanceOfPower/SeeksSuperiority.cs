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
		bool mustSeekWar = false;
		bool mustMilitarize = false;
		if (isUnderAttack && !isAttacking) {
			if (!kingdom.isFortifying) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Fortify (true, isUnderAttack);
				}
			}
		} else if (!isUnderAttack && isAttacking) {
			if (!kingdom.isMilitarize) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Militarize (true, isAttacking);
				}
			}
		} else {
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(relationship.isDiscovered){
					if(relationship.targetKingdomThreatLevel >= 100f){
						if(relationship.targetKingdomInvasionValue < 75f){
							if(!relationship.AreAllies()){
								mustSeekAlliance = true;
								break;
							}
						}
					}else if(relationship.targetKingdomThreatLevel >= 50f){
						if(relationship.targetKingdomInvasionValue >= 25f){
							if (!relationship.AreAllies ()) {
								mustSeekWar = true;
							}
						}
					}
					if(!kingdom.isMilitarize){
						if(relationship.targetKingdomInvasionValue >= 75f){
							if (!relationship.AreAllies ()) {
								mustMilitarize = true;
							}
						}
					}
				}
			}

			if(mustSeekAlliance){
				//if i am not part of any alliance, create or join an alliance if possible
				if(kingdom.alliancePool == null){
					kingdom.SeekAlliance ();
					if(kingdom.alliancePool != null){
						skipPhase2 = true;
					}
				}
				if (!kingdom.isFortifying) {
					int chance = UnityEngine.Random.Range (0, 2);
					if (chance == 0) {
						kingdom.Fortify (true);
						Debug.Log (kingdom.name + " starts fortifying");
					}
				}
			}else{
				if(mustSeekWar){
					Kingdom targetKingdom = null;
					float highestInvasionValue = kingdom.relationships.Values.Max (x => x.targetKingdomInvasionValue);
					foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
						if(relationship.targetKingdomInvasionValue == highestInvasionValue && !relationship.AreAllies()){
							targetKingdom = relationship.targetKingdom;
							skipPhase3 = true;
							break;
						}
					}
					if(targetKingdom != null){
						Warfare warfare = new Warfare (kingdom, targetKingdom);
						Debug.Log(kingdom.name + " prepares for war against " + targetKingdom.name);
					}
				}else{
					//if there are kingdoms whose invasion value is 75 or above that is not part of my alliance
					if(mustMilitarize){
						if(!kingdom.isMilitarize){
							int chance = UnityEngine.Random.Range (0, 2);
							if (chance == 0) {
								kingdom.Militarize (true);
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
									kingdom.AdjustStability (-10);
									if (totalLikeToEnemies > 0) {
										int leavingValue = totalLikeToEnemies * 2;
										int chanceOfLeaving = UnityEngine.Random.Range (0, 100);

										if (totalLikeToAllies >= 0) {
											//Refuse to participate in war and leave alliance
											if (chanceOfLeaving < leavingValue) {
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
													info.warfare.JoinWar (enemySide, kingdom);
													kingdom.ShowBetrayalWarLog (info.warfare, enemyKingdom);
												} else {
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
					}
				}
			}
			if(!hasLeftAlliance && warsToJoin.Count > 0){
				for (int i = 0; i < warsToJoin.Count; i++) {
					List<Kingdom> allySideKingdoms = warsToJoin[i].warfare.GetListFromSide (warsToJoin[i].side);
					Kingdom allyKingdom = warsToJoin [i].warfare.GetListFromSide(warsToJoin[i].side)[0];
					if(warsToJoin[i].warfare.IsAdjacentToEnemyKingdoms(kingdom, warsToJoin[i].side)){
						warsToJoin [i].warfare.JoinWar (warsToJoin [i].side, kingdom);
						kingdom.ShowJoinWarLog (allyKingdom, warsToJoin [i].warfare);
						Debug.Log(kingdom.name + " will join in " + allyKingdom.name + "'s war in side: " + warsToJoin [i].side.ToString());
					}else{
						float transferPercentage = 0.1f;
						if (isKingdomHasWar) {
							transferPercentage = 0.05f;
						}
						transferPercentage /= (float)allySideKingdoms.Count;
						for (int j = 0; j < allySideKingdoms.Count; j++) {
							string logAmount = kingdom.ProvideWeaponsArmorsAidToKingdom (allySideKingdoms[i], transferPercentage);
							kingdom.ShowTransferWeaponsArmorsLog (allyKingdom, logAmount);
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
					Kingdom targetKingdom = null;
					bool hasOver100InvasionValue = false;
					bool hasOver50InvasionValue = false;
					float highestInvasionValue = 0f;
					int stabilityModifier = (int)((float)kingdom.stability / 20f);
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
						if(relationship.isDiscovered && !relationship.AreAllies()){
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
						if(hasOver100InvasionValue){
							int chance = UnityEngine.Random.Range (0, 100);
							int value = kingdom.kingdomTypeData.prepareForWarChance * stabilityModifier;
							if(chance < value){
								//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
								Warfare warfare = new Warfare (kingdom, targetKingdom);
								Debug.Log(kingdom.name + " prepares for war against " + targetKingdom.name);
							}
						}else{
							if(hasOver50InvasionValue){
								KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (targetKingdom);
								int chance = UnityEngine.Random.Range (0, 100);
								int value = 1 * stabilityModifier;
								int threshold = KingdomManager.Instance.GetReducedInvasionValueThreshHold (50f, overPopulationReduction);
								int totalValue = ((int)kr.targetKingdomInvasionValue - threshold) * value;
								if(chance < totalValue){
									//if there is anyone whose Invasion Value is 1 or above, prepare for war against the one with the highest Invasion Value
									Warfare warfare = new Warfare (kingdom, targetKingdom);
									Debug.Log(kingdom.name + " prepares for war against " + targetKingdom.name);
								}else{
									if(!kingdom.isFortifying && !kingdom.isMilitarize){
										int newChance = UnityEngine.Random.Range (0, 2);
										if(newChance == 0){
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
