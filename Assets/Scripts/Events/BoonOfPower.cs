using UnityEngine;
using System.Collections;

public class BoonOfPower : GameEvent {
	internal Kingdom ownerKingdom;
	internal GameObject avatar;

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
	public BoonOfPower(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BOON_OF_POWER;
		this.name = "Boon Of Power";
		this.ownerKingdom = null;
		this._isActivated = false;
		this._isDestroyed = false;
		this.avatar = null;
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
	#endregion
	internal void Initialize(HexTile hexTile){
		this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/BoonOfPower"), hexTile.transform) as GameObject;
		this.avatar.transform.localPosition = Vector3.zero;
		this.avatar.GetComponent<BoonOfPowerAvatar>().Init(this);
	}
	internal void AddOwnership(Kingdom kingdom){
		this.ownerKingdom = kingdom;
	}
	internal void Activate(){
		this._isActivated = true;
		this.activatedMonth = GameManager.Instance.month;
		this.activatedDay = GameManager.Instance.days;
		this.activatedYear = GameManager.Instance.year;
		EventManager.Instance.onWeekEnd.AddListener (this.PerformAction);
	}

	private void DestroyThis(){
		this.DoneEvent ();
		this._isDestroyed = true;
		this._isActivated = false;
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);

		if(this.ownerKingdom.isAlive()){
			this.ownerKingdom.DestroyBoonOfPower (this);
		}
	}
}
