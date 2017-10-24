using UnityEngine;
using System.Collections;

public class CorpseMound: MonoBehaviour {

	[SerializeField] private UILabel corpsesLbl;

	private int _corpseCount;
	private HexTile _tile;
	private GameObject _corpseMoundGO;
	private GameDate _destroyDate;

	#region Getters/Setters
	public int corpseCount{
		get { return this._corpseCount; }
	}
	#endregion
	internal void Initialize(HexTile tile, int amount){
		this._tile = tile;
		this._corpseMoundGO = this.gameObject;
		this._corpseCount = amount;
		this.corpsesLbl.text = this._corpseCount.ToString();
		SetDestroyDate ();
	}
	internal void AdjustCorpseCount (int amount){
		this._corpseCount += amount;
		this.corpsesLbl.text = this._corpseCount.ToString();
		if(amount > 0){
			OverrideDestroyDate ();
		}
	}

	private void OverrideDestroyDate(){
		if(GameManager.Instance.month == this._destroyDate.month && GameManager.Instance.days == this._destroyDate.day && GameManager.Instance.year == this._destroyDate.year){
			return;
		}
		SchedulingManager.Instance.RemoveSpecificEntry (this._destroyDate.month, this._destroyDate.day, this._destroyDate.year, () => DestroyCorpseMound ());
		SetDestroyDate ();
	}
	private void SetDestroyDate(){
		GameDate destroyDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		destroyDate.AddYears (5);
		this._destroyDate = destroyDate;
		SchedulingManager.Instance.AddEntry (destroyDate, () => DestroyCorpseMound ());
	}
	internal void DestroyCorpseMound(){
		ObjectPoolManager.Instance.DestroyObject(this._corpseMoundGO);
		this._tile.SetCorpseMound (null);
	}
}
