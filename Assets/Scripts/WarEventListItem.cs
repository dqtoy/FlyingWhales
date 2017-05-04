using UnityEngine;
using System.Collections;

public class WarEventListItem : MonoBehaviour {

	public UIAnchor anchor;
	public UILabel warTitleLbl;
	public UIGrid warEventsGrid;

	public Kingdom targetKingdom;

	public void Init(Kingdom targetKingdom){
		this.targetKingdom = targetKingdom;
		warTitleLbl.text = "War with " + targetKingdom.name;
	}

	public void SetAnchorPoint(GameObject anchorPoint){
		anchor.container = anchorPoint;
	}

	public void ToggleList(){
		warEventsGrid.gameObject.SetActive(!warEventsGrid.gameObject.activeSelf);
	}
}
