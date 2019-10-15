using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DualObjectPicker : MonoBehaviour {

    [Header("Main")]
    [SerializeField] private Button confirmBtn;
    [SerializeField] private TextMeshProUGUI confirmBtnLbl;

    [Header("Column 1")]
    [SerializeField] private TextMeshProUGUI column1TitleLbl;
    [SerializeField] private ScrollRect column1ScrollView;
    [SerializeField] private ToggleGroup column1ToggleGroup;

    [Header("Column 2")]
    [SerializeField] private TextMeshProUGUI column2TitleLbl;
    [SerializeField] private ScrollRect column2ScrollView;
    [SerializeField] private ToggleGroup column2ToggleGroup;


    private object _pickedObj1;
    private object _pickedObj2;

    private object pickedObj1 {
        get { return _pickedObj1; }
        set {
            _pickedObj1 = value;
            UpdateConfirmBtnState();
        }
    }

    private object pickedObj2 {
        get { return _pickedObj2; }
        set {
            _pickedObj2 = value;
            UpdateConfirmBtnState();
        }
    }


    /// <summary>
    /// Show the dual object picker.
    /// </summary>
    /// <typeparam name="T">The type of objects to be shown in column 1.</typeparam>
    /// <typeparam name="U">The type of objects to be shown in column 2.</typeparam>
    /// <param name="column1Items">The list of items to be shown on column 1.</param>
    /// <param name="column2Items">The list of items to be shown on column 2.</param>
    /// <param name="column1Title">The title of column 1.</param>
    /// <param name="column2Title">The title of column 2.</param>
    /// <param name="column1ValidityChecker">Function to check if an item in column 1 should be pickable.</param>
    /// <param name="column2ValidityChecker">Function to check if an item in column 2 should be pickable.</param>
    /// <param name="onClickConfirmAction">Function to execute when the player has picked 2 items from the 2 lists.</param>
    public void ShowDualObjectPicker<T, U>(List<T> column1Items, List<U> column2Items,
        string column1Title, string column2Title,
        Func<T, bool> column1ValidityChecker, Func<U, bool> column2ValidityChecker,
        Action<T> column1ItemHoverEnterAction, Action<U> column2ItemHoverEnterAction,
        Action<T> column1ItemHoverExitAction, Action<U> column2ItemHoverExitAction,
        Action<T, U> onClickConfirmActio) {

        //destroy existing items
        Utilities.DestroyChildren(column1ScrollView.content);
        Utilities.DestroyChildren(column2ScrollView.content);

        //set titles
        column1TitleLbl.text = column1Title;
        column2TitleLbl.text = column2Title;

        //reset values
        _pickedObj1 = null;
        _pickedObj2 = null;

        //populate column 1
        PopulateColumn(column1Items, column1ValidityChecker, column1ItemHoverEnterAction, column1ItemHoverExitAction, column1ScrollView, column1ToggleGroup);

        //populate column 2
        PopulateColumn(column2Items, column2ValidityChecker, column2ItemHoverEnterAction, column2ItemHoverExitAction, column2ScrollView, column2ToggleGroup);
    }

    private void PopulateColumn<T>(List<T> items, Func<T, bool> validityChecker, Action<T> hoverEnterAction, Action<T> hoverExitAction, ScrollRect column, ToggleGroup toggleGroup) {
        List<T> validItems;
        List<T> invalidItems;
        OrganizeList(items, out validItems, out invalidItems, validityChecker);
        if (typeof(T) == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        }
    }


    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
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
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.characterNameplatePrefab.name, column.content);
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
            characterItem.SetAsToggle();
            characterItem.SetToggleGroup(toggleGroup);
            characterItem.AddOnToggleAction((character, isOn) => OnPickItem(character, isOn, column));
            characterItem.SetInteractableState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.characterNameplatePrefab.name, column.content);
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
            characterItem.SetInteractableState(false);
        }
    }

    #region Utlities
    private void OrganizeList<T>(List<T> items, out List<T> validItems, out List<T> invalidItems, Func<T, bool> validityChecker) {
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
    }
    private void OnPickItem(object obj, bool isOn, ScrollRect column) {
        if (isOn) {
            if (column == column1ScrollView) {
                pickedObj1 = obj;
            } else {
                pickedObj2 = obj;
            }
        }
    }
    private void UpdateConfirmBtnState() {
        confirmBtn.interactable = pickedObj1 != null && pickedObj2 != null;
    }
    #endregion

    #region Converters
    private Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    #endregion
}
