using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarEventListItem : MonoBehaviour {

	public delegate void OnClickEvent(Campaign campaign);
	public OnClickEvent onClickEvent;

	private Campaign _campaign;
	[SerializeField] private UILabel eventTitleLbl;
	[SerializeField] private UILabel eventDateLbl;

	private Kingdom ownerOfThisItem;

	#region getters/setters
	public Campaign campaign{
		get { return this._campaign; }
	}
	#endregion

	internal void SetCampaign(Campaign _campaign, Kingdom ownerOfThisItem){
		this._campaign = _campaign;
		if (this._campaign.campaignType == CAMPAIGN.DEFENSE) {
			this.eventTitleLbl.text = "Campaign to defend " + _campaign.targetCity.name;
		} else {
			this.eventTitleLbl.text = "Campaign to invade " + _campaign.targetCity.name;
		}
		this.eventDateLbl.gameObject.SetActive(false);

	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this._campaign);
		}
	}
}
