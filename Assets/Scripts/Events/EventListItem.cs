using UnityEngine;
using System.Collections;

public class EventListItem : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UILabel eventTitleLbl;
	public UILabel eventDateLbl;

	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
		eventTitleLbl.text = gameEvent.eventType.ToString();
		eventDateLbl.text = ((MONTH)gameEvent.startMonth).ToString () + " " + gameEvent.startWeek.ToString () + ", " + gameEvent.startYear.ToString ();
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.gameEvent);
		}
	}
}
