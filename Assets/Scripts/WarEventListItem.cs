using UnityEngine;
using System.Collections;

public class WarEventListItem : MonoBehaviour {

	public delegate void OnClickEvent(Campaign gameEvent);
	public OnClickEvent onClickEvent;

	private Campaign campaign;
	public UILabel eventTitleLbl;
	public UILabel eventDateLbl;

	public Kingdom ownerOfThisItem;

	internal void SetCampaign(Campaign campaign, Kingdom ownerOfThisItem){
		this.campaign = campaign;
		this.eventTitleLbl.text = "Campaign against " + campaign.targetCity.name;
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.campaign);
		}
	}
}
