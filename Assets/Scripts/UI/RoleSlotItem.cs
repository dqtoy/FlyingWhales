using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleSlotItem : MonoBehaviour {

    private Character character;

    [SerializeField] private JOB slotJob;
    [SerializeField] private Image jobIcon;
    [SerializeField] private TextMeshProUGUI jobNameLbl;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Button assignBtn;

    [Header("Job Actions")]
    [SerializeField] private GameObject jobActionBtnPrefab;
    [SerializeField] private RectTransform jobActionsParent;

    public void SetSlotJob(JOB job) { //This should only be called once!
        slotJob = job;
        UpdateVisuals();
        AddListeners();
    }

    private void AddListeners() {
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_ASSIGNED_TO_JOB, OnCharacterAssignedToJob);
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<JOB, bool>(Signals.JOB_SLOT_LOCK_CHANGED, OnJobSlotLockChanged);
    }

    public void SetCharacter(Character character) {
        this.character = character;
        if (character == null) {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to null");
        } else {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to " + character.name);
        }
        
        UpdateVisuals();
        UpdateActionButtons();
    }
    private void UpdateVisuals() {
        jobIcon.sprite = CharacterManager.Instance.GetJobSprite(slotJob);
        jobNameLbl.text = Utilities.NormalizeString(slotJob.ToString());
        if (character == null) {
            portrait.gameObject.SetActive(false);
        } else {
            portrait.GeneratePortrait(character);
            portrait.gameObject.SetActive(true);
        }
    }
	
    public void OnClickAssign() {
        UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.allOwnedCharacters, AssignCharacterToJob, new CharacterLevelComparer(), CanAssignCharacterToJob);
    }
    private bool CanAssignCharacterToJob(Character character) {
        if (this.character != null && this.character.id == character.id) {
            return false; //This means that the character is already assigned to this job
        }
        JOB charactersJob = PlayerManager.Instance.player.GetCharactersCurrentJob(character);
        if (charactersJob != JOB.NONE 
            && PlayerManager.Instance.player.roleSlots[charactersJob].activeAction != null
            && PlayerManager.Instance.player.roleSlots[charactersJob].activeAction.isInCooldown) {
            return false;
        }
        return PlayerManager.Instance.player.CanAssignCharacterToJob(slotJob, character);
    }
    private void AssignCharacterToJob(Character character) {
        Debug.Log("Assigning " + character.name + " to job " + slotJob.ToString());
        PlayerManager.Instance.player.AssignCharacterToJob(slotJob, character);
        UIManager.Instance.HideObjectPicker();
    }

    private void OnCharacterAssignedToJob(JOB job, Character character) {
        if (slotJob == job) {
            SetCharacter(character);
        }
    }
    private void OnCharacterUnassignedFromJob(JOB job, Character character) {
        if (slotJob == job) {
            SetCharacter(null);
        }
    }

    #region Assign
    private void OnJobSlotLockChanged(JOB job, bool isLocked) {
        if (slotJob == job) {
            assignBtn.interactable = !isLocked;
        }
    }
    #endregion

    #region Action Buttons
    private void HideActionButtons() {
        jobActionsParent.gameObject.SetActive(false);
    }
    private void ShowActionButtons(JOB_ACTION_TARGET actionTarget) {
        Utilities.DestroyChildren(jobActionsParent);
        List<PlayerJobAction> actions = PlayerManager.Instance.player.GetJobActionsThatCanTarget(slotJob, actionTarget);
        for (int i = 0; i < actions.Count; i++) {
            PlayerJobAction currAction = actions[i];
            GameObject buttonGO = UIManager.Instance.InstantiateUIObject(jobActionBtnPrefab.name, jobActionsParent);
            buttonGO.name = currAction.actionName;
            PlayerJobActionButton btn = buttonGO.GetComponent<PlayerJobActionButton>();
            switch (actionTarget) {
                case JOB_ACTION_TARGET.CHARACTER:
                    btn.SetJobAction(currAction, character, UIManager.Instance.characterInfoUI.activeCharacter);
                    btn.SetClickAction(() => currAction.ActivateAction(character, UIManager.Instance.characterInfoUI.activeCharacter));
                    break;
                case JOB_ACTION_TARGET.AREA:
                    btn.SetJobAction(currAction, character, UIManager.Instance.areaInfoUI.activeArea);
                    btn.SetClickAction(() => currAction.ActivateAction(character, UIManager.Instance.areaInfoUI.activeArea));
                    break;
                case JOB_ACTION_TARGET.FACTION:
                    break;
                default:
                    break;
            }
        }
        jobActionsParent.gameObject.SetActive(true);
    }
    private void OnMenuOpened(UIMenu menu) {
        UpdateActionButtons();
    }
    private void OnMenuClosed(UIMenu menu) {
        UpdateActionButtons();
    }
    private void UpdateActionButtons() {
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.minion == null) {
            ShowActionButtons(JOB_ACTION_TARGET.CHARACTER);
        } else if (UIManager.Instance.areaInfoUI.isShowing && UIManager.Instance.areaInfoUI.activeArea.id != PlayerManager.Instance.player.playerArea.id) {
            ShowActionButtons(JOB_ACTION_TARGET.AREA);
        } else {
            HideActionButtons();
        }
    }
    #endregion
}

public class CharacterLevelComparer : IComparer<Character> {
    public int Compare(Character x, Character y) {
        return x.level.CompareTo(y.level);
    }
}
