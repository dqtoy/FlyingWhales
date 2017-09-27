using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SeeksBalance {

//	private static bool skipPhase2;
//	private static bool skipPhase3;
//	private static bool hasAllianceInWar;
	public static void Initialize(Kingdom kingdom){
		bool skipPhase2 = false;
		bool skipPhase3 = false;
		bool hasAllianceInWar = false;
		Phase1 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
	}

	private static void Phase1(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool hasAllianceInWar){
		Debug.Log("========== " + kingdom.name + " is seeking balance " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ==========");
		//break any alliances with anyone whose threat value is 100 or above and lose Base Prestige
		if(kingdom.alliancePool != null){
			for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
				if(kingdom.id != allyKingdom.id){
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(allyKingdom);
					if(kr.targetKingdomThreatLevel >= 100f){
						kingdom.LeaveAlliance ();
						kingdom.AdjustPrestige(-GridMap.Instance.numOfRegions);
						Debug.Log(kingdom.name + " broke alliance with " + allyKingdom.name +
							" because it's threat level is " + kr.targetKingdomThreatLevel.ToString() + "," + kingdom.name + 
							" lost 50 prestige. Prestige is now " + kingdom.prestige.ToString());
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
		if(isUnderAttack && !isAttacking){
			if (!kingdom.isFortifying) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Fortify (true);
				}
			}
		}else if(!isUnderAttack && isAttacking){
			if (!kingdom.isMilitarize) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Militarize (true);
				}
			}
		}else{
			//if there are kingdoms whose threat value is 50 or above that is not part of my alliance
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(relationship.isDiscovered && relationship.targetKingdomThreatLevel < 100f){
					if(relationship.targetKingdomThreatLevel >= 75f){
						if(relationship.targetKingdomInvasionValue >= 75f){
							if(!relationship.AreAllies()){
								mustSeekWar = true;
							}
						}
					}else if(relationship.targetKingdomThreatLevel >= 50f){
						if(relationship.targetKingdomInvasionValue < 75f){
							if (!relationship.AreAllies ()) {
								mustSeekAlliance = true;
								break;
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
				}
			}
		}

		if(!skipPhase2){
			Phase2 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
		}else{
			if(!skipPhase3){
				Phase3 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
			}else{
				Debug.Log("==========SKIPPED PHASE 3 END SEEKS BALANCE " + kingdom.name + " ==========");
			}
		}

	}

	private static void Phase2(Kingdom kingdom, bool skipPhase2, bool skipPhase3, bool hasAllianceInWar){
		if(kingdom.alliancePool != null){
			bool hasLeftAlliance = false;
			Dictionary<Warfare, WAR_SIDE> warsToJoin = new Dictionary<Warfare, WAR_SIDE>();
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(!relationship.isAtWar && !relationship.AreAllies() && relationship.isDiscovered){
					for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
						Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
						if(kingdom.id != allyKingdom.id){
							KingdomRelationship kr = allyKingdom.GetRelationshipWithKingdom(relationship.targetKingdom);
							if(kr.isAtWar || kr.isPreparingForWar){
								hasAllianceInWar = true;
								if(!warsToJoin.ContainsKey(kr.warfare)){
									KingdomRelationship krWithAlly = kingdom.GetRelationshipWithKingdom (allyKingdom);
									int totalChanceOfJoining = krWithAlly.totalLike * 2;
									int chance = UnityEngine.Random.Range (0, 100);
									if(chance < totalChanceOfJoining){
										//Join War
										warsToJoin.Add(kr.warfare, kr.warfare.kingdomSides[allyKingdom]);
										Debug.Log(kingdom.name + " will join in " + allyKingdom.name + "'s war");
									} else{
										//Don't join war, leave alliance, lose 100 prestige
										kingdom.LeaveAlliance();
										int prestigeLost = (int)((float)GridMap.Instance.numOfRegions * 1.5f);
										kingdom.AdjustPrestige (-prestigeLost);
										hasLeftAlliance = true;
										Debug.Log(kingdom.name + " does not join in " + allyKingdom.name + "'s war, leaves the alliance and loses " + prestigeLost.ToString() + " prestige. Prestige is now " + kingdom.prestige.ToString());
										break;
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
				foreach (Warfare warfare in warsToJoin.Keys) {
					warfare.JoinWar(warsToJoin[warfare], kingdom);
				}
			}
		}

		if(!skipPhase3){
			Phase3 (kingdom, skipPhase2, skipPhase3, hasAllianceInWar);
		}else{
			Debug.Log("==========SKIPPED PHASE 3 END SEEKS BALANCE " + kingdom.name + " ==========");
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
			if(kingdom.cities.Count < kingdom.cityCap){
				if(!kingdom.HasWar()){
					HexTile hexTile = CityGenerator.Instance.GetExpandableTileForKingdom(kingdom);
					if(hexTile == null){
						//Can no longer expand
						Kingdom targetKingdom = null;
						float highestInvasionValue = kingdom.relationships.Values.Max(x => x.targetKingdomInvasionValue);
						if(highestInvasionValue >= 100f){
							int value = kingdom.kingdomTypeData.prepareForWarChance * (int)(kingdom.prestige / GridMap.Instance.numOfRegions);
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
		}

		Debug.Log("========== END SEEKS BALANCE " + kingdom.name + " ==========");
	}
}
