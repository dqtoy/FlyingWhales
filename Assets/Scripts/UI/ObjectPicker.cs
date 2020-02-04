using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPicker : MonoBehaviour {

    [Header("Object Picker")]
    [SerializeField] private ScrollRect objectPickerScrollView;
    [SerializeField] private GameObject objectPickerCharacterItemPrefab;
    [SerializeField] private GameObject objectPickerAreaItemPrefab;
    [SerializeField] private GameObject objectPickerRegionItemPrefab;
    [SerializeField] private GameObject objectPickerStringItemPrefab;
    [SerializeField] private GameObject objectPickerAttackItemPrefab;
    [SerializeField] private GameObject objectPickerSummonSlotItemPrefab;
    [SerializeField] private GameObject objectPickerArtifactSlotItemPrefab;
    [SerializeField] private TextMeshProUGUI titleLbl;
    [SerializeField] private GameObject cover;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("Misc")]
    [SerializeField] private UIHoverPosition minionCardPos;

    private bool _isGamePausedBeforeOpeningPicker;

    private object _pickedObj;
    private System.Action<object> onConfirmAction;

    public object pickedObj {
        get { return _pickedObj; }
        set { 
            _pickedObj = value;
            UpdateConfirmBtnState();
        }
    }

    public void ShowClickable<T>(List<T> items, Action<object> onConfirmAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, 
        string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "", bool showCover = false, int layer = 9, bool closable = true) {
        Ruinarch.Utilities.DestroyChildren(objectPickerScrollView.content);

        pickedObj = null;
        this.onConfirmAction = onConfirmAction;

        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier);
        }
        //else if (type == typeof(Settlement)) {
        //    ShowAreaItems(validItems.Cast<Settlement>().ToList(), invalidItems.Cast<Settlement>().ToList(), onHoverItemAction, onHoverExitItemAction);
        //} 
        else if (type == typeof(Region)) {
            ShowRegionItems(validItems.Cast<Region>().ToList(), invalidItems.Cast<Region>().ToList(), onHoverItemAction, onHoverExitItemAction);
        } else if (type == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier);
        } else if (type == typeof(SummonSlot)) {
            ShowSummonItems(validItems.Cast<SummonSlot>().ToList(), invalidItems.Cast<SummonSlot>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier);
        } else if (type == typeof(ArtifactSlot)) {
            ShowArtifactSlotItems(validItems.Cast<ArtifactSlot>().ToList(), invalidItems.Cast<ArtifactSlot>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier);
        }
        titleLbl.text = title;
        if (!gameObject.activeSelf) {
            this.gameObject.SetActive(true);
            _isGamePausedBeforeOpeningPicker = GameManager.Instance.isPaused;
            GameManager.Instance.SetPausedState(true);
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        cover.SetActive(showCover);
        this.gameObject.transform.SetSiblingIndex(layer);
        closeBtn.interactable = closable;
    }
    public void Hide() {
        if (gameObject.activeSelf) {
            this.gameObject.SetActive(false);
            GameManager.Instance.SetPausedState(_isGamePausedBeforeOpeningPicker);
            UIManager.Instance.SetSpeedTogglesState(true);
        }
    }

    private void OrganizeList<T>(List<T> items, out List<T> validItems, out List<T> invalidItems, IComparer<T> comparer = null, Func<T, bool> validityChecker = null) {
        validItems = new List<T>();
        invalidItems = new List<T>();
        if (validityChecker != null) {
            for (int i = 0; i < items.Count; i++) {
                T currItem = items[i];
                if (validityChecker(currItem)) {
                    validItems.Add(currItem);
                } else {
                    invalidItems.Add(currItem);
                }
            }
        } else {
            validItems.AddRange(items);
        }

        if (comparer != null) {
            validItems.Sort(comparer);
            invalidItems.Sort(comparer);
        }
    }

    #region Instantiators
    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<Character> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = Convert(onHoverItemAction);
        }
        Action<Character> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = Convert(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Character currCharacter = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterNameplateItem characterItem = characterItemGO.GetComponent<CharacterNameplateItem>();
            characterItem.SetObject(currCharacter);
            characterItem.ClearAllOnClickActions();
            characterItem.AddOnToggleAction(OnPickObject);

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            //specific case for minion
            if (currCharacter.minion != null) {
                characterItem.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(currCharacter.minion, minionCardPos));
                characterItem.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
            }
            characterItem.SetAsToggle();
            characterItem.SetToggleGroup(toggleGroup);
            characterItem.SetPortraitInteractableState(false);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterNameplateItem characterItem = characterItemGO.GetComponent<CharacterNameplateItem>();
            characterItem.SetObject(currCharacter);
            characterItem.ClearAllOnClickActions();

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            //specific case for minion
            if (currCharacter.minion != null) {
                characterItem.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(currCharacter.minion, minionCardPos));
                characterItem.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
            }
            characterItem.SetAsToggle();
            characterItem.SetInteractableState(false);
            characterItem.SetPortraitInteractableState(true);
        }
    }
    //private void ShowAreaItems<T>(List<Settlement> validItems, List<Settlement> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction) {
    //    Action<Settlement> convertedHoverAction = null;
    //    if (onHoverItemAction != null) {
    //        convertedHoverAction = ConvertToArea(onHoverItemAction);
    //    }
    //    Action<Settlement> convertedHoverExitAction = null;
    //    if (onHoverExitItemAction != null) {
    //        convertedHoverExitAction = ConvertToArea(onHoverExitItemAction);
    //    }
    //    for (int i = 0; i < validItems.Count; i++) {
    //        Settlement currSettlement = validItems[i];
    //        GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
    //        AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
    //        areaItem.SetArea(currSettlement);
    //        areaItem.onClickAction = convertedAction;
    //        areaItem.onHoverEnterAction = convertedHoverAction;
    //        areaItem.onHoverExitAction = convertedHoverExitAction;
    //        areaItem.SetButtonState(true);
    //    }
    //    for (int i = 0; i < invalidItems.Count; i++) {
    //        Settlement currSettlement = invalidItems[i];
    //        GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
    //        AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
    //        areaItem.SetArea(currSettlement);
    //        areaItem.onClickAction = null;
    //        areaItem.onHoverEnterAction = convertedHoverAction;
    //        areaItem.onHoverExitAction = convertedHoverExitAction;
    //        areaItem.SetButtonState(false);
    //    }
    //}
    private void ShowRegionItems<T>(List<Region> validItems, List<Region> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction) {
        Action<Region> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToRegion(onHoverItemAction);
        }
        Action<Region> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToRegion(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Region currRegion = validItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content);
            RegionNameplateItem item = areaItemGO.GetComponent<RegionNameplateItem>();
            item.SetObject(currRegion);
            item.ClearAllOnClickActions();
            item.AddOnToggleAction(OnPickObject);

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetAsToggle();
            item.SetToggleGroup(toggleGroup);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Region currRegion = invalidItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content);
            RegionNameplateItem item = areaItemGO.GetComponent<RegionNameplateItem>();
            item.SetObject(currRegion);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetAsToggle();
            item.SetInteractableState(false);
        }
    }
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<string> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToString(onHoverItemAction);
        }
        Action<string> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToString(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            string currString = validItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringNameplateItem stringItem = stringItemGO.GetComponent<StringNameplateItem>();
            stringItem.SetObject(currString);
            stringItem.SetIdentifier(identifier);
            stringItem.ClearAllOnClickActions();
            stringItem.AddOnToggleAction(OnPickObject);

            stringItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                stringItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            stringItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                stringItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            stringItem.SetAsToggle();
            stringItem.SetToggleGroup(toggleGroup);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            string currString = invalidItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringNameplateItem stringItem = stringItemGO.GetComponent<StringNameplateItem>();
            stringItem.SetObject(currString);
            stringItem.SetIdentifier(identifier);
            stringItem.ClearAllOnClickActions();

            stringItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                stringItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            stringItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                stringItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            stringItem.SetAsToggle();
            stringItem.SetInteractableState(false);
        }
    }
    private void ShowSummonItems<T>(List<SummonSlot> validItems, List<SummonSlot> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<SummonSlot> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToSummonSlot(onHoverItemAction);
        }
        Action<SummonSlot> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToSummonSlot(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            SummonSlot currSummonSlot = validItems[i];
            GameObject summonSlotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerSummonSlotItemPrefab.name, objectPickerScrollView.content);
            SummonSlotPickerItem item = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            item.SetObject(currSummonSlot);

            item.ClearAllOnClickActions();
            item.AddOnToggleAction(OnPickObject);

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }

            item.SetAsToggle();
            item.SetToggleGroup(toggleGroup);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            SummonSlot currSummonSlot = invalidItems[i];
            GameObject summonSlotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerSummonSlotItemPrefab.name, objectPickerScrollView.content);
            SummonSlotPickerItem item = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            item.SetObject(currSummonSlot);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }

            item.SetAsToggle();
            item.SetInteractableState(false);
        }
    }
    private void ShowArtifactSlotItems<T>(List<ArtifactSlot> validItems, List<ArtifactSlot> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<ArtifactSlot> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToArtifactSlot(onHoverItemAction);
        }
        Action<ArtifactSlot> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToArtifactSlot(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            ArtifactSlot currSlot = validItems[i];
            GameObject slotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerArtifactSlotItemPrefab.name, objectPickerScrollView.content);
            ArtifactSlotPickerItem item = slotItemGO.GetComponent<ArtifactSlotPickerItem>();
            item.SetObject(currSlot);

            item.ClearAllOnClickActions();
            item.AddOnToggleAction(OnPickObject);

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }

            item.SetAsToggle();
            item.SetToggleGroup(toggleGroup);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            ArtifactSlot currSlot = invalidItems[i];
            GameObject slotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerArtifactSlotItemPrefab.name, objectPickerScrollView.content);
            ArtifactSlotPickerItem item = slotItemGO.GetComponent<ArtifactSlotPickerItem>();
            item.SetObject(currSlot);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }

            item.SetAsToggle();
            item.SetInteractableState(false);
        }
    }
    #endregion

    #region Utilities
    private void UpdateConfirmBtnState() {
        confirmBtn.interactable = pickedObj != null;
    }
    public void OnPickObject(object obj, bool isOn) {
        if (isOn) {
            pickedObj = obj;
        } else {
            if (pickedObj == obj) {
                pickedObj = null;
            }
        }
    }
    public void OnClickConfirm() {
        onConfirmAction.Invoke(pickedObj);
    }
    #endregion


    #region Converters
    public Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    public Action<SummonSlot> ConvertToSummonSlot<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<SummonSlot>(o => myActionT((T)(object)o));
    }
    public Action<ArtifactSlot> ConvertToArtifactSlot<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<ArtifactSlot>(o => myActionT((T)(object)o));
    }
    public Action<Minion> ConvertToMinion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Minion>(o => myActionT((T)(object)o));
    }
    public Action<Settlement> ConvertToArea<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Settlement>(o => myActionT((T)(object)o));
    }
    public Action<Region> ConvertToRegion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Region>(o => myActionT((T)(object)o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T)(object)o));
    }
    #endregion
}


