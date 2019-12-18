using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheFingersUI : MonoBehaviour {
    [Header("General")]
    public Button createBtn;
    public Image createProgress;

    //[Header("Minion")]
    //public TextMeshProUGUI minionName;
    //public CharacterPortrait minionPortrait;
    //public Button selectMinionBtn;

    [Header("Faction")]
    public GameObject createNewFactionGO;
    public GameObject characterNameplateItemPrefab;
    public TMP_InputField factionNameInput;
    public TextMeshProUGUI leaderNameLbl;
    public ScrollRect characterScrollRect;
    public GameObject ideologyHolder;
    public TMP_Dropdown ideologyDropdown;
    public TextMeshProUGUI ideologiesText;
    public List<CharacterNameplateItem> characterNameplateItems { get; private set; }

    [Header("Exclusive Ideology")]
    public GameObject exclusiveIdeologyHolder;
    public TMP_Dropdown exclusiveIdeologyCategoryDropdown;
    public TMP_Dropdown exclusiveIdeologyRequirementDropdown;

    public TheFingers fingers { get; private set; }
    //public Minion chosenMinion { get; private set; }
    public Character chosenLeader { get; private set; }
    private List<FactionIdeology> ideologies = new List<FactionIdeology>();

    #region General
    public void ShowTheFingersUI(TheFingers fingers) {
        this.fingers = fingers;
        if (characterNameplateItems == null) {
            characterNameplateItems = new List<CharacterNameplateItem>();
        }
        //ideologyDropdown.onValueChanged.AddListener(OnChangeIdeology);
        //exclusiveIdeologyCategoryDropdown.onValueChanged.AddListener(OnChangeExclusiveIdeologyCategory);
        if (!fingers.hasBeenActivated) {
            //chosenMinion = null;
            createBtn.interactable = true;
            createProgress.fillAmount = 0;
            //minionName.gameObject.SetActive(false);
            //minionPortrait.gameObject.SetActive(false);
            //selectMinionBtn.interactable = true;
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
        //fingers.tileLocation.region.SetAssignedMinion(chosenMinion);
        //chosenMinion.SetAssignedRegion(fingers.tileLocation.region);
        //fingers.StartDelay(0);
        ShowCreateNewFactionUI();
        UpdateCreateButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateCreateButton() {
        createProgress.gameObject.SetActive(false);
        createBtn.interactable = /*chosenMinion != null &&*/ !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
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
            for (int i = 0; i < ideologies.Count; i++) {
                newFaction.ideologyComponent.SetCurrentIdeology(i, ideologies[i]);
            }
            //newFaction.ideologyComponent.SwitchToIdeology((FACTION_IDEOLOGY) ideologyDropdown.value);
            //if(newFaction.ideologyComponent.currentIdeologies.ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
            //    Exclusive exclusiveIdeology = newFaction.ideologyComponent.currentIdeologies as Exclusive;
            //    exclusiveIdeology.SetIndividualRequirements((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value, exclusiveIdeologyRequirementDropdown.options[exclusiveIdeologyRequirementDropdown.value].text);
            //}

            Region regionLocation = chosenLeader.currentRegion;
            //if (chosenLeader.currentRegion != null) {
            //    regionLocation = chosenLeader.currentRegion;
            //} else if (chosenLeader.currentArea != null) {
            //    regionLocation = chosenLeader.currentArea.region;
            //}

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "character_create_faction");
            log.AddToFillers(chosenLeader, chosenLeader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            log.AddToFillers(regionLocation, regionLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);

            fingers.Activate();

            HideCreateNewFactionUI();
            UpdateSelectMinionBtn();
            UpdateCreateButton();
            PlayerUI.Instance.ShowGeneralConfirmation("New Faction", "Found New Faction!");

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
        if(ideologies.Count != FactionManager.Instance.categorizedFactionIdeologies.Length) {
            PlayerUI.Instance.ShowGeneralConfirmation("Incomplete Ideologies!", "Please continue selecting ideologies for your faction.");
            return false;
        }
        return true;
    }
    private bool CanAddIdeology() {
        if(ideologies.Count == FactionManager.Instance.categorizedFactionIdeologies.Length) {
            PlayerUI.Instance.ShowGeneralConfirmation("Ideologies Completed!", "Faction ideologies are complete.");
            return false;
        }
        if (GetIdeologyDropdownValue() == FACTION_IDEOLOGY.EXCLUSIVE) {
            if ((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE && exclusiveIdeologyRequirementDropdown.value == 0) { //This means that the race choice is NONE
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
        //chosenMinion = character.minion;
        //minionPortrait.GeneratePortrait(chosenMinion.character);
        //minionName.text = chosenMinion.character.name;
        //minionPortrait.gameObject.SetActive(true);
        //minionName.gameObject.SetActive(true);
        UpdateCreateButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        //selectMinionBtn.interactable = !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
    }
    #endregion

    #region New Faction
    private void ShowCreateNewFactionUI() {
        SetChosenCharacter(null);
        factionNameInput.text = string.Empty;
        ClearIdeologyList();
        PopulateCharactersToChooseFrom();
        HideChooseIdeology();
        createNewFactionGO.SetActive(true);
    }
    public void HideCreateNewFactionUI() {
        createNewFactionGO.SetActive(false);
        UpdateCreateButton();
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
            if(character.isFriendlyFactionless && character.faction.leader != character) {
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
        FACTION_IDEOLOGY[] categorizedIdeologies = FactionManager.Instance.categorizedFactionIdeologies[ideologies.Count];
        //string[] ideologies = System.Enum.GetNames(typeof(FACTION_IDEOLOGY));
        for (int i = 0; i < categorizedIdeologies.Length; i++) {
            ideologyDropdown.options.Add(new TMP_Dropdown.OptionData(System.Enum.GetName(typeof(FACTION_IDEOLOGY), categorizedIdeologies[i])));
        }
        ideologyDropdown.RefreshShownValue();
        ideologyDropdown.value = 0;
    }
    private void ShowAppropriateIdeologyContent() {
        FACTION_IDEOLOGY ideologyType = GetIdeologyDropdownValue();
        if(ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
            ShowExclusiveIdeology();
        } else {
            HideExclusiveIdeology();
        }
    }
    public void OnChangeIdeology(int index) {
        ShowAppropriateIdeologyContent();
    }
    public void AddIdeology() {
        if (CanAddIdeology()) {
            FactionIdeology currIdeology = FactionManager.Instance.CreateIdeology(GetIdeologyDropdownValue());
            if (currIdeology.ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
                Exclusive exclusiveIdeology = currIdeology as Exclusive;
                exclusiveIdeology.SetIndividualRequirements((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value, exclusiveIdeologyRequirementDropdown.options[exclusiveIdeologyRequirementDropdown.value].text);
            }
            AddIdeologyToList(currIdeology);
        }
    }
    public void RemoveIdeology() {
        RemoveRecentIdeology();
    }
    private FACTION_IDEOLOGY GetIdeologyDropdownValue() {
        return (FACTION_IDEOLOGY) System.Enum.Parse(typeof(FACTION_IDEOLOGY), ideologyDropdown.options[ideologyDropdown.value].text);
    }
    private void AddIdeologyToList(FactionIdeology ideology) {
        ideologies.Add(ideology);
        UpdateIdeologiesText();
        if (ideologies.Count < FactionManager.Instance.categorizedFactionIdeologies.Length) {
            ShowChooseIdeology();
        }
    }
    private void RemoveRecentIdeology() {
        if(ideologies.Count > 0) {
            ideologies.RemoveAt(ideologies.Count - 1);
            UpdateIdeologiesText();
            if(ideologies.Count < FactionManager.Instance.categorizedFactionIdeologies.Length) {
                ShowChooseIdeology();
            }
        }
    }
    private void UpdateIdeologiesText() {
        string text = string.Empty;
        for (int i = 0; i < ideologies.Count; i++) {
            if (i > 0) {
                text += ", ";
            }
            text += ideologies[i].name;
        }
        ideologiesText.text = text;
    }
    private void ClearIdeologyList() {
        ideologies.Clear();
        ideologiesText.text = string.Empty;
    }
    #region Exclusive Ideology
    private void ShowExclusiveIdeology() {
        PopulateExclusiveIdeologyCategory();
        PopulateExclusiveIdeologyRequirements();
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
        exclusiveIdeologyCategoryDropdown.RefreshShownValue();
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
        exclusiveIdeologyRequirementDropdown.RefreshShownValue();
        exclusiveIdeologyRequirementDropdown.value = 0;
    }
    public void OnChangeExclusiveIdeologyCategory(int index) {
        PopulateExclusiveIdeologyRequirements();
    }
    #endregion
    #endregion
}
