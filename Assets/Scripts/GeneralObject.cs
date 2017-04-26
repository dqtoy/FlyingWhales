using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneralObject : MonoBehaviour {
	public General general;
	public TextMesh textMesh;
	public List<HexTile> path;

	internal void Init(){
		this.GetComponent<BoxCollider2D>().enabled = true;
		if(this.general != null){
			this.textMesh.text = this.general.army.hp.ToString ();
			this.path = this.general.roads;
		}
//		StartCoroutine (Initialize());
	}
	private IEnumerator Initialize(){
		yield return null;
		this.GetComponent<BoxCollider2D>().enabled = true;
		if(this.general != null){
			this.textMesh.text = this.general.army.hp.ToString ();
			this.path = this.general.roads;
		}
	}
	void OnTriggerEnter2D(Collider2D other){
		if(this.tag == "General" && other.tag == "General"){
			if(this.gameObject != null && other.gameObject != null){
				if(!Utilities.AreTwoGeneralsFriendly(other.gameObject.GetComponent<GeneralObject>().general, this.general)){
					if(!Utilities.AreTwoGeneralsFriendly(this.general, other.gameObject.GetComponent<GeneralObject>().general)){
						if(this.general.army.hp > 0 && other.gameObject.GetComponent<GeneralObject> ().general.army.hp > 0){
							CombatManager.Instance.BattleMidway (this.general, other.gameObject.GetComponent<GeneralObject> ().general);
						}
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
	internal void UpdateUI(){
		if(this.general != null){
			this.textMesh.text = this.general.army.hp.ToString ();
		}
	}

	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			if(this.general.warLeader != null){
				Campaign chosenCampaign = this.general.warLeader.campaignManager.SearchCampaignByID (this.general.campaignID);
				if(chosenCampaign != null){
					string info = this.CampaignInfo (chosenCampaign);
					UIManager.Instance.ShowSmallInfo (info, UIManager.Instance.transform);
					HighlightPath ();
				}
			}
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
		UnHighlightPath ();
	}
	void HighlightPath(){
		for (int i = 0; i < this.general.roads.Count; i++) {
			if (!this.general.roads [i].isHabitable) {
				this.general.roads [i].SetTileColor (Color.gray);
			}
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.path.Count; i++) {
			if (!this.path [i].isHabitable) {
				this.path [i].SetTileColor (Color.white);
			}
		}
	}
	private string CampaignInfo(Campaign campaign){
		string info = string.Empty;
		info += "id: " + campaign.id;
		info += "\n";

		info += "campaign type: " + campaign.campaignType.ToString ();
		info += "\n";

		info += "general: " + this.general.citizen.name;
		info += "\n";

		info += "target city: " + campaign.targetCity.name;
		info += "\n";
		if(campaign.rallyPoint == null){
			info += "rally point: N/A";
		}else{
			info += "rally point: " + campaign.rallyPoint.city.name; 
		}
		info += "\n";

		info += "leader: " + campaign.leader.name;
		info += "\n";

		info += "war type: " + campaign.warType.ToString ();
		info += "\n";

		info += "needed army: " + campaign.neededArmyStrength.ToString ();
		info += "\n";

		info += "army: " + campaign.GetArmyStrength ().ToString ();
		info += "\n";

		if(campaign.campaignType == CAMPAIGN.DEFENSE){
			if(campaign.expiration == -1){
				info += "expiration: none";
			}else{
				info += "will expire in " + campaign.expiration + " weeks";
			}
		}else{
			info += "expiration: none";

		}

		return info;
	}
}
