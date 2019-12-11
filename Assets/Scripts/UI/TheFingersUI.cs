using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheFingersUI : MonoBehaviour {
    [Header("General")]
    public Button createBtn;
    public Image createProgress;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    [Header("Faction")]
    public GameObject createNewFactionGO;
    public GameObject characterNameplateItemPrefab;
    public TMP_InputField factionNameInput;
    public TextMeshProUGUI leaderNameLbl;
    public ScrollRect characterScrollRect;
    public GameObject ideologyHolder;
    public TMP_Dropdown ideologyDropdown;
    public List<CharacterNameplateItem> characterNameplateItems { get; private set; }

    [Header("Exclusive Ideology")]
    public GameObject exclusiveIdeologyHolder;
    public TMP_Dropdown exclusiveIdeologyCategoryDropdown;
    public TMP_Dropdown exclusiveIdeologyRequirementDropdown;

    public TheFingers fingers { get; private set; }
    public Minion chosenMinion { get; private set; }
    public Character chosenLeader { get; private set; }

    #region General
    public void ShowTheFingersUI(TheFingers fingers) {
        this.fingers = fingers;
        if (characterNameplateItems == null) {
            characterNameplateItems = new List<CharacterNameplateItem>();
        }
        if (!fingers.hasBeenActivated) {
            chosenMinion = null;
            createBtn.interactable = false;
            createProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
        } else {
            SetChosenMinion(fingers.tileLocation.region.assignedMinion.character);
            UpdateSelectMinionBtn();
            UpdateTheFingersUI();
        }

        gameObject.SetActive(true);
    }
    public void HideTheFingersUI() {
        gameObject.SetActive(false);
    }
    public void UpdateTheFingersUI() {
        if (fingers.hasBeenActivated && createProgress.gameObject.activeSelf) {
            createProgress.fillAmount = fingers.currentTick / (float) fingers.duration;
        }
    }
    public void OnClickCreate() {
        fingers.tileLocation.region.SetAssignedMinion(chosenMinion);
        chosenMinion.SetAssignedRegion(fingers.tileLocation.region);
        //fingers.StartDelay(0);
        ShowCreateNewFactionUI();
        UpdateCreateButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateCreateButton() {
        createProgress.gameObject.SetActive(false);
        createBtn.interactable = chosenMinion != null && !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
        if (!createBtn.interactable) {
            if (fingers.hasBeenActivated) {
                createProgress.gameObject.SetActive(true);
                createProgress.fillAmount = 0;
            }
        }
    }
    public void OnClickCreateNewFaction() {
        if (CanCreateNewFaction()) {
            Faction newFaction = FactionManager.Instance.CreateNewFaction(factionName: factionNameInput.text);
            chosenLeader.ChangeFactionTo(newFaction);
            newFaction.SetLeader(chosenLeader, false);
            newFaction.ideologyComponent.SwitchToIdeology((FACTION_IDEOLOGY) ideologyDropdown.value);
            if(newFaction.ideologyComponent.currentIdeology.ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
                Exclusive exclusiveIdeology = newFaction.ideologyComponent.currentIdeology as Exclusive;
                exclusiveIdeology.SetIndividualRequirements((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value, exclusiveIdeologyRequirementDropdown.options[exclusiveIdeologyRequirementDropdown.value].text);
            }

            Region regionLocation = null;
            if (chosenLeader.currentRegion != null) {
                regionLocation = chosenLeader.currentRegion;
            } else if (chosenLeader.specificLocation != null) {
                regionLocation = chosenLeader.specificLocation.region;
            }

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_create_faction");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            log.AddToFillers(regionLocation, regionLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);

            fingers.Activate();

            HideCreateNewFactionUI();
            UpdateSelectMinionBtn();
            UpdateCreateButton();
            PlayerUI.Instance.ShowGeneralConfirmation("New Faction", "Foung New Faction!");

        }
    }
    private bool CanCreateNewFaction() {
        if (string.IsNullOrEmpty(factionNameInput.text)) {
            PlayerUI.Instance.ShowGeneralConfirmation("No Faction Name Error!", "Please enter a faction name.");
            return false;
        }
        if (chosenLeader == null) {
            PlayerUI.Instance.ShowGeneralConfirmation("No Faction Leader Error!", "Please select a faction leader.");
            return false;
        }
        if ((FACTION_IDEOLOGY) ideologyDropdown.value == FACTION_IDEOLOGY.EXCLUSIVE) {
            if((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE && exclusiveIdeologyRequirementDropdown.value == 0) { //This means that the race choice is NONE
                PlayerUI.Instance.ShowGeneralConfirmation("No Race Requirement Error!", "Please select a race for the requirement.");
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    private void SetChosenMinion(object c) {
        Character character = c as Character;
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateCreateButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
    }
    #endregion

    #region New Faction
    private void ShowCreateNewFactionUI() {
        SetChosenCharacter(null);
        factionNameInput.text = string.Empty;
        PopulateCharactersToChooseFrom();
        HideChooseIdeology();
        createNewFactionGO.SetActive(true);
    }
    public void HideCreateNewFactionUI() {
        createNewFactionGO.SetActive(false);
    }
    private void ShowChooseIdeology() {
        ideologyHolder.SetActive(true);
        HideExclusiveIdeology();
        PopulateIdeologyDropdown();
    }
    private void HideChooseIdeology() {
        ideologyHolder.SetActive(false);
        //exclusiveIdeologyHolder.SetActive(false);
    }
    private void PopulateCharactersToChooseFrom() {
        Utilities.DestroyChildren(characterScrollRect.content);
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.faction.isMajorFriendlyNeutral && !character.faction.isPlayerFaction) {
                CharacterNameplateItem item = CreateNewCharacterNameplateItem();
                item.SetObject(character);
                item.AddOnClickAction(OnClickCharacter);
                item.gameObject.SetActive(true);
            }
        }
    }
    private CharacterNameplateItem CreateNewCharacterNameplateItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterNameplateItemPrefab.name, Vector3.zero, Quaternion.identity, characterScrollRect.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        characterNameplateItems.Add(item);
        return item;
    }
    private void OnClickCharacter(Character character) {
        if(chosenLeader == null) {
            ShowChooseIdeology();
        }
        SetChosenCharacter(character);
    }
    private void SetChosenCharacter(Character character) {
        chosenLeader = character;
        if(chosenLeader == null) {
            leaderNameLbl.text = string.Empty;
        } else {
            leaderNameLbl.text = character.name + ", " + character.raceClassName;
        }
    }
    #endregion

    #region Ideology
    private void PopulateIdeologyDropdown() {
        ideologyDropdown.ClearOptions();
        string[] ideologies = System.Enum.GetNames(typeof(FACTION_IDEOLOGY));
        for (int i = 0; i < ideologies.Length; i++) {
            ideologyDropdown.options.Add(new TMP_Dropdown.OptionData(ideologies[i]));
        }
        ideologyDropdown.value = 0;
    }
    private void ShowAppropriateIdeologyContent() {
        FACTION_IDEOLOGY ideologyType = (FACTION_IDEOLOGY) ideologyDropdown.value;
        if(ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
            ShowExclusiveIdeology();
        } else {
            HideExclusiveIdeology();
        }
    }
    public void OnChangeIdeology(int index) {
        ShowAppropriateIdeologyContent();
    }

    #region Exclusive Ideology
    private void ShowExclusiveIdeology() {
        PopulateExclusiveIdeologyCategory();
        exclusiveIdeologyHolder.SetActive(true);
    }
    private void HideExclusiveIdeology() {
        exclusiveIdeologyHolder.SetActive(false);
    }
    private void PopulateExclusiveIdeologyCategory() {
        exclusiveIdeologyCategoryDropdown.ClearOptions();
        string[] exclusiveIdeologyCategories = System.Enum.GetNames(typeof(EXCLUSIVE_IDEOLOGY_CATEGORIES));
        for (int i = 0; i < exclusiveIdeologyCategories.Length; i++) {
            exclusiveIdeologyCategoryDropdown.options.Add(new TMP_Dropdown.OptionData(exclusiveIdeologyCategories[i]));
        }
        exclusiveIdeologyCategoryDropdown.value = 0;
    }
    private void PopulateExclusiveIdeologyRequirements() {
        EXCLUSIVE_IDEOLOGY_CATEGORIES category = (EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value;
        exclusiveIdeologyRequirementDropdown.ClearOptions();
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            string[] genders = System.Enum.GetNames(typeof(GENDER));
            for (int i = 0; i < genders.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(genders[i]));
            }
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            string[] races = System.Enum.GetNames(typeof(RACE));
            for (int i = 0; i < races.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(races[i]));
            }
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
            string[] traits = FactionManager.Instance.exclusiveIdeologyTraitRequirements;
            for (int i = 0; i < traits.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(traits[i]));
            }
        }
        exclusiveIdeologyRequirementDropdown.value = 0;
    }
    public void OnChangeExclusiveIdeologyCategory(int index) {
        PopulateExclusiveIdeologyRequirements();
    }
    #endregion
    #endregion
}
