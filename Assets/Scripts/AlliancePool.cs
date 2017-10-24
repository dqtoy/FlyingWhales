using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlliancePool {
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
			if(relationshipTo.totalLike <= 0 || relationshipFrom.totalLike <= 0){
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
				if(!kr.isDiscovered){
					KingdomManager.Instance.DiscoverKingdom (kingdom, this._kingdomsInvolved [i]);
				}
			}
		}
	}
	internal bool HasAdjacentAllianceMember(Kingdom kingdom){
		for (int i = 0; i < this._kingdomsInvolved.Count; i++) {
			if(kingdom.id != this._kingdomsInvolved[i].id){
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._kingdomsInvolved [i]);
				if(kr.isAdjacent){
					return true;
				}
			}
		}
		return false;
	}
}
