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
		this.eventTitleLbl.text = "Campaign against " + _campaign.targetCity.name;
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this._campaign);
		}
	}
}
