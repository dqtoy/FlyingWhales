using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Linq;
using System;
using UnityEngine.Events;

public class InteractionItem : MonoBehaviour {
    private Interaction _interaction;
    private ActionOption _currentSelectedActionOption;
    private Toggle _toggle;

    //public CharacterPortrait portrait;
    public TextMeshProUGUI descriptionText;
    public ActionOptionButton[] actionOptionButtons;
    public Button confirmBtn;
    public RectTransform confirmBtnRect;

    [SerializeField] private GameObject slotItemPrefab;
    [SerializeField] private Vector3 confirmBtnPosNoSlot;
    [SerializeField] private Vector3 confirmBtnPosWithSlot;
    [SerializeField] private SlotItem[] slotItems;

    private List<CustomDropZone> neededObjectSlots;

    private bool _isToggled;

    private int currentNeededObjectIndex;
    private object lastAssignedObject;

    #region getters/setters
    public Interaction interaction {
        get { return _interaction; }
    }
    #endregion

    public void Initialize() {
        for (int i = 0; i < slotItems.Length; i++) {
            SlotItem currItem = slotItems[i];
            currItem.SetSlotIndex(i);
            currItem.itemDroppedCallback = new ItemDroppedCallback();
            currItem.itemDroppedCallback.AddListener(OnItemDroppedInSlot);
        }
    }

