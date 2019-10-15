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

    private bool _isGamePausedBeforeOpeningPicker;

    public void ShowClickable<T>(List<T> items, Action<T> onClickItemAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, 
        string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "", bool showCover = false, int layer = 9, bool closable = true) {
        Utilities.DestroyChildren(objectPickerScrollView.content);
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction, identifier);
        } else if (type == typeof(Area)) {
            ShowAreaItems(validItems.Cast<Area>().ToList(), invalidItems.Cast<Area>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction);
        } else if (type == typeof(Region)) {
            ShowRegionItems(validItems.Cast<Region>().ToList(), invalidItems.Cast<Region>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction);
        } else if (type == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction, identifier);
        } else if (type == typeof(SummonSlot)) {
            ShowSummonItems(validItems.Cast<SummonSlot>().ToList(), invalidItems.Cast<SummonSlot>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction, identifier);
        } else if (type == typeof(ArtifactSlot)) {
            ShowArtifactSlotItems(validItems.Cast<ArtifactSlot>().ToList(), invalidItems.Cast<ArtifactSlot>().ToList(), onClickItemAction, onHoverItemAction, onHoverExitItemAction, identifier);
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
    //public void ShowDraggable<T>(List<T> items, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "") {
    //    Utilities.DestroyChildren(objectPickerScrollView.content);
    //    List<T> validItems;
    //    List<T> invalidItems;
    //    OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
    //    Type type = typeof(T);
    //    if (type == typeof(Character)) {
    //        ShowDraggableCharacterItems<T>(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList());
    //    }
    //    titleLbl.text = title;
    //    if (!gameObject.activeSelf) {
    //        this.gameObject.SetActive(true);
    //        _isGamePausedBeforeOpeningPicker = GameManager.Instance.isPaused;
    //        GameManager.Instance.SetPausedState(true);
    //        UIManager.Instance.SetSpeedTogglesState(false);
    //    }
    //}
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

    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<Character> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = Convert(onClickItemAction);
        }
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
            if (convertedAction != null) {
                characterItem.AddOnClickAction(convertedAction.Invoke);
            }

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }
            
            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            characterItem.SetAsButton();
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
            characterItem.SetAsButton();
            characterItem.SetInteractableState(false);
        }
    }
    //private void ShowDraggableCharacterItems<T>(List<Character> validItems, List<Character> invalidItems) {
    //    for (int i = 0; i < validItems.Count; i++) {
    //        Character currCharacter = validItems[i];
    //        GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAttackItemPrefab.name, objectPickerScrollView.content);
    //        AttackPickerItem characterItem = characterItemGO.GetComponent<AttackPickerItem>();
    //        characterItem.SetCharacter(currCharacter);
    //        characterItem.SetDraggableState(true);
    //    }
    //    for (int i = 0; i < invalidItems.Count; i++) {
    //        Character currCharacter = invalidItems[i];
    //        GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAttackItemPrefab.name, objectPickerScrollView.content);
    //        AttackPickerItem characterItem = characterItemGO.GetComponent<AttackPickerItem>();
    //        characterItem.SetCharacter(currCharacter);
    //        characterItem.SetDraggableState(false);
    //    }
    //}
    private void ShowAreaItems<T>(List<Area> validItems, List<Area> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction) {
        Action<Area> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToArea(onClickItemAction);
        }   
        Action<Area> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToArea(onHoverItemAction);
        }
        Action<Area> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToArea(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Area currArea = validItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
            AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
            areaItem.SetArea(currArea);
            areaItem.onClickAction = convertedAction;
            areaItem.onHoverEnterAction = convertedHoverAction;
            areaItem.onHoverExitAction = convertedHoverExitAction;
            areaItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Area currArea = invalidItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
            AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
            areaItem.SetArea(currArea);
            areaItem.onClickAction = null;
            areaItem.onHoverEnterAction = convertedHoverAction;
            areaItem.onHoverExitAction = convertedHoverExitAction;
            areaItem.SetButtonState(false);
        }
    }
    private void ShowRegionItems<T>(List<Region> validItems, List<Region> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction) {
        Action<Region> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToRegion(onClickItemAction);
        }
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
            RegionPickerItem regionItem = areaItemGO.GetComponent<RegionPickerItem>();
            regionItem.SetRegion(currRegion);
            regionItem.onClickAction = convertedAction;
            regionItem.onHoverEnterAction = convertedHoverAction;
            regionItem.onHoverExitAction = convertedHoverExitAction;
            regionItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Region currRegion = invalidItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content);
            RegionPickerItem regionItem = areaItemGO.GetComponent<RegionPickerItem>();
            regionItem.SetRegion(currRegion);
            regionItem.onClickAction = null;
            regionItem.onHoverEnterAction = convertedHoverAction;
            regionItem.onHoverExitAction = convertedHoverExitAction;
            regionItem.SetButtonState(false);
        }
    }
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<string> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToString(onClickItemAction);
        }
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
            if (convertedAction != null) {
                stringItem.AddOnClickAction(convertedAction.Invoke);
            }

            stringItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                stringItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            stringItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                stringItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            stringItem.SetAsButton();
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
            stringItem.SetAsButton();
            stringItem.SetInteractableState(false);
        }
    }
    private void ShowSummonItems<T>(List<SummonSlot> validItems, List<SummonSlot> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<SummonSlot> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToSummonSlot(onClickItemAction);
        }
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
            SummonSlotPickerItem characterItem = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            characterItem.SetSummonSlot(currSummonSlot);
            characterItem.onClickAction = convertedAction;
            characterItem.onHoverEnterAction = convertedHoverAction;
            characterItem.onHoverExitAction = convertedHoverExitAction;
            characterItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            SummonSlot currSummonSlot = invalidItems[i];
            GameObject summonSlotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerSummonSlotItemPrefab.name, objectPickerScrollView.content);
            SummonSlotPickerItem summonSlotItem = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            summonSlotItem.SetSummonSlot(currSummonSlot);
            summonSlotItem.onClickAction = null;
            summonSlotItem.onHoverEnterAction = convertedHoverAction;
            summonSlotItem.onHoverExitAction = convertedHoverExitAction;
            summonSlotItem.SetButtonState(false);
        }
    }
    private void ShowArtifactSlotItems<T>(List<ArtifactSlot> validItems, List<ArtifactSlot> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier) {
        Action<ArtifactSlot> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToArtifactSlot(onClickItemAction);
        }
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
            item.SetArtifactSlot(currSlot);
            item.onClickAction = convertedAction;
            item.onHoverEnterAction = convertedHoverAction;
            item.onHoverExitAction = convertedHoverExitAction;
            item.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            ArtifactSlot currSlot = invalidItems[i];
            GameObject slotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerArtifactSlotItemPrefab.name, objectPickerScrollView.content);
            ArtifactSlotPickerItem slotItem = slotItemGO.GetComponent<ArtifactSlotPickerItem>();
            slotItem.SetArtifactSlot(currSlot);
            slotItem.onClickAction = null;
            slotItem.onHoverEnterAction = convertedHoverAction;
            slotItem.onHoverExitAction = convertedHoverExitAction;
            slotItem.SetButtonState(false);
        }
    }
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
        else return new Action<Minion>(o => myActionT((T) (object) o));
    }
    public Action<Area> ConvertToArea<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Area>(o => myActionT((T) (object) o));
    }
    public Action<Region> ConvertToRegion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Region>(o => myActionT((T) (object) o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T) (object) o));
    }
}


