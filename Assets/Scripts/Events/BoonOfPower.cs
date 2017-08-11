using UnityEngine;
using System.Collections;

public class BoonOfPower : GameEvent {
	internal Kingdom ownerKingdom;
	internal GameObject avatar;
	internal HexTile hexTileSpawnPoint;

	private bool _isActivated;
	private bool _isDestroyed;

	private int activatedMonth;
	private int activatedDay;
	private int activatedYear;

	#region getters/setters
	public bool isActivated{
		get { return this._isActivated; }
	}
	#endregion
	public BoonOfPower(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile hexTile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BOON_OF_POWER;
		this.name = "Boon Of Power";
		this.ownerKingdom = null;
		this._isActivated = false;
		this._isDestroyed = false;
		this.avatar = null;
		this.hexTileSpawnPoint = hexTile;
		Initialize ();

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "event_title");


	}

	#region Overrides
	internal override void PerformAction (){
		if(GameManager.Instance.month == this.activatedMonth && GameManager.Instance.days == this.activatedDay && GameManager.Instance.year > this.activatedYear){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 35){
				DestroyThis ();
			}
		}
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
	}
	#endregion
	private void Initialize(){
		this.hexTileSpawnPoint.PutEventOnTile (this);
		//this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/BoonOfPower"), this.hexTileSpawnPoint.transform) as GameObject;
		//this.avatar.transform.localPosition = Vector3.zero;
		//this.avatar.GetComponent<BoonOfPowerAvatar>().Init(this);
	}
	internal void AddOwnership(Kingdom kingdom){
		this.ownerKingdom = kingdom;
		this.hexTileSpawnPoint.RemoveEventOnTile ();
	}
	internal void Activate(){
		this._isActivated = true;
		this.activatedMonth = GameManager.Instance.month;
		this.activatedDay = GameManager.Instance.days;
		this.activatedYear = GameManager.Instance.year;
		Messenger.AddListener("OnDayEnd", this.PerformAction);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "power_activated");
	}
	internal void TransferBoonOfPower(Kingdom kingdom, Citizen citizen){
		kingdom.CollectBoonOfPower (this);
		GameObject.Destroy (this.avatar);
		if(citizen == null){
			//Discovered by structure/tile
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "discovery_structure");
			newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		}else{
			//Discovered by an agent
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "discovery_agent");
			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		}
		this.EventIsCreated ();
	}
	private void DestroyThis(){
		this.DoneEvent ();
		this._isDestroyed = true;
		this._isActivated = false;
		this.ownerKingdom = null;
		Messenger.RemoveListener("OnDayEnd", this.PerformAction);

		if(this.ownerKingdom.isAlive()){
			this.ownerKingdom.DestroyBoonOfPower (this);
		}

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "power_stop");

	}
}
