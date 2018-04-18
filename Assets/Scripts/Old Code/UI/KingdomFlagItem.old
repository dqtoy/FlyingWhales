using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomFlagItem : MonoBehaviour {

	public delegate void OnHoverOver();
	public OnHoverOver onHoverOver;

	public delegate void OnHoverExit();
	public OnHoverExit onHoverExit;

    public delegate void OnClickKingdomFlag(Kingdom clickedKingdom, KingdomFlagItem clickedFlag);
    public OnClickKingdomFlag onClickKingdomFlag;

    internal Kingdom kingdom;

	[SerializeField] private UI2DSprite _kingdomColorSprite;
    [SerializeField] private TweenPosition _tweenPos;
    [SerializeField] private UIGrid _eventsGrid;
    [SerializeField] private GameObject deathIcon;
    [SerializeField] private GameObject kingdomInfoGO;
    [SerializeField] private UILabel kingdomInfoLbl;
    private bool isHovering = false;

    private bool isHoverEnabled = true;

    #region getters/setters
    public UIGrid eventsGrid {
        get { return _eventsGrid; }
    }
    #endregion

    internal void SetKingdom(Kingdom kingdom, bool showKingdomInfo = false){
		this.kingdom = kingdom;
		this._kingdomColorSprite.color = kingdom.kingdomColor;

        if (showKingdomInfo) {
            ShowKingdomInfo();
        } else {
            HideKingdomInfo();
        }
    }

    private void ShowKingdomInfo() {
        isHoverEnabled = false;
        kingdomInfoLbl.text = kingdom.name + "\n" + Utilities.NormalizeString(kingdom.kingdomType.ToString()) + "\nCities: " + kingdom.cities.Count;
        kingdomInfoGO.SetActive(true);
    }

    private void HideKingdomInfo() {
        isHoverEnabled = true;
        kingdomInfoGO.SetActive(false);
    }

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
            //GameObject eventGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.gameEventPrefab.name, _eventsGrid.transform);
            //AddGameObjectToGrid(eventGO);
            //eventGO.GetComponent<EventItem>().SetEvent(gameEventToAdd);
        }
    }

    #region Monobehaviour Functions
    private void OnDoubleClick() {
        //Debug.Log("DOUBLE CLICK!: " + kingdom.name);
        CameraMove.Instance.CenterCameraOn(kingdom.capitalCity.hexTile.gameObject);
    }
    private void OnClick() {
        if(onClickKingdomFlag != null) {
            onClickKingdomFlag(kingdom, this);
        }
    }
    private void OnHover(bool isOver) {
        if (isHoverEnabled) {
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
    }

    private void Update() {
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
