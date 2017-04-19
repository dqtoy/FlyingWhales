using UnityEngine;
using System.Collections;

public class GeneralObject : MonoBehaviour {
	public General general;
	public TextMesh textMesh;

	internal void Init(){
		StartCoroutine (Initialize());
	}
	private IEnumerator Initialize(){
		yield return null;
		this.GetComponent<BoxCollider2D>().enabled = true;
		if(this.general != null){
			this.textMesh.text = this.general.army.hp.ToString ();
		}
	}
	void OnTriggerEnter2D(Collider2D other){
		if(this.tag == "General" && other.tag == "General"){
			if(this.gameObject != null && other.gameObject != null){
				if(!Utilities.AreTwoGeneralsFriendly(other.gameObject.GetComponent<GeneralObject>().general, this.general)){
					if(!Utilities.AreTwoGeneralsFriendly(this.general, other.gameObject.GetComponent<GeneralObject>().general)){
						CombatManager.Instance.BattleMidway (this.general, other.gameObject.GetComponent<GeneralObject> ().general);
					}
				}
			}
		}


	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
		this.transform.position = targetTile.transform.position;
		if(this.general != null){
			this.textMesh.text = this.general.army.hp.ToString ();
		}
	}

	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			if(this.general.warLeader != null){
				Campaign chosenCampaign = this.general.warLeader.campaignManager.SearchCampaignByID (this.general.campaignID);
				if(chosenCampaign != null){
					UIManager.Instance.ShowCampaignInfo (chosenCampaign, this.general, UIManager.Instance.transform);

				}
			}
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideCampaignInfo();
	}
}