    private void Start() {
        neededObjectSlots = new List<CustomDropZone>();
        Messenger.AddListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.AddListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.RemoveListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
        GameObject.Destroy(_toggle.gameObject);
    }
    private void OnUpdatedInteractionState(Interaction interaction) {
        if(_interaction != null && _interaction == interaction) {
            UpdateState();
        }
    }
    private void OnChangedActivatedState(Interaction interaction) {
        if (_interaction != null && _interaction == interaction) {
            ChangedActivatedState();
        }
    }
    public void SetInteraction(Interaction interaction) {
        _interaction = interaction;
        if(_interaction != null) {
            _interaction.SetInteractionItem(this);
            InitializeActionButtons();
            UpdateState();
            ChangedActivatedState();
        }
    }
    private void InitializeActionButtons() {
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            actionOptionButtons[i].Initialize();
        }
    }
    public void UpdateState() {
        SetDescription(_interaction.currentState.description);
        confirmBtn.gameObject.SetActive(false);
        for (int i = 0; i < slotItems.Length; i++) {
            slotItems[i].gameObject.SetActive(false);
            slotItems[i].ClearSlot();
        }
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            if (_interaction.currentState.actionOptions[i] != null) {
                actionOptionButtons[i].SetOption(_interaction.currentState.actionOptions[i]);
                actionOptionButtons[i].toggle.isOn = false;
                actionOptionButtons[i].gameObject.SetActive(true);
            } else {
                actionOptionButtons[i].gameObject.SetActive(false);
            }
        }
        if (_interaction.isActivated && !_interaction.currentState.isEnd) {
            //this is to load any objects already assigned to the option
            //all needed slots will always be occupied since the interaction has already been activated
            //meaning that all needed objects were already assigned beforehand.
            confirmBtn.gameObject.SetActive(true);
            confirmBtn.interactable = false ;
            ActionOption actionOption = _interaction.currentState.chosenOption;
            if (actionOption != null) {
                for (int i = 0; i < actionOption.assignedObjects.Count; i++) {
                    object assignedObject = actionOption.assignedObjects[i];
                    SlotItem currItem = slotItems.ElementAtOrDefault(i);
                    if (currItem == null) {
                        Debug.LogWarning("Not enough slots for an option!");
                        break; //no more slots
                    } else {
                        currItem.gameObject.SetActive(true);
                        currItem.PlaceObject(assignedObject);
                    }
                    
                }
                //if (actionOption.assignedMinion != null) {
                //    portrait.GeneratePortrait(actionOption.assignedMinion.icharacter.portraitSettings, 95, true);
                //} else {
                //    portrait.GeneratePortrait(null, 95, true);
                //}
            }
        } else {
            if (_interaction.currentState.isEnd) {
                confirmBtn.gameObject.SetActive(true);
            }
            //portrait.GeneratePortrait(null, 95, true);
        }
    }
    public void SetDescription(string text) {
        descriptionText.text = text;
    }
    private void ChangedActivatedState() {
        ChangeStateAllButtons(!_interaction.isActivated);
        if (_interaction.isActivated) {
            _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveUnselected;
            _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveSelected;
        } else {
            _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveUnselected;
            _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveSelected;
        }
    }
    public void SetCurrentSelectedActionOption(ActionOption actionOption) {
        if (_currentSelectedActionOption != null) {
            _currentSelectedActionOption.ClearAssignedObjects();
        }
        _currentSelectedActionOption = actionOption;
        currentNeededObjectIndex = 0; //reset index
        confirmBtn.gameObject.SetActive(true);
        if (actionOption.neededObjects == null || actionOption.neededObjects.Count == 0) {
            //no needed slots
            confirmBtnRect.anchoredPosition = confirmBtnPosNoSlot;
            for (int i = 0; i < slotItems.Length; i++) {
                slotItems[i].gameObject.SetActive(false);
            }
            confirmBtn.interactable = true;
        } else {
            //needs slots
            confirmBtnRect.anchoredPosition = confirmBtnPosWithSlot;
            slotItems[0].gameObject.SetActive(true);
            for (int i = 1; i < slotItems.Length; i++) {
                slotItems[i].gameObject.SetActive(false);
            }
            LoadSlotData();
            confirmBtn.interactable = false;
            UpdateSideMenu();
        }
    }
    private void LoadSlotData() {
        for (int i = 0; i < slotItems.Length; i++) {
            SlotItem currItem = slotItems[i];
            System.Type neededType = _currentSelectedActionOption.neededObjects.ElementAtOrDefault(i);
            currItem.ClearSlot();
            currItem.SetNeededType(neededType);
        }
    }

    public void OnClickConfirm() {
        if (_currentSelectedActionOption.neededObjects == null || _currentSelectedActionOption.neededObjects.Count <= 0) {
            if (!_interaction.currentState.isEnd) {
                _currentSelectedActionOption.ActivateOption(_interaction.interactable);
            } else {
                _interaction.currentState.EndResult();
            }
        } else {
            _currentSelectedActionOption.AddAssignedObject(lastAssignedObject);
            //check if needed objects are done
            if (_currentSelectedActionOption.neededObjects.Count > currentNeededObjectIndex + 1) {
                //there are still needed objects
                SlotItem currentSlotItem = slotItems[currentNeededObjectIndex];
                SlotItem nextSlotItem = slotItems[currentNeededObjectIndex + 1];
                nextSlotItem.gameObject.SetActive(true);
                currentNeededObjectIndex++;
                UpdateSideMenu();
            } else {
                //no more needed objects
                if (!_interaction.currentState.isEnd) {
                    _currentSelectedActionOption.ActivateOption(_interaction.interactable);
                } else {
                    _interaction.currentState.EndResult();
                }
            }
        }
    }
    private void ChangeStateAllButtons(bool state) {
        confirmBtn.interactable = state;
        if (!state) {
            for (int i = 0; i < actionOptionButtons.Length; i++) {
                actionOptionButtons[i].toggle.interactable = state;
            }
        }
    }
    //public void OnMinionDrop(Transform transform) {
    //    if (_interaction.isActivated) {
    //        return;
    //    }
    //    MinionItem minionItem = transform.GetComponent<MinionItem>();
    //    _currentSelectedActionOption.assignedMinion = minionItem.minion;
    //    portrait.GeneratePortrait(minionItem.portrait.portraitSettings, 95, true);
    //    if (_currentSelectedActionOption.assignedMinion != null) {
    //        confirmBtn.interactable = true;
    //    }
    //}

    private void OnItemDroppedInSlot(object droppedObject, int slotIndex) {
        confirmBtn.interactable = true;
        lastAssignedObject = droppedObject;
    }
    public void SetToggle(Toggle toggle) {
        _toggle = toggle;
        _toggle.onValueChanged.AddListener(OnToggle);
    }
    private void OnToggle(bool state) {
        if(_isToggled != state) {
            _isToggled = state;
            _toggle.targetGraphic.enabled = !state;
            _toggle.interactable = !state;
            if (state) {
                GoToThisPage();
            }
        } 
    }
    private void GoToThisPage() {
        int index = InteractionUI.Instance.GetIndexOfInteraction(this);
        if (InteractionUI.Instance.scrollSnap.CurrentPage != index) {
            InteractionUI.Instance.scrollSnap.ChangePage(index);
        }
    }

    public void ClearNeededObjectSlots() {
        for (int i = 0; i < neededObjectSlots.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(neededObjectSlots[i].gameObject);
        }
        neededObjectSlots.Clear();
    }

    private void UpdateSideMenu() {
        System.Type neededObject = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        if (neededObject == typeof(FactionIntel)) {
            UIManager.Instance.ShowFactionIntelMenu();
        } else if (neededObject == typeof(CharacterIntel)) {
            UIManager.Instance.ShowCharacterIntelMenu();
        } else if (neededObject == typeof(Minion)) {
            UIManager.Instance.ShowMinionsMenu();
        } else if (neededObject == typeof(LocationIntel)) {
            UIManager.Instance.ShowLocationIntelMenu();
        }
    }
}
