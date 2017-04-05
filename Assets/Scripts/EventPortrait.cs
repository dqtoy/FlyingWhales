using UnityEngine;
using System.Collections;

public class EventPortrait : MonoBehaviour {

	public UILabel dateLbl;
	public UI2DSprite iconSprite;

	public Sprite[] icons;
	public GameEvent chosenEvent;

	public void SetEvent(GameEvent chosenEvent){
		this.chosenEvent = chosenEvent;
		this.iconSprite.sprite2D = GetSprite ();
		this.iconSprite.MakePixelPerfect ();
		this.dateLbl.text = "[b]" + ((MONTH)chosenEvent.startMonth).ToString () + " " + chosenEvent.startWeek.ToString () + ", " + chosenEvent.startYear.ToString () + "[/b]";

	}
	private Sprite GetSprite(){
		switch(this.chosenEvent.eventType){
		case EVENT_TYPES.BORDER_CONFLICT:
			return icons [0];
		case EVENT_TYPES.STATE_VISIT:
			return icons [1];
		case EVENT_TYPES.ASSASSINATION:
			return icons [2];
		case EVENT_TYPES.ESPIONAGE:
			return icons [3];
		case EVENT_TYPES.RAID:
			return icons [4];
		case EVENT_TYPES.INVASION_PLAN:
			return icons [5];
		case EVENT_TYPES.JOIN_WAR_REQUEST:
			return icons [6];
		case EVENT_TYPES.MILITARIZATION:
			return icons [7];
		case EVENT_TYPES.POWER_GRAB:
			return icons [8];
		case EVENT_TYPES.EXHORTATION:
			return icons [9];
		}
		return null;
	}
	void OnHover(bool isOver){
		if (isOver) {
			UIManager.Instance.ShowSmallInfo ("[b]" + this.chosenEvent.description + "[/b]", this.transform);
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}
}
