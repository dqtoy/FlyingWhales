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
    [SerializeField] private GameObject objectPickerStringItemPrefab;
    [SerializeField] private GameObject objectPickerAttackItemPrefab;
    [SerializeField] private TextMeshProUGUI titleLbl;

    private bool _isGamePausedBeforeOpeningPicker;

    public void ShowClickable<T>(List<T> items, Action<T> onClickItemAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverItemAction = null) {
        Utilities.DestroyChildren(objectPickerScrollView.content);
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onClickItemAction);
        } else if (type == typeof(Area)) {
            ShowAreaItems(validItems.Cast<Area>().ToList(), invalidItems.Cast<Area>().ToList(), onClickItemAction, onHoverItemAction);
        } else if (type == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), onClickItemAction, onHoverItemAction);
        }
        titleLbl.text = title;
        if (!gameObject.activeSelf) {
            this.gameObject.SetActive(true);
            _isGamePausedBeforeOpeningPicker = GameManager.Instance.isPaused;
            GameManager.Instance.SetPausedState(true);
        }
    }
    public void ShowDraggable<T>(List<T> items, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "") {
        Utilities.DestroyChildren(objectPickerScrollView.content);
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowDraggableCharacterItems<T>(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList());
        }
        titleLbl.text = title;
        if (!gameObject.activeSelf) {
            this.gameObject.SetActive(true);
            _isGamePausedBeforeOpeningPicker = GameManager.Instance.isPaused;
            GameManager.Instance.SetPausedState(true);
        }
    }
    public void Hide() {
        if (gameObject.activeSelf) {
            this.gameObject.SetActive(false);
            GameManager.Instance.SetPausedState(_isGamePausedBeforeOpeningPicker);
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

    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onClickItemAction) {
        Action<Character> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = Convert(onClickItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Character currCharacter = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterPickerItem characterItem = characterItemGO.GetComponent<CharacterPickerItem>();
            characterItem.SetCharacter(currCharacter);
            characterItem.onClickAction = convertedAction;
            characterItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterPickerItem characterItem = characterItemGO.GetComponent<CharacterPickerItem>();
            characterItem.SetCharacter(currCharacter);
            characterItem.SetButtonState(false);
        }
    }
    private void ShowDraggableCharacterItems<T>(List<Character> validItems, List<Character> invalidItems) {
        for (int i = 0; i < validItems.Count; i++) {
            Character currCharacter = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAttackItemPrefab.name, objectPickerScrollView.content);
            AttackPickerItem characterItem = characterItemGO.GetComponent<AttackPickerItem>();
            characterItem.SetCharacter(currCharacter);
            characterItem.SetDraggableState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAttackItemPrefab.name, objectPickerScrollView.content);
            AttackPickerItem characterItem = characterItemGO.GetComponent<AttackPickerItem>();
            characterItem.SetCharacter(currCharacter);
            characterItem.SetDraggableState(false);
        }
    }
    private void ShowAreaItems<T>(List<Area> validItems, List<Area> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction) {
        Action<Area> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToArea(onClickItemAction);
        }   
        Action<Area> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToArea(onHoverItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Area currArea = validItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
            AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
            areaItem.SetArea(currArea);
            areaItem.onClickAction = convertedAction;
            areaItem.onHoverEnterAction = convertedHoverAction;
            areaItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Area currArea = invalidItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
            AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
            areaItem.SetArea(currArea);
            areaItem.SetButtonState(false);
        }
    }
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onClickItemAction, Action<T> onHoverItemAction) {
        Action<string> convertedAction = null;
        if (onClickItemAction != null) {
            convertedAction = ConvertToString(onClickItemAction);
        }
        Action<string> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToString(onHoverItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            string currString = validItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringPickerItem stringItem = stringItemGO.GetComponent<StringPickerItem>();
            stringItem.SetString(currString);
            stringItem.onClickAction = convertedAction;
            stringItem.onHoverEnterAction = convertedHoverAction;
            stringItem.SetButtonState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            string currString = invalidItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringPickerItem stringItem = stringItemGO.GetComponent<StringPickerItem>();
            stringItem.SetString(currString);
            stringItem.onHoverEnterAction = convertedHoverAction;
            stringItem.SetButtonState(false);
        }
    }
    public Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    public Action<Area> ConvertToArea<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Area>(o => myActionT((T) (object) o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T) (object) o));
    }
}


