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
    public ScrollRect column1ScrollView;
    public ToggleGroup column1ToggleGroup;
    public TextMeshProUGUI column1TitleLbl;
    

    [Header("Column 2")]
    public ScrollRect column2ScrollView;
    public ToggleGroup column2ToggleGroup;
    public TextMeshProUGUI column2TitleLbl;
  

    private object _pickedObj1;
    private object _pickedObj2;
    private System.Action<object, object> onConfirmAction;
    private System.Action<object> onPickFirstObjAction;

    private bool needs2Objects; //does the current object picker require 2 objects to be picked? If not only an object in the first column is required to be selected.

    #region getters/setters
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
    #endregion

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
        Action<object, object> onClickConfirmAction,
        string confirmBtnStr, bool needs2Objects = true) {

        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }

        //destroy existing items
        Utilities.DestroyChildren(column1ScrollView.content);
        Utilities.DestroyChildren(column2ScrollView.content);

        //set titles
        column1TitleLbl.text = string.Empty;
        column2TitleLbl.text = string.Empty;

        //reset values
        pickedObj1 = null;
        pickedObj2 = null;

        onConfirmAction = onClickConfirmAction;
        confirmBtnLbl.text = confirmBtnStr;

        this.needs2Objects = needs2Objects;

        if (!needs2Objects) {
            column1ToggleGroup.allowSwitchOff = false;
            column2ToggleGroup.allowSwitchOff = true;
        } else {
            column1ToggleGroup.allowSwitchOff = false;
            column2ToggleGroup.allowSwitchOff = false;
        }

        //populate column 1
        PopulateColumn(column1Items, column1ValidityChecker, column1ItemHoverEnterAction, column1ItemHoverExitAction, column1ScrollView, column1ToggleGroup, column1Title);

        //populate column 2
        PopulateColumn(column2Items, column2ValidityChecker, column2ItemHoverEnterAction, column2ItemHoverExitAction, column2ScrollView, column2ToggleGroup, column2Title);

        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Show dual object picker but initially, only populate the first column.
    /// This is used for cases when the content of the second column is dependent upon the first chosen item.
    /// NOTE: onPickFirstObjAction is the action called once an object in the first column is picked.
    /// </summary>
    public void ShowDualObjectPicker<T>(List<T> column1Items,
        string column1Title,
        Func<T, bool> column1ValidityChecker, 
        Action<T> column1ItemHoverEnterAction, 
        Action<T> column1ItemHoverExitAction,
        Action<object> onPickFirstObjAction,
        Action<object, object> onClickConfirmAction,
        string confirmBtnStr) {

        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }

        //destroy existing items
        Utilities.DestroyChildren(column1ScrollView.content);
        Utilities.DestroyChildren(column2ScrollView.content);

        //set titles
        column1TitleLbl.text = string.Empty;

        //reset values
        pickedObj1 = null;
        pickedObj2 = null;

        onConfirmAction = onClickConfirmAction;
        confirmBtnLbl.text = confirmBtnStr;

        this.onPickFirstObjAction = onPickFirstObjAction;

        //populate column 1
        PopulateColumn(column1Items, column1ValidityChecker, column1ItemHoverEnterAction, column1ItemHoverExitAction, column1ScrollView, column1ToggleGroup, column1Title);

        this.gameObject.SetActive(true);
    }
    public void Hide() {
        UIManager.Instance.ResumeLastProgressionSpeed();
        this.gameObject.SetActive(false);
    }

    public void OnClickConfirm() {
        onConfirmAction?.Invoke(pickedObj1, pickedObj2);
        Hide();
    }

    public void PopulateColumn<T>(List<T> items, Func<T, bool> validityChecker, Action<T> hoverEnterAction, Action<T> hoverExitAction, ScrollRect column, ToggleGroup toggleGroup, string columnTitle) {
        Utilities.DestroyChildren(column.content);
        List<T> validItems;
        List<T> invalidItems;

        if (column == column1ScrollView) {
            column1TitleLbl.text = columnTitle;
        } else if (column == column2ScrollView) {
            column2TitleLbl.text = columnTitle;
        }

        OrganizeList(items, out validItems, out invalidItems, validityChecker);
        if (typeof(T) == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        } else if (typeof(T) == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        } else if (typeof(T) == typeof(UnsummonedMinionData)) {
            ShowUnsummonedMinionItems(validItems.Cast<UnsummonedMinionData>().ToList(), invalidItems.Cast<UnsummonedMinionData>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        }
    }

    #region Instantiators
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
            characterItem.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            characterItem.SetToggleGroup(toggleGroup);
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
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
        Action<string> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToString(onHoverItemAction);
        }
        Action<string> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToString(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            string currStr = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.stringNameplatePrefab.name, column.content);
            StringNameplateItem item = characterItemGO.GetComponent<StringNameplateItem>();
            item.SetObject(currStr);
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
            item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            item.SetToggleGroup(toggleGroup);
            item.SetInteractableState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            string currStr = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.stringNameplatePrefab.name, column.content);
            StringNameplateItem characterItem = characterItemGO.GetComponent<StringNameplateItem>();
            characterItem.SetObject(currStr);
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
    private void ShowUnsummonedMinionItems<T>(List<UnsummonedMinionData> validItems, List<UnsummonedMinionData> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
        Action<UnsummonedMinionData> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToUnsummonedMinion(onHoverItemAction);
        }
        Action<UnsummonedMinionData> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToUnsummonedMinion(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            UnsummonedMinionData minion = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.unsummonedMinionNameplatePrefab.name, column.content);
            UnsummonedMinionNameplateItem item = characterItemGO.GetComponent<UnsummonedMinionNameplateItem>();
            item.SetObject(minion);
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
            item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            item.SetToggleGroup(toggleGroup);
            item.SetInteractableState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            UnsummonedMinionData minion = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.stringNameplatePrefab.name, column.content);
            UnsummonedMinionNameplateItem characterItem = characterItemGO.GetComponent<UnsummonedMinionNameplateItem>();
            characterItem.SetObject(minion);
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
    #endregion

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
    private void OnToggleItem(object obj, bool isOn, ScrollRect column) {
        if (isOn) {
            if (column == column1ScrollView) {
                pickedObj1 = obj;
                if (onPickFirstObjAction != null) {
                    onPickFirstObjAction.Invoke(pickedObj1);
                }
            } else {
                pickedObj2 = obj;
            }
        } else {
            if (column == column1ScrollView) {
                pickedObj1 = null;
            } else {
                pickedObj2 = null;
            }
        }
    }
    private void UpdateConfirmBtnState() {
        if (needs2Objects) {
            confirmBtn.interactable = pickedObj1 != null && pickedObj2 != null;
        } else {
            confirmBtn.interactable = pickedObj1 != null;
        }
        
    }
    #endregion

    #region Converters
    private Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T)(object)o));
    }
    public Action<UnsummonedMinionData> ConvertToUnsummonedMinion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<UnsummonedMinionData>(o => myActionT((T)(object)o));
    }
    #endregion
}
