using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    public TokensUI charactersIntelUI;

    [Header("Currency")]
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI suppliesText;
    public TextMeshProUGUI impsText;

    [Header("Minions")]
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

    [Header("Role Slots")]
    [SerializeField] private GameObject roleSlotsParent;
    [SerializeField] private RoleSlotItem[] roleSlots;

    [Header("Attack")]
    public GameObject attackGridGO;
    [SerializeField] private Sprite attackGridIconSprite;
    [SerializeField] private Sprite defenseGridIconSprite;
    public AttackSlotItem attackSlot;
    [SerializeField] private DefenseSlotItem defenseSlot;
    public SlotItem[] attackGridSlots;

    [Header("Bottom Menu")]
    public Toggle goalsToggle;
    //public Toggle intelToggle;
    public Toggle inventoryToggle;
    public Toggle factionToggle;
    public ToggleGroup minionSortingToggleGroup;

    [Header("Intel")]
    [SerializeField] private InteractionIntelItem[] intelItems;
    [SerializeField] private Toggle intelToggle;

    [Header("Miscellaneous")]
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Image combatGridAssignerIcon;

    private MINIONS_SORT_TYPE _minionSortType;
    private bool _isScrollingUp;
    private bool _isScrollingDown;
    public CombatGrid attackGridReference { get; private set; }
    public CombatGrid defenseGridReference { get; private set; }

    #region getters/setters
    public MINIONS_SORT_TYPE minionSortType {
        get { return _minionSortType; }
    }
    #endregion

    void Awake() {
        Instance = this;
        //minionItems = new List<PlayerCharacterItem>();
        //Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    //void Start() {
    //    Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
    //}
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

    public void Initialize() {
        //attack/raid
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetNeededType(typeof(Character));
            //currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetSlotIndex(i);
            //currSlot.SetItemDroppedCallback(OnDropOnAttackGrid);
            //currSlot.SetItemDroppedOutCallback(OnDroppedOutFromAttackGrid);
        }
        minionItems = new List<PlayerCharacterItem>();

        LoadRoleSlots();
        LoadAttackSlot();

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<InteractionIntel>(Signals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<InteractionIntel>(Signals.PLAYER_REMOVED_INTEL, OnIntelRemoved);
    }

    #region Role Slots
    private void LoadRoleSlots() {
        roleSlots = Utilities.GetComponentsInDirectChildren<RoleSlotItem>(roleSlotsParent);
        int currIndex = 0;
        foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in PlayerManager.Instance.player.roleSlots) {
            RoleSlotItem item = roleSlots.ElementAtOrDefault(currIndex);
            if (item != null) {
                item.SetSlotJob(keyValuePair.Key);
            } else {
                Debug.LogWarning("There is no slot item for job " + keyValuePair.Key.ToString());
            }
            currIndex++;
        }
    }

    #endregion

    #region Attack UI
    private void LoadAttackSlot() {
        attackGridReference = new CombatGrid();
        defenseGridReference = new CombatGrid();
        attackGridReference.Initialize();
        defenseGridReference.Initialize();
        attackSlot.UpdateVisuals();
        defenseSlot.UpdateVisuals();
    }
    public void ShowAttackGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnAttackGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromAttackGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = attackGridIconSprite;
        SetAttackGridCharactersFromPlayer();
    }
    public void ShowDefenseGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnDefenseGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromDefenseGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = defenseGridIconSprite;
        SetDefenseGridCharactersFromPlayer();
    }
    public void OnClickConfirmCombatGrid() {
        if(combatGridAssignerIcon.sprite == attackGridIconSprite) {
            attackSlot.OnClickConfirm();
        } else {
            defenseSlot.OnClickConfirm();
        }
    }
    public void HideCombatGrid() {
        attackGridGO.SetActive(false);
    }
    private void OnDropOnAttackGrid(object obj, int index) {
        if(obj is Character) {
            Character character = obj as Character;
            if (attackGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(attackGridReference.slots[index].character);
                return;
            }
            attackGridReference.AssignCharacterToGrid(character, index, true);
            UpdateAttackGridSlots();
        }
    }
    private void OnDropOnDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            if (defenseGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(defenseGridReference.slots[index].character);
                return;
            }
            defenseGridReference.AssignCharacterToGrid(character, index, true);
            UpdateDefenseGridSlots();
        }
    }
    private void OnDroppedOutFromAttackGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            attackGridReference.RemoveCharacterFromGrid(character);
            UpdateAttackGridSlots();
        }
    }
    private void OnDroppedOutFromDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            defenseGridReference.RemoveCharacterFromGrid(character);
            UpdateDefenseGridSlots();
        }
    }
    private bool IsObjectValidForAttack(object obj, SlotItem slotItem) {
        if (obj is Character) {
            Character character = obj as Character;
            if (character.characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                if (attackGridSlots[0] == slotItem || attackGridSlots[1] == slotItem) {
                    return true;
                }
            } else {
                if (attackGridSlots[2] == slotItem || attackGridSlots[3] == slotItem) {
                    return true;
                }
            }
        }
        return false;
    }
    private void SetAttackGridCharactersFromPlayer() {
        for (int i = 0; i < attackGridReference.slots.Length; i++) {
            attackGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void SetDefenseGridCharactersFromPlayer() {
        for (int i = 0; i < defenseGridReference.slots.Length; i++) {
            defenseGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateAttackGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateDefenseGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(defenseGridReference.slots[i].character);
        }
    }
    #endregion

    #region Miscellaneous
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
    public void SortByClassMinions() {
        _minionSortType = MINIONS_SORT_TYPE.TYPE;
        PlayerManager.Instance.player.SortByClass();
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
            SortByClassMinions();
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
    public void OnClickAssign() {
        UIManager.Instance.ShowDraggableObjectPicker(PlayerManager.Instance.player.allOwnedCharacters, new CharacterLevelComparer());
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
                UIManager.Instance.ShowCharacterTokenMenu();
            } else if (previousMenu.Equals("location")) {
                UIManager.Instance.ShowLocationTokenMenu();
            } else if (previousMenu.Equals("faction")) {
                UIManager.Instance.ShowFactionTokenMenu();
            }
        }
    }

    #region Intel
    private void OnIntelObtained(InteractionIntel intel) {
        UpdateIntel();
    }
    private void OnIntelRemoved(InteractionIntel intel) {
        UpdateIntel();
    }
    private void UpdateIntel() {
        for (int i = 0; i < intelItems.Length; i++) {
            InteractionIntelItem currItem = intelItems[i];
            InteractionIntel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
        }
    }
    public void SetIntelMenuState(bool state) {
        if (intelToggle.isOn == state) {
            return; //ignore change
        }
        intelToggle.isOn = state;
        if (!intelToggle.isOn) {
            OnCloseIntelMenu();
        }
    }
    private void OnCloseIntelMenu() {
        for (int i = 0; i < intelItems.Length; i++) {
            InteractionIntelItem currItem = intelItems[i];
            currItem.ClearClickActions();
        }
    }
    public void SetIntelItemClickActions(InteractionIntelItem.OnClickAction clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            InteractionIntelItem currItem = intelItems[i];
            currItem.SetClickAction(clickAction);
        }
    }
    public void AddIntelItemOtherClickActions(System.Action clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            InteractionIntelItem currItem = intelItems[i];
            currItem.AddOtherClickAction(clickAction);
        }
    }
    #endregion
}
