using UnityEngine;
using System.Collections;

public class EventItem : MonoBehaviour {

	public delegate void OnClickEvent(object obj);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UI2DSprite eventIcon;

	private bool isHovering;
	private bool isPaused;
	private string toolTip;
	private float timeElapsed;

	void Start(){
		this.isHovering = false;
		this.isPaused = false;
		this.toolTip = string.Empty;
		this.timeElapsed = 0f;
		UIManager.Instance.onPauseEventExpiration += this.PauseExpirationTimer;
	}
	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo (this.toolTip);
		}
		if(!this.isPaused){
			this.timeElapsed += Time.deltaTime * 1f;
			if(this.timeElapsed >= 10f){
				HasExpired ();
			}
		}
	}

	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
	}

	public void SetSpriteIcon(Sprite sprite){
		eventIcon.sprite2D = sprite;
		eventIcon.MakePixelPerfect();
	}
	internal void StartExpirationTimer(){
		this.isPaused = false;
	}
	private void PauseExpirationTimer(bool state){
		this.isPaused = state;
	}
	private void HasExpired(){
		this.isPaused = true;
		UIManager.Instance.HideSmallInfo ();
        UIGrid parentGrid = this.transform.parent.GetComponent<UIGrid>();
        parentGrid.RemoveChild(this.transform);
		Destroy (this.gameObject);
        StartCoroutine(UIManager.Instance.RepositionGrid(parentGrid));
    }
	public IEnumerator StartExpiration(){
		yield return new WaitForSeconds (10);
		UIManager.Instance.HideSmallInfo ();
		Destroy (this.gameObject);
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.isHovering = true;
			this.toolTip = Utilities.LogReplacer (this.gameEvent.logs [0]);
			UIManager.Instance.ShowSmallInfo (this.toolTip);
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

	void OnDestroy(){
		UIManager.Instance.onPauseEventExpiration -= this.PauseExpirationTimer;
		UIManager.Instance.RepositionGridCallback (UIManager.Instance.gameEventsOfTypeGrid);

	}
}
