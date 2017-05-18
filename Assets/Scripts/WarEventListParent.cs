using UnityEngine;
using System.Collections;

public class WarEventListParent : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	private War war;

	public GameObject anchorPoint;
	public UIAnchor anchor;
	public UILabel eventTitleLbl;
	public UIGrid eventsGrid;

	public GameObject arrow;
	public TweenRotation tweenRotation;

	private Kingdom targetKingdom;

	public void SetWarEvent(War war, Kingdom targetKingdom){
		this.war = war;
		this.targetKingdom = targetKingdom;
		this.eventTitleLbl.text = "War with " + targetKingdom.name;
	}

	public void ToggleList(){
		eventsGrid.gameObject.SetActive(!eventsGrid.gameObject.activeSelf);
	}

	public void Reset(){
		arrow.transform.localEulerAngles = new Vector3 (0f, 0f, -90f);
		tweenRotation.from = new Vector3 (0f, 0f, -90f);
		tweenRotation.to = new Vector3 (0f, 0f, -180f);
		eventsGrid.gameObject.SetActive(false);
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent((GameEvent)this.war);
		}
	}
}
