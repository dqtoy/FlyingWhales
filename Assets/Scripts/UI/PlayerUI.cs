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
    public RectTransform minionsScrollRectTransform;
    public LayoutElement minionsScrollRectLE;

    public GameObject minionPrefab;
    public GameObject minionsHolderGO;
    public RectTransform minionsContentTransform;
    public TweenPosition minionContentTweenPos;
    public Button upScrollButton;
    public Button downScrollButton;
    public List<PlayerCharacterItem> minionItems;
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
    private bool _isScrollingUp;
    private bool _isScrollingDown;

    #region getters/setters
    public MINIONS_SORT_TYPE minionSortType {
        get { return _minionSortType; }
    }
    #endregion

    void Awake() {
        Instance = this;
        minionItems = new List<PlayerCharacterItem>();
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    void Start() {
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
    }
    public void UpdateUI() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        manaText.text = PlayerManager.Instance.player.currencies[CURRENCY.MANA].ToString();
        //redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        //greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        suppliesText.text = PlayerManager.Instance.player.currencies[CURRENCY.SUPPLY].ToString();
        impsText.text = "Imps: " + PlayerManager.Instance.player.currencies[CURRENCY.IMP].ToString() + "/" + PlayerManager.Instance.player.maxImps.ToString();
        //threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    #region PlayerPicker
    public void ShowPlayerPickerAndPopulate() {
        //if (PlayerManager.Instance.player.currentActiveAbility is ShareIntel) {
        //    UIManager.Instance.PopulatePlayerIntelsInPicker();
        //} else if (PlayerManager.Instance.player.currentActiveAbility is GiveItem) {
        //    UIManager.Instance.PopulatePlayerItemsInPicker();
        //} else if (PlayerManager.Instance.player.currentActiveAbility is TakeItem) {
        //    UIManager.Instance.PopulateLandmarkItemsInPicker();
        //}
        //UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerIntel() {
        //PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerIntelsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerInventory() {
        //PlayerManager.Instance.player.OnHidePlayerPicker();
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
        StartCoroutine(StartMinionUICoroutine());
    }
    private IEnumerator StartMinionUICoroutine() {
        yield return null;
        OnScroll(Vector2.zero);
    }
    public void MinionDragged(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        PlayerCharacterItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<PlayerCharacterItem>();
        //minionItem.portrait.SetBorderState(true);
    }
    public void MinionCancel(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        PlayerCharacterItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<PlayerCharacterItem>();
        //minionItem.portrait.SetBorderState(false);
        //minionItem.SetEnabledState(false);
    }
    public void ScrollUp() {
        if (!_isScrollingUp) {
            _isScrollingUp = true;
            float y = minionsContentTransform.localPosition.y - 115f;
            if (y < 0f) {
                y = 0f;
                upScrollButton.gameObject.SetActive(false);
            }
            //minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
            minionContentTweenPos.from = minionsContentTransform.localPosition;
            minionContentTweenPos.to = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
            minionContentTweenPos.SetOnFinished(OnFinishedScrollUp);
            minionContentTweenPos.ResetToBeginning();
            minionContentTweenPos.PlayForward();
        }
    }
    private void OnFinishedScrollUp() {
        _isScrollingUp = false;
    }
    public void ScrollDown() {
        if (!_isScrollingDown) {
            _isScrollingDown = true;
            float y = minionsContentTransform.localPosition.y + 115f;
            float height = minionsScrollRectLE.preferredHeight;
            if (y > height) {
                y = height;
            }
            //if((y + minionsScrollRectLE.preferredHeight) == minionsContentTransform.rect.height) {
            //    downScrollButton.gameObject.SetActive(false);
            //}
            //minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
            minionContentTweenPos.from = minionsContentTransform.localPosition;
            minionContentTweenPos.to = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
            minionContentTweenPos.SetOnFinished(OnFinishedScrollDown);
            minionContentTweenPos.ResetToBeginning();
            minionContentTweenPos.PlayForward();
        }
    }
    private void OnFinishedScrollDown() {
        _isScrollingDown = false;
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
            if (upScrollButton.gameObject.activeSelf) {
                upScrollButton.gameObject.SetActive(false);
            }
            //downScrollButton.gameObject.SetActive(true);
        } else {
            if (!upScrollButton.gameObject.activeSelf) {
                upScrollButton.gameObject.SetActive(true);
            }
        }
        if ((minionsContentTransform.localPosition.y + minionsScrollRectLE.preferredHeight) < minionsContentTransform.rect.height) {
            //on bottom
            //upScrollButton.gameObject.SetActive(true);
            if (!downScrollButton.gameObject.activeSelf) {
                downScrollButton.gameObject.SetActive(true);
            }
        } else {
            if (downScrollButton.gameObject.activeSelf) {
                downScrollButton.gameObject.SetActive(false);
            }
        }
    }
    public void ResetAllMinionItems() {
        for (int i = 0; i < minionItems.Count; i++) {
            minionItems[i].SetCharacter(null);
        }
    }
    //public void OnMaxMinionsChanged() {
    //    //load the number of minion slots the player has
    //    if (minionItems.Count > PlayerManager.Instance.player.maxMinions) {
    //        //if the current number of minion items is greater than the slots that the player has
    //        int excess = minionItems.Count - PlayerManager.Instance.player.maxMinions; //check the number of excess items
    //        List<PlayerCharacterItem> unoccupiedItems = GetUnoccupiedCharacterItems(); //check the number of items that are unoccupied
    //        if (excess > 0 && unoccupiedItems.Count > 0) { //if there are unoccupied items
    //            for (int i = 0; i < excess; i++) { //loop through the number of excess items, then destroy any unoccupied items
    //                PlayerCharacterItem item = unoccupiedItems.ElementAtOrDefault(i);
    //                if (item != null) {
    //                    RemoveCharacterItem(item);
    //                }
    //            }
    //        }
    //    } else {
    //        //if the current number of minion items is less than the slots the player has, instantiate the new slots
    //        int remainingSlots = PlayerManager.Instance.player.maxMinions - minionItems.Count;
    //        for (int i = 0; i < remainingSlots; i++) {
    //            CreateMinionItem().SetCharacter(null);
    //        }
    //    }
    //}
    public PlayerCharacterItem GetUnoccupiedCharacterItem() {
        for (int i = 0; i < minionItems.Count; i++) {
            PlayerCharacterItem item = minionItems[i];
            if (item.character == null) {
                return item;
            }
        }
        return null;
    }
    private List<PlayerCharacterItem> GetUnoccupiedCharacterItems() {
        List<PlayerCharacterItem> items = new List<PlayerCharacterItem>();
        for (int i = 0; i < minionItems.Count; i++) {
            PlayerCharacterItem item = minionItems[i];
            if (item.character == null) {
                items.Add(item);
            }
        }
        return items;
    }
    public PlayerCharacterItem CreateMinionItem() {
        GameObject minionItemGO = UIManager.Instance.InstantiateUIObject(minionPrefab.name, minionsContentTransform);
        PlayerCharacterItem minionItem = minionItemGO.GetComponent<PlayerCharacterItem>();
        minionItems.Add(minionItem);
        return minionItem;
    }
    public void RemoveCharacterItem(PlayerCharacterItem item) {
        minionItems.Remove(item);
        ObjectPoolManager.Instance.DestroyObject(item.gameObject);
    }
    #endregion

    public void SetMinionsMenuShowing(bool state) {
        isMinionsMenuShowing = state;
    }

    public void CreateNewParty() {
        if (!UIManager.Instance.partyinfoUI.isShowing) {
            UIManager.Instance.partyinfoUI.ShowCreatePartyUI();
        } else {
            UIManager.Instance.partyinfoUI.CloseMenu();
        }
    }

    public string previousMenu;
    private void OnMenuOpened(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            UIManager.Instance.ShowMinionsMenu();
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            if (string.IsNullOrEmpty(previousMenu)) {
                UIManager.Instance.HideRightMenus();
            } else if (previousMenu.Equals("minion")) {
                UIManager.Instance.ShowMinionsMenu();
            } else if (previousMenu.Equals("character")) {
                UIManager.Instance.ShowCharacterIntelMenu();
            } else if (previousMenu.Equals("location")) {
                UIManager.Instance.ShowLocationIntelMenu();
            } else if (previousMenu.Equals("faction")) {
                UIManager.Instance.ShowFactionIntelMenu();
            }
        }
    }
}
