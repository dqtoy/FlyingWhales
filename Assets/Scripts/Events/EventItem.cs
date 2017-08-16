using UnityEngine;
using System.Collections;
using EZObjectPools;

public class EventItem : PooledObject {

	public delegate void OnClickEvent(object obj);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UI2DSprite eventIcon;
	public GameObject goExclaimation;

	private bool isHovering;
	private bool isPaused;
	private string toolTip;
	private float timeElapsed;

	void Awake(){
		this.ActivateNewLogIndicator ();
	}
	void Start(){
		this.isHovering = false;
//		this.isPaused = false;
		this.toolTip = string.Empty;
		this.timeElapsed = 0f;
//		UIManager.Instance.onPauseEventExpiration += this.PauseExpirationTimer;
	}
	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo (this.toolTip);
		}
//		if(!this.isPaused){
//			this.timeElapsed += Time.deltaTime * 1f;
//			if(this.timeElapsed >= 10f){
//				HasExpired ();
//			}
//		}
	}

	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
        SetSpriteIcon(UIManager.Instance.GetSpriteForEvent(gameEvent.eventType));
        onClickEvent += UIManager.Instance.ShowEventLogs;
        StartExpirationTimer();
        this.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        gameEvent.goEventItem = this.gameObject;
    }

	public void SetSpriteIcon(Sprite sprite){
		eventIcon.sprite2D = sprite;
		eventIcon.MakePixelPerfect();
	}
	internal void StartExpirationTimer(){
		this.isPaused = GameManager.Instance.isPaused;
	}
	private void PauseExpirationTimer(bool state){
		this.isPaused = state;
	}
	internal void HasExpired(){
		this.isPaused = true;

        UIManager.Instance.HideSmallInfo();
        UIGrid parentGrid = this.transform.parent.GetComponent<UIGrid>();
        UIScrollView parentScrollView = parentGrid.transform.parent.GetComponent<UIScrollView>();
        parentGrid.RemoveChild(this.transform);

        UIManager.Instance.RepositionGridCallback(parentGrid);
        StartCoroutine(UIManager.Instance.RepositionScrollView(parentScrollView));

        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
	internal void ActivateNewLogIndicator(){
		this.goExclaimation.SetActive (true);
	}
	internal void DeactivateNewLogIndicator(){
		this.goExclaimation.SetActive (false);
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

    #region overrides
    public override void Reset() {
        if (onClickEvent != null) {
            onClickEvent -= UIManager.Instance.ShowEventLogs;
        }
        //		UIManager.Instance.onPauseEventExpiration -= this.PauseExpirationTimer;
        //		UIManager.Instance.RepositionGridCallback (UIManager.Instance.gameEventsOfTypeGrid);
    }
    #endregion

}
