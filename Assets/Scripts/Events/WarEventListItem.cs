using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarEventListItem : MonoBehaviour {

	public delegate void OnClickEvent(Campaign campaign);
	public OnClickEvent onClickEvent;

	private Campaign campaign;
	[SerializeField] private UILabel eventTitleLbl;
	[SerializeField] private UILabel eventDateLbl;

	private Kingdom ownerOfThisItem;

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
