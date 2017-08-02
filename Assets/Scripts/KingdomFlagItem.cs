using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomFlagItem : MonoBehaviour {

	public delegate void OnHoverOver();
	public OnHoverOver onHoverOver;

	public delegate void OnHoverExit ();
	public OnHoverExit onHoverExit;

	internal Kingdom kingdom;

	[SerializeField] private UI2DSprite _kingdomColorSprite;
    [SerializeField] private TweenPosition _tweenPos;
    [SerializeField] private UIGrid _eventsGrid;
    [SerializeField] private GameObject deathIcon;
	private bool isHovering = false;

    #region getters/setters
    public UIGrid eventsGrid {
        get { return _eventsGrid; }
    }
    #endregion

    internal void SetKingdom(Kingdom kingdom){
		this.kingdom = kingdom;
		this._kingdomColorSprite.color = kingdom.kingdomColor;
    }

	//void OnClick(){
 //       this.SetAsSelected();
 //   }

    void OnDoubleClick() {
        //Debug.Log("DOUBLE CLICK!: " + kingdom.name);
        CameraMove.Instance.CenterCameraOn(kingdom.capitalCity.hexTile.gameObject);
    }

    //internal void SetAsSelected() {
    //    if (UIManager.Instance.currentlyShowingKingdom != null && 
    //        UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
    //        return;
    //    }
    //    this.PlayAnimation();
    //    SetKingdomAsActive();
    //}

    //private void PlayAnimation() {
    //    _tweenPos.enabled = true;
    //    _tweenPos.PlayForward();
    //}

    //public void PlayAnimationReverse() {
    //    _tweenPos.ResetToBeginning();
    //    _tweenPos.enabled = true;
    //    _tweenPos.PlayForward();
    //    //_tweenPos.ReverseValues();
    //}

    //public void SetKingdomAsActive() {
    //    UIManager.Instance.SetKingdomAsActive(this.kingdom);
    //}

    public void AddGameObjectToGrid(GameObject GO) {
        _eventsGrid.AddChild(GO.transform);
        GO.transform.localPosition = Vector3.zero;
        _eventsGrid.Reposition();
    }

    public void AddEventToGrid(GameEvent gameEventToAdd) {
        List<EventItem> inactiveEventItems = _eventsGrid.GetChildList().Where(x => !x.gameObject.activeSelf).Select(x => x.GetComponent<EventItem>()).ToList();
        if(inactiveEventItems.Count > 0) {
            EventItem eventItemToUse = inactiveEventItems.First();
            eventItemToUse.SetEvent(gameEventToAdd);
            eventItemToUse.gameObject.SetActive(true);
        } else {
            GameObject eventGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.gameEventPrefab, _eventsGrid.transform);
            AddGameObjectToGrid(eventGO);
            eventGO.GetComponent<EventItem>().SetEvent(gameEventToAdd);
        }
    }

    #region Monobehaviour Functions
    void OnHover(bool isOver) {
        if (isOver) {
            this.isHovering = true;
            UIManager.Instance.ShowSmallInfo("[b]" + this.kingdom.name + "[/b]");
            if (onHoverOver != null) {
                this.onHoverOver();
            }
        } else {
            this.isHovering = false;
            UIManager.Instance.HideSmallInfo();
            if (onHoverExit != null) {
                this.onHoverExit();
            }
        }
    }

    void Update() {
        if (this.isHovering) {
            UIManager.Instance.ShowSmallInfo("[b]" + this.kingdom.name + "[/b]");
        }
        if(kingdom != null) {
            if (kingdom.isDead) {
                deathIcon.SetActive(true);
            } else {
                deathIcon.SetActive(false);
            }
        }
    }
    #endregion

}
