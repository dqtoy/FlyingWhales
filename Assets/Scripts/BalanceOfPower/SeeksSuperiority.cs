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
			List<JoinWarfareInfo> warsToJoin = new List<JoinWarfareInfo> ();
			//			Dictionary<Warfare, WAR_SIDE> warsToJoin = new Dictionary<Warfare, WAR_SIDE>();
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(!relationship.isAtWar && !relationship.AreAllies() && relationship.isDiscovered){
					for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
						Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
						if(kingdom.id != allyKingdom.id){
							KingdomRelationship kr = allyKingdom.GetRelationshipWithKingdom(relationship.targetKingdom);
							if(kr.isAtWar){
								hasAllianceInWar = true;
								WarfareInfo info = allyKingdom.GetWarfareInfo (kr.warfare.id);
								JoinWarfareInfo joinInfo = new JoinWarfareInfo (info, allyKingdom, relationship.isAdjacent);
								if(info.warfare != null && !warsToJoin.Contains(joinInfo)){
									KingdomRelationship krWithAlly = kingdom.GetRelationshipWithKingdom (allyKingdom);
									int totalChanceOfJoining = krWithAlly.totalLike * 2;
									int chanceReduction = relationship.totalLike;
									if(chanceReduction >= 0){
										totalChanceOfJoining -= chanceReduction;
									}
									int chance = UnityEngine.Random.Range (0, 100);
									if(chance < totalChanceOfJoining){
										//Join War
										warsToJoin.Add(joinInfo);
									} else{
										//Don't join war, lose 10 Stability
										kingdom.AdjustStability (-10);
										if(relationship.totalLike > 0){
											int leavingValue = relationship.totalLike * 2;
											int chanceOfLeaving = UnityEngine.Random.Range (0, 100);

											if(krWithAlly.totalLike >= 0){
												//Refuse to participate in war and leave alliance
												if(chanceOfLeaving < leavingValue){
													kingdom.ShowRefuseAndLeaveAllianceLog (kingdom.alliancePool, info.warfare);
													kingdom.LeaveAlliance(true);
													hasLeftAlliance = true;
													break;
												}else{
													kingdom.ShowDoNothingLog (info.warfare);
												}
											}else{
												//Leave alliance and join enemy's side
												if(chanceOfLeaving < leavingValue){
													kingdom.AdjustStability (-10);
													WarfareInfo warfareInfo = relationship.targetKingdom.GetWarfareInfo (kr.warfare.id);
													AlliancePool allianceOfSourceKingdom = kingdom.alliancePool;
													kingdom.LeaveAlliance(true);
													if(relationship.targetKingdom.alliancePool != null){
														relationship.targetKingdom.alliancePool.AttemptToJoinAlliance (kingdom, relationship.targetKingdom);
													}else{
														KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms (kingdom, relationship.targetKingdom);
													}

													if(krWithAlly.isAdjacent){
														warfareInfo.warfare.JoinWar (warfareInfo.side, kingdom);
														kingdom.ShowBetrayalWarLog (warfareInfo.warfare, relationship.targetKingdom);
													}else{
														string logAmount = kingdom.ProvideWeaponsArmorsAidToKingdom (relationship.targetKingdom);
														kingdom.ShowBetrayalProvideLog (allianceOfSourceKingdom, logAmount, relationship.targetKingdom);
													}
													break;
												}else{
													kingdom.ShowDoNothingLog (info.warfare);
												}
											}
										}
									}
								}
							}
						}
					}
					if(hasLeftAlliance){
						break;
					}
				}
			}
			if(!hasLeftAlliance && warsToJoin.Count > 0){
				for (int i = 0; i < warsToJoin.Count; i++) {
					Kingdom allyKingdom = warsToJoin [i].allyKingdom;
					if(warsToJoin [i].isAdjacentToEnemy){
						warsToJoin [i].info.warfare.JoinWar (warsToJoin [i].info.side, kingdom);
						kingdom.ShowJoinWarLog (allyKingdom, warsToJoin [i].info.warfare);
						Debug.Log(kingdom.name + " will join in " + allyKingdom.name + "'s war in side: " + warsToJoin [i].info.side.ToString());
					}else{
						string logAmount = kingdom.ProvideWeaponsArmorsAidToKingdom (allyKingdom);
						kingdom.ShowTransferWeaponsArmorsLog (allyKingdom, logAmount);
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
					float highestInvasionValue = kingdom.relationships.Values.Max(x => x.targetKingdomInvasionValue);
					float invasionValueThreshold = 100f;
					int overpopulation = kingdom.GetOverpopulationPercentage();
					if(overpopulation > 0) {
						if (overpopulation <= 10) {
							invasionValueThreshold -= 10;
						} else if (overpopulation > 10 && overpopulation <= 20) {
							invasionValueThreshold -= 20;
						} else if (overpopulation > 20 && overpopulation <= 40) {
							invasionValueThreshold -= 35;
						} else if (overpopulation > 40 && overpopulation <= 60) {
							invasionValueThreshold -= 50;
						} else if (overpopulation > 60) {
							invasionValueThreshold -= 65;
						}
					}
					if(highestInvasionValue >= invasionValueThreshold) {
						int value = kingdom.kingdomTypeData.prepareForWarChance * kingdom.cityCap;
						foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
							if(relationship.isDiscovered && relationship.targetKingdomInvasionValue == highestInvasionValue){
								if(!relationship.AreAllies()){
									int chance = UnityEngine.Random.Range (0, 100);
									if(chance < value){
										targetKingdom = relationship.targetKingdom;
									}
								}
							}
						}
						if(targetKingdom != null){
							//if there is anyone whose Invasion Value is 50 or above, prepare for war against the one with the highest Invasion Value
							Warfare warfare = new Warfare (kingdom, targetKingdom);
							Debug.Log(kingdom.name + " prepares for war against " + targetKingdom.name);
						}
					}else{
						foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
							if(relationship.isDiscovered && !relationship.AreAllies()){
								if(relationship.targetKingdomInvasionValue >= 50f && relationship.targetKingdomInvasionValue < 100f){
									if(!kingdom.isFortifying && !kingdom.isMilitarize){
										int chance = UnityEngine.Random.Range (0, 2);
										if(chance == 0){
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
