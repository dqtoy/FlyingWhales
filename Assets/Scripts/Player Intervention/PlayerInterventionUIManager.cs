using UnityEngine;
using System.Collections;

public class PlayerInterventionUIManager : MonoBehaviour {

	[Header("Monster Lair")]
	public GameObject selectRegionGO;
	public UIButton btnOkSelectRegion;

	public void ToggleMonsterLair(){
		this.selectRegionGO.SetActive (!this.selectRegionGO.activeSelf);
		btnOkSelectRegion.isEnabled = false;
	}
	public void OnClickOkSelectRegion(){
		
	}
}
