using UnityEngine;
using System.Collections;

public class PlayerInterventionUIManager : MonoBehaviour {
	public static PlayerInterventionUIManager Instance;

	[Header("Monster Lair")]
	public GameObject selectRegionGO;
	public UIButton btnOkSelectRegion;
	public ButtonToggle btnMonsterLair;

	void Awake(){
		Instance = this;
	}
	public void ToggleMonsterLair(){
		this.selectRegionGO.SetActive (!this.selectRegionGO.activeSelf);
		if(this.selectRegionGO.activeSelf){
			btnOkSelectRegion.isEnabled = false;
		}
	}
	public void OnClickOkSelectRegion(){
		Region selectedRegion = SelectRegion.Instance.selectedTile.region;
		//PlayerInterventionManager.Instance.SpawnMonsterLair (selectedRegion);
		selectRegionGO.SetActive (false);
		btnMonsterLair.SetClickState (false);
	}
}
