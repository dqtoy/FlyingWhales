using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlliancePool {
	private int _id;
	private bool _isDissolved;
	private List<Kingdom> kingdomsInvolved;

	#region getters/setters
	public int id{
		get{return this._id;}
	}
	public bool isDissolved{
		get{return this._isDissolved;}
	}
	#endregion

	public AlliancePool(){
		SetID ();
	}

	private void SetID(){
		this._id = Utilities.lastAlliancePoolID + 1;
		Utilities.lastAlliancePoolID = this._id;
	}
	internal void AttemptToAddKingdomInAlliance(Kingdom kingdom){
		bool canBeAccepted = true;
		for (int i = 0; i < this.kingdomsInvolved.Count; i++) {
			KingdomRelationship relationshipTo = kingdom.GetRelationshipWithKingdom (this.kingdomsInvolved [i]);
			KingdomRelationship relationshipFrom = this.kingdomsInvolved [i].GetRelationshipWithKingdom (kingdom);
			if(relationshipTo.totalLike <= 0 || relationshipFrom.totalLike <= 0){
				canBeAccepted = false;
				break;
			}
		}
		if(canBeAccepted){
			AddKingdomInAlliance (kingdom);
		}
	}
	internal void AddKingdomInAlliance(Kingdom kingdom){
		this.kingdomsInvolved.Add (kingdom);
		kingdom.SetAlliancePool (this);
	}
	internal void RemoveKingdomInAlliance(Kingdom kingdom){
		this.kingdomsInvolved.Remove (kingdom);
		kingdom.SetAlliancePool (null);
		CheckKingdomsInvolved ();
	}
	private void CheckKingdomsInvolved(){
		if(this.kingdomsInvolved.Count == 1){
			//Dissolve Alliance
			DissolveAlliance();
		}
	}
	private void DissolveAlliance(){
		if(!this._isDissolved){
			this._isDissolved = true;
			int count = this.kingdomsInvolved.Count;
			for (int i = 0; i < count; i++) {
				this.kingdomsInvolved [i].SetAlliancePool (null);
				this.kingdomsInvolved.RemoveAt (0);
			}
		}
	}
}
