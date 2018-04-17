using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharedKingdomRelationship {

	private int _id;
	private bool _isAtWar;
	private bool _isAdjacent;
	private bool _isDiscovered;
	private bool _isRecentWar;

	private Warfare _warfare;
	private Battle _battle;

	private List<InternationalIncident> _internationalIncidents;

	private KingdomRelationship _kr1;
	private KingdomRelationship _kr2;

	internal bool cantAlly;

	#region getters/setters
	public int id {
		get { return this._id; }
	}
	public bool isAtWar {
		get { return _isAtWar; }
	}
	public bool isAdjacent {
		get { return this._isAdjacent; }
	}
	public bool isDiscovered {
		get { return this._isDiscovered; }
	}
	public Warfare warfare{
		get { return this._warfare; }
	}
	public Battle battle{
		get { return this._battle; }
	}
	public bool isRecentWar {
		get { return this._isRecentWar; }
	}
	public List<InternationalIncident> internationalIncidents {
		get { return this._internationalIncidents; }
	}
	public KingdomRelationship kr1 {
		get { return this._kr1; }
	}
	public KingdomRelationship kr2 {
		get { return this._kr2; }
	}
	#endregion

	public SharedKingdomRelationship(KingdomRelationship kr1, KingdomRelationship kr2){
		Utilities.SetID<SharedKingdomRelationship> (this);
		this._kr1 = kr1;
		this._kr2 = kr2;
		this._isAdjacent = false;
		this._isRecentWar = false;
		this._isAtWar = false;
		this._isDiscovered = false;
		this._warfare = null;
		this._battle = null;
		this._internationalIncidents = new List<InternationalIncident> ();
	}

	internal void SetWarStatus(bool warStatus, Warfare warfare) {
		if(this._isAtWar != warStatus){
			this._isAtWar = warStatus;
			SetWarfare(warfare);
			if(warStatus){
				ResolveAllIntlIncidents ();
			}
		}
	}
	internal void SetRecentWar(bool state) {
		if(this._isRecentWar != state){
			this._isRecentWar = state;
		}
	}
	internal void SetDiscovery(bool state) {
		if(this._isDiscovered != state){
			this._isDiscovered = state;
		}
	}
	internal void SetAdjacency(bool state){
		if(this._isAdjacent != state){
			this._isAdjacent = state;
		}
	}
	internal void SetCantAlly(bool state){
		this.cantAlly = state;
	}
	internal void SetWarfare(Warfare warfare){
		this._warfare = warfare;
	}
	internal void SetBattle(Battle battle){
		this._battle = battle;
	}

	internal void AddInternationalIncident(InternationalIncident intlIncident){
		this._internationalIncidents.Add (intlIncident);
	}
	internal void RemoveInternationalIncident(InternationalIncident intlIncident){
		this._internationalIncidents.Remove (intlIncident);
	}

	private void ResolveAllIntlIncidents(){
		while(this._internationalIncidents.Count > 0) {
			this._internationalIncidents [0].CancelEvent ();
		}
	}
}
