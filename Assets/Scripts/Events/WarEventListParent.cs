using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarEventListParent : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	[SerializeField] private GameObject _anchorPoint;
	[SerializeField] private UIAnchor anchor;
	[SerializeField] private UILabel eventTitleLbl;
	[SerializeField] private UIGrid _eventsGrid;

	[SerializeField] private GameObject arrow;
	[SerializeField] private TweenRotation tweenRotation;

	private War war;
	private Kingdom targetKingdom;

	#region getters/setters
	public GameObject anchorPoint{
		get { return this._anchorPoint; }
	}
	public UIGrid eventsGrid{
		get { return this._eventsGrid; }
	}
	#endregion

	public void SetWarEvent(War war, Kingdom targetKingdom){
		this.war = war;
		this.targetKingdom = targetKingdom;
		this.eventTitleLbl.text = "War with " + targetKingdom.name;
	}

	public void ToggleList(){
		this._eventsGrid.gameObject.SetActive(!eventsGrid.gameObject.activeSelf);
	}

	public void Reset(){
		arrow.transform.localEulerAngles = new Vector3 (0f, 0f, -90f);
		tweenRotation.from = new Vector3 (0f, 0f, -90f);
		tweenRotation.to = new Vector3 (0f, 0f, -180f);
		this._eventsGrid.gameObject.SetActive(false);
	}

	public void SetAnchor(GameObject anchorPoint){
		this.anchor.container = anchorPoint;
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent((GameEvent)this.war);
		}
	}
}
