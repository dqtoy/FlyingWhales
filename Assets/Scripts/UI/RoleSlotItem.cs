using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleSlotItem : MonoBehaviour, IDragParentItem {

    private Character character;

    public CharacterPortrait portrait;

    public JOB slotJob { get; private set; }
    [SerializeField] private Image jobIcon;
    [SerializeField] private TextMeshProUGUI jobNameLbl;
    [SerializeField] private Image cooldownProgress;
    [SerializeField] private UIHoverHandler portraitHover;


    [SerializeField] private GameObject validPortraitGO;
    [SerializeField] private GameObject invalidPortraitGO;
    [SerializeField] private GameObject tooltipGO;
    [SerializeField] private TextMeshProUGUI tooltipLbl;

    public CustomDropZone dropZone;

    public System.Type neededType { get; private set; }

    public object associatedObj {
        get { return character; }
    }

    public void SetSlotJob(JOB job) { //This should only be called once!
        slotJob = job;
        UpdateVisuals();
        AddListeners();
        neededType = typeof(Character);
    }

    private void AddListeners() {
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_ASSIGNED_TO_JOB, OnCharacterAssignedToJob);
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        Messenger.AddListener<DragObject>(Signals.DRAG_OBJECT_CREATED, OnDragObjectCreated);
        Messenger.AddListener<DragObject>(Signals.DRAG_OBJECT_DESTROYED, OnDragObjectDestroyed);
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobCooldownActivated);
        //Messenger.AddListener<Intel>(Signals.PLAYER_OBTAINED_INTEL, OnPlayerObtainedIntel);
    }

    public void SetCharacter(Character character) {
        this.character = character;
        if (character == null) {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to null");
        } else {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to " + character.name);
        }
        
        UpdateVisuals();
        //UpdateActionButtons();
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
        if (PlayerManager.Instance.player.roleSlots[slotJob].isSlotLocked) {
            return false;
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
        //UIManager.Instance.HideObjectPicker();
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

    #region Drag
    private void OnDragObjectCreated(DragObject obj) {
        if (dropZone != null && dropZone.isEnabled && neededType != null && CanAssignCharacter(obj.parentItem.associatedObj)) {
            //glowGO.SetActive(true);
            validPortraitGO.SetActive(true);
            invalidPortraitGO.SetActive(false);
        } else {
            validPortraitGO.SetActive(false);
            invalidPortraitGO.SetActive(true);
        }
    }
    private void OnDragObjectDestroyed(DragObject obj) {
        //glowGO.SetActive(false);
        validPortraitGO.SetActive(false);
        invalidPortraitGO.SetActive(false);
    }
    private bool CanAssignCharacter(object obj) {
        if(obj is Character) {
            Character character = obj as Character;
            return CanAssignCharacterToJob(character);
        }
        return false;
    }
    public void OnDropItemAtDropZone(GameObject go) { //this is used to filter if the dragged object is valid for this slot
        DragObject dragObj = go.GetComponent<DragObject>();
        if (dragObj == null) {
            return;
        }
        IDragParentItem parentItem = dragObj.parentItem;
        if (parentItem != null) {
            if (CanAssignCharacter(parentItem.associatedObj)) {
                AssignCharacterToJob(parentItem.associatedObj as Character);
            }
        }
    }
    #endregion

    #region Cooldown
    private void OnJobCooldownActivated(PlayerJobAction action) {
        if (PlayerManager.Instance.player.roleSlots[slotJob].activeAction == action) {
            //the job that was activated is associated with this slot
            if (action.isInCooldown) {
                cooldownProgress.fillAmount = 1f;
                Messenger.AddListener(Signals.TICK_ENDED, UpdateCooldownProgress);
                Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobCooldownDone);
            }
        }
    }
    private void UpdateCooldownProgress() {
        PlayerJobAction activeAction = PlayerManager.Instance.player.roleSlots[slotJob].activeAction;
        float destinationValue = 1f - ((float)activeAction.ticksInCooldown / (float)activeAction.cooldown);
        //float value = Mathf.Lerp(cooldownProgress.fillAmount, destinationValue, Time.deltaTime * 10f);
        //cooldownProgress.fillAmount = value;
        StartCoroutine(SmoothProgress(cooldownProgress.fillAmount, destinationValue));
    }
    IEnumerator SmoothProgress(float start, float end) {
        float t = 0f;
        while (t < 1) {
            if (!GameManager.Instance.isPaused) {
                t += Time.deltaTime / GameManager.Instance.progressionSpeed;
                cooldownProgress.fillAmount = Mathf.Lerp(start, end, t);
            }
            yield return null;
        }
    }
    private void OnJobCooldownDone(PlayerJobAction action) {
        if (PlayerManager.Instance.player.roleSlots[slotJob].activeAction == action && !action.isInCooldown) {
            Messenger.RemoveListener(Signals.TICK_ENDED, UpdateCooldownProgress);
            Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobCooldownDone);
            cooldownProgress.fillAmount = 0f;
        }
    }
    #endregion

    #region Hover
    public void ShowHoverTooltip() {
        string header = Utilities.NormalizeStringUpperCaseFirstLetters(slotJob.ToString()) + ": " + character.name;
        string message = string.Empty;
        switch (slotJob) {
            case JOB.SPY:
                message = "An agent that gathers information about places and characters.";
                break;
            case JOB.RECRUITER:
                header = "Seducer: " + character.name;
                message = "An agent that corrupts heroes and recruits new minions.";
                break;
            case JOB.DIPLOMAT:
                message = "An agent that builds relationships with other characters.";
                break;
            case JOB.INSTIGATOR:
                message = "An agent that sows discord and chaos.";
                break;
            case JOB.DEBILITATOR:
                message = "An agent that halts unwanted actions and activities.";
                break;
        }
        //portraitHover.ShowSmallInfoInSpecificPosition(message, header);
        UIManager.Instance.ShowSmallInfo(message, header);
    }
    public void HideTooltip() {
        UIManager.Instance.HideSmallInfo();
    }
    private void ShowActionBtnTooltip(string message, string header) {
        string m = string.Empty;   
        if (!string.IsNullOrEmpty(header)) {
            m = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        m += "<line-height=70%><size=16>" + message;

        m = m.Replace("\\n", "\n");

        //actionBtnTooltipLbl.text = m;
        //actionBtnTooltipGO.gameObject.SetActive(true);
    }
    public void HideActionBtnTooltip() {
        //actionBtnTooltipGO.gameObject.SetActive(false);
    }
    #endregion
}

public class CharacterLevelComparer : IComparer<Character> {
    public int Compare(Character x, Character y) {
        return x.level.CompareTo(y.level);
    }
}
