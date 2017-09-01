using UnityEngine;
using System.Collections;

public class InvasionPlanUI : MonoBehaviour {

	public UIPopupList dropdownKingdoms;

	void OnEnable(){
		this.dropdownKingdoms.Clear ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(UIManager.Instance.currentlyShowingCity != null){
				if(UIManager.Instance.currentlyShowingCity.kingdom.id != KingdomManager.Instance.allKingdoms[i].id){
					this.dropdownKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
				}
			}else{
				this.dropdownKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
			}
		}
	}

	public void OnClickOk(){
		if(this.dropdownKingdoms.data != null && UIManager.Instance.currentlyShowingCity != null){
			Kingdom selectedKingdom = this.dropdownKingdoms.data as Kingdom;
			Kingdom sourceKingdom = UIManager.Instance.currentlyShowingCity.kingdom;
//			sourceKingdom.GetRelationshipWithKingdom (selectedKingdom).CreateInvasionPlan(null);
//			InvasionPlan invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
//				sourceKingdom.king, sourceKingdom, selectedKingdom, null, null);
		}

	}
}
