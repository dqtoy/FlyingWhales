using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarEventListParent : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	[SerializeField] private UILabel eventTitleLbl;
	[SerializeField] private UIGrid _eventsGrid;

	[SerializeField] private GameObject arrow;
	[SerializeField] private TweenRotation tweenRotation;

	private War _war;
	private Kingdom targetKingdom;

	#region getters/setters
	public UIGrid eventsGrid{
		get { return this._eventsGrid; }
	}
    public War war {
        get { return this._war; }
    }
	#endregion

	public void SetWarEvent(War war, Kingdom targetKingdom){
		this._war = war;
		this.targetKingdom = targetKingdom;
		this.eventTitleLbl.text = "War with " + targetKingdom.name;
	}

	public void ToggleList(){
		UIManager.Instance.RepositionKingdomEventsTable();
		this._eventsGrid.gameObject.SetActive(!eventsGrid.gameObject.activeSelf);
	}

	public void Reset(){
		arrow.transform.localEulerAngles = new Vector3 (0f, 0f, -90f);
		tweenRotation.from = new Vector3 (0f, 0f, -90f);
		tweenRotation.to = new Vector3 (0f, 0f, -180f);
		this._eventsGrid.gameObject.SetActive(false);
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent((GameEvent)this.war);
		}
	}
}
