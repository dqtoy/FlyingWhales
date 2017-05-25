using UnityEngine;
using System.Collections;

public class EventItem : MonoBehaviour {

	public delegate void OnClickEvent(object obj);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UI2DSprite eventIcon;

	private bool isHovering = false;
	string toolTip = string.Empty;

	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo (this.toolTip, this.transform);
		}
	}

	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
	}

	public void SetSpriteIcon(Sprite sprite){
		eventIcon.sprite2D = sprite;
		eventIcon.MakePixelPerfect();
	}
	public void StartExpirationTimer(){
		StartCoroutine (StartExpiration ());
	}
	public IEnumerator StartExpiration(){
		yield return new WaitForSeconds (10);
		UIManager.Instance.HideSmallInfo ();
		Destroy (this.gameObject);
		UIManager.Instance.RepositionGridCallback (UIManager.Instance.gameEventsOfTypeGrid);
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.isHovering = true;
			this.toolTip = Utilities.LogReplacer (this.gameEvent.logs [0]);
			UIManager.Instance.ShowSmallInfo (this.toolTip, this.transform);
		}else{
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
		}
	}
	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.gameEvent);
		}
	}
}
