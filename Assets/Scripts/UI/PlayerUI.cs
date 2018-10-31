using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    public TextMeshProUGUI manaText;
    public TextMeshProUGUI suppliesText;
    public TextMeshProUGUI impsText;

    public Image threatFiller;
    public ScrollRect minionsScrollRect;
    public LayoutElement minionsScrollRectLE;

    public GameObject minionPrefab;
    public GameObject minionsHolderGO;
    public Transform minionsContentTransform;
    public TweenPosition minionContentTweenPos;
    public Button upScrollButton;
    public Button downScrollButton;
    public List<MinionItem> minionItems;
    public bool isMinionsMenuShowing;

    public Toggle goalsToggle;
    public Toggle intelToggle;
    public Toggle inventoryToggle;
    public Toggle factionToggle;
    public ToggleGroup minionSortingToggleGroup;

    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private MINIONS_SORT_TYPE _minionSortType;

    #region getters/setters
    public MINIONS_SORT_TYPE minionSortType {
        get { return _minionSortType; }
    }
    #endregion
    void Awake() {
        Instance = this;
        minionItems = new List<MinionItem>();
        //Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        //Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
    }

    public void UpdateUI() {
        manaText.text = PlayerManager.Instance.player.currencies[CURRENCY.MANA].ToString();
        //redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        //greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        suppliesText.text = PlayerManager.Instance.player.currencies[CURRENCY.SUPPLY].ToString();
        impsText.text = "Imps: " + PlayerManager.Instance.player.currencies[CURRENCY.IMP].ToString() + "/" + PlayerManager.Instance.player.maxImps.ToString();
        //threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    #region PlayerPicker
    public void ShowPlayerPickerAndPopulate() {
        if (PlayerManager.Instance.player.currentActiveAbility is ShareIntel) {
            UIManager.Instance.PopulatePlayerIntelsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is GiveItem) {
            UIManager.Instance.PopulatePlayerItemsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is TakeItem) {
            UIManager.Instance.PopulateLandmarkItemsInPicker();
        }
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerIntel() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerIntelsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerInventory() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerItemsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ToggleIntelMenu(bool isOn) {
        if (isOn) {
            ShowPlayerPickerIntel();
        } else {
            UIManager.Instance.HidePlayerPicker();
        }
    }
    public void ToggleInventoryMenu(bool isOn) {
        if (isOn) {
            ShowPlayerPickerInventory();
        } else {
            UIManager.Instance.HidePlayerPicker();
        }
    }
    public void SetBottomMenuTogglesState(bool isOn) {
        goalsToggle.isOn = isOn;
        intelToggle.isOn = isOn;
        inventoryToggle.isOn = isOn;
        factionToggle.isOn = isOn;
    }
    #endregion

    #region Minions
    public void OnStartMinionUI() {
        OnScroll(Vector2.zero);
    }
    public void MinionDragged(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        MinionItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<MinionItem>();
        minionItem.portrait.SetBorderState(true);
    }
    public void MinionCancel(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        MinionItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<MinionItem>();
        minionItem.portrait.SetBorderState(false);
        //minionItem.SetEnabledState(false);
    }
    //public void CollapseMinionHolder() {
    //    //minionsHolderGO.GetComponent<TweenPosition>().PlayReverse();
    //}
    //public void UncollapseMinionHolder() {
    //    //minionsHolderGO.GetComponent<TweenPosition>().PlayForward();
    //}
    public void ScrollUp() {
        float y = minionsContentTransform.localPosition.y - 115f;
        if(y < 0f) {
            y = 0f;
        }
        //minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
        minionContentTweenPos.from = minionsContentTransform.localPosition;
        minionContentTweenPos.to = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
        minionContentTweenPos.ResetToBeginning();
        minionContentTweenPos.PlayForward();
    }
    public void ScrollDown() {
        float y = minionsContentTransform.localPosition.y + 115f;
        float height = minionsScrollRectLE.preferredHeight;
        if (y > height) {
            y = height;
        }
        //minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
        minionContentTweenPos.from = minionsContentTransform.localPosition;
        minionContentTweenPos.to = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
        minionContentTweenPos.ResetToBeginning();
        minionContentTweenPos.PlayForward();
    }
    public void SortByLvlMinions() {
        _minionSortType = MINIONS_SORT_TYPE.LEVEL;
        PlayerManager.Instance.player.SortByLevel();
    }
    public void SortByTypeMinions() {
        _minionSortType = MINIONS_SORT_TYPE.TYPE;
        PlayerManager.Instance.player.SortByType();
    }
    public void SortByDefaultMinions() {
        _minionSortType = MINIONS_SORT_TYPE.DEFAULT;
        PlayerManager.Instance.player.SortByDefault();
    }
    public void OnToggleSortLvlMinions(bool state) {
        if (state) {
            SortByLvlMinions();
        } else {
            if (!minionSortingToggleGroup.AnyTogglesOn()) {
                SortByDefaultMinions();
            }
        }
    }
    public void OnToggleSortTypeMinions(bool state) {
        if (state) {
            SortByTypeMinions();
        } else {
            if (!minionSortingToggleGroup.AnyTogglesOn()) {
                SortByDefaultMinions();
            }
        }
    }
    public void OnScroll(Vector2 vector2) {
        if (minionsContentTransform.localPosition.y == 0f) {
            //on top
            upScrollButton.gameObject.SetActive(false);
            downScrollButton.gameObject.SetActive(true);
        } else if (minionsContentTransform.localPosition.y == minionsScrollRectLE.preferredHeight) {
            //on bottom
            upScrollButton.gameObject.SetActive(true);
            downScrollButton.gameObject.SetActive(false);
        } else {
            upScrollButton.gameObject.SetActive(true);
            downScrollButton.gameObject.SetActive(true);
        }
    }
    public void ResetAllMinionItems() {
        for (int i = 0; i < minionItems.Count; i++) {
            minionItems[i].SetMinion(null);
        }
    }
    public void OnMaxMinionsChanged() {
        //load the number of minion slots the player has
        if (minionItems.Count > PlayerManager.Instance.player.maxMinions) {
            //if the current number of minion items is greater than the slots that the player has
            int excess = minionItems.Count - PlayerManager.Instance.player.maxMinions; //check the number of excess items
            List<MinionItem> unoccupiedItems = GetUnoccupiedMinionItems(); //check the number of items that are unoccupied
            if (excess > 0 && unoccupiedItems.Count > 0) { //if there are unoccupied items
                for (int i = 0; i < excess; i++) { //loop through the number of excess items, then destroy any unoccupied items
                    MinionItem item = unoccupiedItems.ElementAtOrDefault(i);
                    if (item != null) {
                        RemoveMinionItem(item);
                    }
                }
            }
        } else {
            //if the current number of minion items is less than the slots the player has, instantiate the new slots
            int remainingSlots = PlayerManager.Instance.player.maxMinions - minionItems.Count;
            for (int i = 0; i < remainingSlots; i++) {
                CreateMinionItem().SetMinion(null);
            }
        }
    }
    public MinionItem GetUnoccupiedMinionItem() {
        for (int i = 0; i < minionItems.Count; i++) {
            MinionItem item = minionItems[i];
            if (item.minion == null) {
                return item;
            }
        }
        return null;
    }
    private MinionItem CreateMinionItem() {
        GameObject minionItemGO = UIManager.Instance.InstantiateUIObject(minionPrefab.name, minionsContentTransform);
        MinionItem minionItem = minionItemGO.GetComponent<MinionItem>();
        minionItems.Add(minionItem);
        return minionItem;
    }
    public void RemoveMinionItem(MinionItem item) {
        if(minionItems.Count <= PlayerManager.Instance.player.maxMinions) {
            item.transform.SetAsLastSibling();
            item.SetMinion(null);
        } else {
            minionItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }

    }
    private List<MinionItem> GetUnoccupiedMinionItems() {
        List<MinionItem> items = new List<MinionItem>();
        for (int i = 0; i < minionItems.Count; i++) {
            MinionItem item = minionItems[i];
            if (item.minion == null) {
                items.Add(item);
            }
        }
        return items;
    }
    #endregion

    public void SetMinionsMenuShowing(bool state) {
        isMinionsMenuShowing = state;
    }

    private void OnInteractionMenuOpened() {
        if (this.isMinionsMenuShowing) {
            //if the menu is showing update it's open position
            //only open halfway
            tweener.SetAnimationPosition(openPosition, halfPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        } else {
            //only open halfway
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        }
    }
    private void OnInteractionMenuClosed() {
        if (this.isMinionsMenuShowing) {
            tweener.SetAnimationPosition(halfPosition, openPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        } else {
            //reset positions to normal
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        }
    }
}
