using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlliancePool {
	private enum REACTIONS{
		JOIN_WAR,
		REMAIN_NEUTRAL,
		LEAVE_ALLIANCE,
		BETRAY,
	}

	private int _id;
	private string _name;
	private bool _isDissolved;
	private List<Kingdom> _kingdomsInvolved;

	#region getters/setters
	public int id{
		get{return this._id;}
	}
	public string name{
		get{return this._name;}
	}
	public bool isDissolved{
		get{return this._isDissolved;}
	}
	public List<Kingdom> kingdomsInvolved{
		get{return this._kingdomsInvolved;}
	}

	#endregion

	public AlliancePool(){
		SetID ();
		this._name = RandomNameGenerator.Instance.GetAllianceName ();
		this._kingdomsInvolved = new List<Kingdom>();
		this._isDissolved = false;
	}

	private void SetID(){
		this._id = Utilities.lastAlliancePoolID + 1;
		Utilities.lastAlliancePoolID = this._id;
	}
	internal bool AttemptToJoinAlliance(Kingdom kingdom, Kingdom kingdomInAlliance){
		bool canBeAccepted = true;
		for (int i = 0; i < this._kingdomsInvolved.Count; i++) {
			KingdomRelationship relationshipTo = kingdom.GetRelationshipWithKingdom (this._kingdomsInvolved [i]);
			KingdomRelationship relationshipFrom = this._kingdomsInvolved [i].GetRelationshipWithKingdom (kingdom);
			if(relationshipTo.totalLike <= 0 || relationshipFrom.totalLike <= 0 || relationshipTo.sharedRelationship.isAtWar || relationshipFrom.sharedRelationship.isAtWar){
				canBeAccepted = false;
				break;
			}
		}
		if(canBeAccepted){
			AddKingdomInAlliance (kingdom);
			DiscoverKingdomsInAlliance (kingdom);
			Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "join_alliance");
			newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (null, this.name, LOG_IDENTIFIER.ALLIANCE_NAME);
			newLog.AddAllInvolvedObjects (this._kingdomsInvolved.ToArray ());
			UIManager.Instance.ShowNotification (newLog);
		}
		return canBeAccepted;
	}
	internal void AddKingdomInAlliance(Kingdom kingdom){
		this._kingdomsInvolved.Add (kingdom);
		kingdom.SetAlliancePool (this);
	}
	internal void RemoveKingdomInAlliance(Kingdom kingdom){
		this._kingdomsInvolved.Remove (kingdom);
		kingdom.SetAlliancePool (null);
		CheckKingdomsInvolved ();
	}
	private void CheckKingdomsInvolved(){
		if(this._kingdomsInvolved.Count <= 1){
			//Dissolve Alliance
			DissolveAlliance();
		}
	}
	private void DissolveAlliance(){
		if(!this._isDissolved){
			this._isDissolved = true;
			int count = this._kingdomsInvolved.Count;
			for (int i = 0; i < count; i++) {
				this._kingdomsInvolved [i].SetAlliancePool (null);
				this._kingdomsInvolved.RemoveAt (0);
			}
			KingdomManager.Instance.RemoveAlliancePool (this);
		}
	}
	private void DiscoverKingdomsInAlliance(Kingdom kingdom){
		for (int i = 0; i < this._kingdomsInvolved.Count; i++) {
			if(kingdom.id != this._kingdomsInvolved[i].id){
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._kingdomsInvolved [i]);
				if(!kr.sharedRelationship.isDiscovered){
					KingdomManager.Instance.DiscoverKingdom (kingdom, this._kingdomsInvolved [i]);
				}
			}
		}
	}
	internal bool HasAdjacentAllianceMember(Kingdom kingdom){
		for (int i = 0; i < this._kingdomsInvolved.Count; i++) {
			if(kingdom.id != this._kingdomsInvolved[i].id){
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._kingdomsInvolved [i]);
				if(kr.sharedRelationship.isAdjacent){
					return true;
				}
			}
		}
		return false;
	}

	internal void AlliesReactionToWar(Kingdom sourceKingdom, Kingdom enemyKingdom, Warfare warfare){
		Dictionary<REACTIONS, int> reactionsWeight = new Dictionary<REACTIONS, int> ();
		for (int i = 0; i < this._kingdomsInvolved.Count; i++) {
			Kingdom allyKingdom = this._kingdomsInvolved [i];
			if(sourceKingdom.id != allyKingdom.id){
				reactionsWeight.Clear ();
				reactionsWeight.Add (REACTIONS.JOIN_WAR, GetReactionWeight (REACTIONS.JOIN_WAR, sourceKingdom, allyKingdom, enemyKingdom));
				reactionsWeight.Add (REACTIONS.REMAIN_NEUTRAL, GetReactionWeight (REACTIONS.REMAIN_NEUTRAL, sourceKingdom, allyKingdom, enemyKingdom));
				reactionsWeight.Add (REACTIONS.LEAVE_ALLIANCE, GetReactionWeight (REACTIONS.LEAVE_ALLIANCE, sourceKingdom, allyKingdom, enemyKingdom));
				reactionsWeight.Add (REACTIONS.BETRAY, GetReactionWeight (REACTIONS.BETRAY, sourceKingdom, allyKingdom, enemyKingdom));

				REACTIONS pickedReaction = Utilities.PickRandomElementWithWeights<REACTIONS> (reactionsWeight);
				AllyReact (pickedReaction, sourceKingdom, allyKingdom, warfare);
				if(pickedReaction == REACTIONS.LEAVE_ALLIANCE || pickedReaction == REACTIONS.BETRAY){
					i--;
				}
			}
		}
	}

	private int GetReactionWeight(REACTIONS reaction, Kingdom sourceKingdom, Kingdom allyKingdom, Kingdom enemyKingdom){
		int totalWeight = 0;
		KingdomRelationship krAllyToSource = allyKingdom.GetRelationshipWithKingdom (sourceKingdom);
		KingdomRelationship krAllyToEnemy = allyKingdom.GetRelationshipWithKingdom (enemyKingdom);
		if(reaction == REACTIONS.JOIN_WAR){
			if(krAllyToSource.totalLike > 0){
				totalWeight += (5 * krAllyToSource.totalLike);
			}
			if(krAllyToEnemy.relativeStrength < 0){
				totalWeight -= (2 * krAllyToEnemy.relativeStrength);
			}
			if (krAllyToEnemy.totalLike < 0) {
				totalWeight -= (5 * krAllyToEnemy.totalLike);
			}
		}else if(reaction == REACTIONS.REMAIN_NEUTRAL){
			if(krAllyToEnemy.relativeStrength > 0){
				totalWeight += (2 * krAllyToEnemy.relativeStrength);
			}
			if(krAllyToSource.totalLike < 0){
				totalWeight -= (2 * krAllyToSource.totalLike);
			}
			if(krAllyToEnemy.AreTradePartners()){
				totalWeight += 30;
			}
			if (krAllyToEnemy.totalLike > 0) {
				totalWeight += (2 * krAllyToEnemy.totalLike);
			}
		}else if(reaction == REACTIONS.LEAVE_ALLIANCE){
			if(krAllyToSource.totalLike < 0){
				totalWeight -= krAllyToSource.totalLike;
			}
		}else if(reaction == REACTIONS.BETRAY){
			if(allyKingdom.king.HasTrait(TRAIT.DECEITFUL)){
				if(krAllyToSource.relativeStrength < 0){
					totalWeight -= (2 * krAllyToSource.relativeStrength);				
				}
				if(krAllyToSource.totalLike < 0){
					totalWeight -= krAllyToSource.totalLike;
				}
			}
		}
		if(totalWeight < 0){
			totalWeight = 0;
		}
		return totalWeight;
	}

	private void AllyReact(REACTIONS reaction, Kingdom sourceKingdom, Kingdom allyKingdom, Warfare warfare){
		if(reaction == REACTIONS.JOIN_WAR){
			warfare.JoinWar (warfare.GetSideOfKingdom (sourceKingdom), allyKingdom);
			allyKingdom.ShowJoinWarLog (sourceKingdom, warfare);
		}else if(reaction == REACTIONS.REMAIN_NEUTRAL){
			allyKingdom.ShowDoNothingLog (warfare);
		}else if(reaction == REACTIONS.LEAVE_ALLIANCE){
			allyKingdom.ShowRefuseAndLeaveAllianceLog (this, warfare);
			allyKingdom.LeaveAlliance (true);
		}else if(reaction == REACTIONS.BETRAY){
			allyKingdom.ShowBetrayalLog (this, sourceKingdom);
			allyKingdom.LeaveAlliance (true);
			Warfare betrayWar = new Warfare (allyKingdom, sourceKingdom);
		}
	}
}
