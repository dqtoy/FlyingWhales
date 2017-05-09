using UnityEngine;
using System.Collections;

public class GeneralHover : MonoBehaviour {
	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			if(this.transform.parent.GetComponent<GeneralObject>().general.assignedCampaign != null){
				if (this.transform.parent.GetComponent<GeneralObject>().general.assignedCampaign.leader != null) {
					Campaign chosenCampaign = this.transform.parent.GetComponent<GeneralObject>().general.assignedCampaign.leader.campaignManager.SearchCampaignByID (this.transform.parent.GetComponent<GeneralObject>().general.assignedCampaign.id);
					if (chosenCampaign != null) {
						string info = this.CampaignInfo (chosenCampaign);
						UIManager.Instance.ShowSmallInfo (info, UIManager.Instance.transform);
						this.HighlightPath ();
					}
				}
			}
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		if (this.transform.parent.GetComponent<GeneralObject> ().general.assignedCampaign != null) {
			for (int i = 0; i < this.transform.parent.GetComponent<GeneralObject> ().general.roads.Count; i++) {
				this.transform.parent.GetComponent<GeneralObject> ().general.roads [i].highlightGO.SetActive (true);
			}
		}
	}

	void UnHighlightPath(){
		if (this.transform.parent.GetComponent<GeneralObject> ().general.assignedCampaign != null) {
			for (int i = 0; i < this.transform.parent.GetComponent<GeneralObject> ().path.Count; i++) {
				this.transform.parent.GetComponent<GeneralObject> ().path [i].highlightGO.SetActive(false);
			}
		}
	}
	private string CampaignInfo(Campaign campaign){
		string info = string.Empty;
		info += "id: " + campaign.id;
		info += "\n";

		info += "campaign type: " + campaign.campaignType.ToString ();
		info += "\n";

		info += "general: " + this.transform.parent.GetComponent<GeneralObject> ().general.citizen.name;
		info += "\n";

		info += "target city: " + campaign.targetCity.name;
		info += "\n";
		if (campaign.rallyPoint == null) {
			info += "rally point: N/A";
		} else {
			info += "rally point: " + campaign.rallyPoint.name; 
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

		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
			if (campaign.expiration == -1) {
				info += "expiration: none";
			} else {
				info += "will expire in " + campaign.expiration + " days";
			}
		} else {
			info += "expiration: none";

		}

		return info;
	}

}
