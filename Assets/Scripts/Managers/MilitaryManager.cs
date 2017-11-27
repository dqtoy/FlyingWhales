using UnityEngine;
using System.Collections;

public class MilitaryManager {

	private Kingdom _kingdom;

	internal int maxGeneral;

	public MilitaryManager(Kingdom kingdom){
		this._kingdom = kingdom;
		UpdateMaxGenerals ();
		ScheduleCreateGeneral ();
	}

	private void ScheduleCreateGeneral(){
		GameDate newDate = new GameDate (GameManager.Instance.month, 1, GameManager.Instance.year);
		newDate.AddMonths (1);

		SchedulingManager.Instance.AddEntry (newDate, () => CreateGeneral ());
	}
	private void CreateGeneral(){
		
	}
	internal void UpdateMaxGenerals(){
		if(this._kingdom.kingdomSize == KINGDOM_SIZE.SMALL){
			this.maxGeneral = 3;
		}else if(this._kingdom.kingdomSize == KINGDOM_SIZE.MEDIUM){
			this.maxGeneral = 5;
		}else if(this._kingdom.kingdomSize == KINGDOM_SIZE.LARGE){
			this.maxGeneral = 7;
		}
		if(this._kingdom.king.otherTraits.Contains(TRAIT.MILITANT) || this._kingdom.king.otherTraits.Contains(TRAIT.HOSTILE)){
			this.maxGeneral += 1;
		}else if(this._kingdom.king.otherTraits.Contains(TRAIT.PACIFIST)){
			this.maxGeneral -= 1;
		}
	}
}
