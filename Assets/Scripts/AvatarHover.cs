using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarHover : MonoBehaviour {
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();
	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
		this.UnHighlightPath ();
	}
	void OnDestroy(){
		UnHighlightPath ();
	}
	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.transform.parent.GetComponent<GeneralObject>().general.roads.Count; i++) {
			this.transform.parent.GetComponent<GeneralObject>().general.roads [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.transform.parent.GetComponent<GeneralObject>().general.roads [i]);
		}
	}

	void UnHighlightPath(){
		//		if (this.transform.parent.GetComponent<GeneralObject>().general.assignedCampaign != null) {
		for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
			this.pathToUnhighlight[i].highlightGO.SetActive(false);
		}
		//		}
	}
	private string EventInfo(GameEvent gameEvent){
		string info = string.Empty;
//		info += "id: " + campaign.id;
//		info += "\n";
//
//		info += "campaign type: " + campaign.campaignType.ToString ();
//		info += "\n";
//
//		info += "general: " + this.transform.parent.GetComponent<GeneralObject>().general.citizen.name;
//		info += "\n";
//
//		info += "target city: " + campaign.targetCity.name;
//		info += "\n";
//		if (campaign.rallyPoint == null) {
//			info += "rally point: N/A";
//		} else {
//			info += "rally point: " + campaign.rallyPoint.name; 
//		}
//		info += "\n";
//
//		info += "leader: " + campaign.leader.name;
//		info += "\n";
//
//		info += "war type: " + campaign.warType.ToString ();
//		info += "\n";
//
//		info += "needed army: " + campaign.neededArmyStrength.ToString ();
//		info += "\n";
//
//		info += "army: " + campaign.GetArmyStrength ().ToString ();
//		info += "\n";
//
//		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
//			if (campaign.expiration == -1) {
//				info += "expiration: none";
//			} else {
//				info += "will expire in " + campaign.expiration + " days";
//			}
//		} else {
//			info += "expiration: none";
//
//		}
//
		return info;
	}


}
