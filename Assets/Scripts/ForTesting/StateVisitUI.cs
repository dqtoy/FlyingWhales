using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateVisitUI : MonoBehaviour {

	public UIPopupList dropdownKingdoms;
	public UIPopupList dropdownReasons;

	public UILabel kingdomsLbl;
	public UILabel reasonsLbl;

	void OnEnable(){
		this.dropdownKingdoms.Clear ();
		this.dropdownReasons.Clear ();

		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(UIManager.Instance.currentlyShowingCity != null){
				if(UIManager.Instance.currentlyShowingCity.kingdom.id != KingdomManager.Instance.allKingdoms[i].id){
					this.dropdownKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
				}
			}else{
				this.dropdownKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
			}
		}

		STATEVISIT_TRIGGER_REASONS[] reasons = (STATEVISIT_TRIGGER_REASONS[])System.Enum.GetValues(typeof(STATEVISIT_TRIGGER_REASONS));
		for(int i = 0; i < reasons.Length; i++){
			this.dropdownReasons.AddItem (reasons[i].ToString(), reasons[i]);
		}
	}
	public void OnValueChangeKingdom(){
		this.kingdomsLbl.text = this.dropdownKingdoms.value;
	}
	public void OnValueChangeReason(){
		this.reasonsLbl.text = this.dropdownReasons.value;
	}
	public void OnClickCreateEvent(){
		STATEVISIT_TRIGGER_REASONS reason = (STATEVISIT_TRIGGER_REASONS)this.dropdownReasons.data;
		Kingdom selectedKingdom = this.dropdownKingdoms.data as Kingdom;
		Citizen targetKing = selectedKingdom.king;
		if((targetKing.spouse != null && !targetKing.spouse.isDead) || targetKing.city.kingdom.successionLine.Count > 0){
			Citizen visitor = null;
			if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count > 0){
				int chance = UnityEngine.Random.Range (0, 2);
				if(chance == 0){
					visitor = targetKing.spouse;
				}else{
					visitor = targetKing.city.kingdom.successionLine [0];
				}
			}else if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count <= 0){
				visitor = targetKing.spouse;
			}else if(targetKing.spouse == null && targetKing.city.kingdom.successionLine.Count > 0){
				visitor = targetKing.city.kingdom.successionLine [0];
			}
			if(visitor != null){
//				StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, UIManager.Instance.currentlyShowingCity.kingdom.king, selectedKingdom, visitor, null);
//				EventManager.Instance.AddEventToDictionary (stateVisit);
			}else{
				Debug.Log ("CANNOT DO STATE VISIT. NO VISITOR AVAILABLE");
			}
		}else{
			Debug.Log ("CANNOT DO STATE VISIT. NO VISITOR AVAILABLE");
		}
	}
}
