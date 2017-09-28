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
		//break any alliances with anyone whose threat value is 100 or above and lose Base Prestige
		if(kingdom.alliancePool != null){
			for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
				if(kingdom.id != allyKingdom.id){
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(allyKingdom);
					if(kr.targetKingdomInvasionValue >= 100f){
						kingdom.LeaveAlliance ();
						kingdom.AdjustPrestige(-GridMap.Instance.numOfRegions);
						Debug.Log(kingdom.name + " broke alliance with " + allyKingdom.name +
							" because its invasion value is " + kr.targetKingdomInvasionValue.ToString() + "," + kingdom.name + 
							" lost " + GridMap.Instance.numOfRegions.ToString() + " prestige. Prestige is now " + kingdom.prestige.ToString());
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
		if (isUnderAttack && !isAttacking) {
			if (!kingdom.isFortifying) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Fortify (true);
				}
			}
		} else if (!isUnderAttack && isAttacking) {
			if (!kingdom.isMilitarize) {
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 75) {
					kingdom.Militarize (true);
				}
			}
		} else {
			//if there are kingdoms whose invasion value is 75 or above that is not part of my alliance
			if(!kingdom.isMilitarize){
				foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
					if (relationship.isDiscovered && relationship.targetKingdomInvasionValue >= 75f) {
						if (!relationship.AreAllies ()) {
							int chance = UnityEngine.Random.Range (0, 2);
							if (chance == 0) {
								kingdom.Militarize (true);
							}
							break;
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
			List<WarfareInfo> warsToJoin = new List<WarfareInfo> ();
//			Dictionary<Warfare, WAR_SIDE> warsToJoin = new Dictionary<Warfare, WAR_SIDE>();
			foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
				if(!relationship.isAtWar && !relationship.AreAllies() && relationship.isDiscovered){
					for (int i = 0; i < kingdom.alliancePool.kingdomsInvolved.Count; i++) {
						Kingdom allyKingdom = kingdom.alliancePool.kingdomsInvolved[i];
						if(kingdom.id != allyKingdom.id){
							KingdomRelationship kr = allyKingdom.GetRelationshipWithKingdom(relationship.targetKingdom);
							if(kr.isAtWar || kr.isPreparingForWar){
								hasAllianceInWar = true;
								WarfareInfo info = allyKingdom.GetWarfareInfo (kr.warfare.id);
								if(info.warfare != null && !warsToJoin.Contains(info)){
									KingdomRelationship krWithAlly = kingdom.GetRelationshipWithKingdom (allyKingdom);
									int totalChanceOfJoining = krWithAlly.totalLike * 2;
									int chance = UnityEngine.Random.Range (0, 100);
									if(chance < totalChanceOfJoining){
										//Join War
										warsToJoin.Add(info);
										Debug.Log(kingdom.name + " will join in " + allyKingdom.name + "'s war in side: " + info.side.ToString());
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
				for (int i = 0; i < warsToJoin.Count; i++) {
					warsToJoin [i].warfare.JoinWar (warsToJoin [i].side, kingdom);
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
			if(kingdom.cities.Count < kingdom.cityCap){
				if(!kingdom.HasWar()){
					Kingdom targetKingdom = null;
					float highestInvasionValue = kingdom.relationships.Values.Max(x => x.targetKingdomInvasionValue);
					if(highestInvasionValue >= 100f){
						int value = kingdom.kingdomTypeData.prepareForWarChance * (int)(kingdom.prestige / GridMap.Instance.numOfRegions);
						foreach (KingdomRelationship relationship in kingdom.relationships.Values) {
							if(relationship.isDiscovered && relationship.targetKingdomInvasionValue == highestInvasionValue && !relationship.AreAllies()){
								int chance = UnityEngine.Random.Range (0, 100);
								if(chance < value){
									targetKingdom = relationship.targetKingdom;
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
							if(relationship.targetKingdomInvasionValue >= 50f && relationship.targetKingdomInvasionValue < 100f){
								if(relationship.isDiscovered && !relationship.AreAllies()){
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
