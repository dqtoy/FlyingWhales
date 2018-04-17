using UnityEngine;
using System.Collections;

public class EventListParent : MonoBehaviour {

	public UILabel eventTitleLbl;
	public UIGrid eventsGrid;

	public GameObject arrow;
	public TweenRotation tweenRotation;

	public void ToggleList(){
		UIManager.Instance.RepositionKingdomEventsTable();
		eventsGrid.gameObject.SetActive(!eventsGrid.gameObject.activeSelf);
	}

	public void Reset(){
		arrow.transform.localEulerAngles = new Vector3 (0f, 0f, -90f);
		tweenRotation.from = new Vector3 (0f, 0f, -90f);
		tweenRotation.to = new Vector3 (0f, 0f, -180f);
		eventsGrid.gameObject.SetActive(false);
	}
}
