using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPicker : MonoBehaviour {

    [Header("Object Picker")]
    [SerializeField] private ScrollRect objectPickerScrollView;
    [SerializeField] private GameObject objectPickerCharacterItemPrefab;
    [SerializeField] private GameObject objectPickerAttackItemPrefab;

    public void ShowClickable<T>(List<T> items, Action<T> onClickItemAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null) {
        Utilities.DestroyChildren(objectPickerScrollView.content);
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onClickItemAction);
        }
        this.gameObject.SetActive(true);
    }
    public void ShowDraggable<T>(List<T> items, IComparer<T> comparer = null, Func<T, bool> validityChecker = null) {
        Utilities.DestroyChildren(objectPickerScrollView.content);
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowDraggableCharacterItems<T>(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList());
        }
        this.gameObject.SetActive(true);
    }
    public void Hide() {
        this.gameObject.SetActive(false);
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

    public Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
}


