using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;

public class ConsoleMenu : UIMenu {

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    private int currentHistoryIndex;

    [SerializeField] private Text consoleLbl;
    [SerializeField] private InputField consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private TextMeshProUGUI commandHistoryLbl;

    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown classDropdown;
    [SerializeField] private InputField levelInput;

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/help", ShowHelp},
            {"/change_faction_rel_stat", ChangeFactionRelationshipStatus},
            {"/kill",  KillCharacter},
            {"/lfli", LogFactionLandmarkInfo},
            {"/center_character", CenterOnCharacter},
            {"/center_landmark", CenterOnLandmark },
            {"/show_logs", ShowLogs },
            {"/log_location_history", LogLocationHistory  },
            {"/log_supply_history", LogSupplyHistory  },
            {"/log_area_characters_history", LogAreaCharactersHistory  },
            {"/get_characters_with_item", GetCharactersWithItem },
            {"/subscribe_to_interaction", SubscribeToInteraction },
            {"/add_trait_character", AddTraitToCharacter },
            {"/transfer_character_faction", TransferCharacterToFaction },
        };

#if UNITY_EDITOR
        Messenger.AddListener(Signals.DAY_ENDED, CheckForWrongCharacterData);
#endif
        InitializeMinion();
    }

    public void ShowConsole() {
        isShowing = true;
        this.gameObject.SetActive(true);
        ClearCommandField();
        consoleInputField.Select();
    }

    public void HideConsole() {
        isShowing = false;
        this.gameObject.SetActive(false);
        //consoleInputField.foc = false;
        HideCommandHistory();
        ClearCommandHistory();
    }
    private void ClearCommandField() {
        consoleLbl.text = string.Empty;
    }
    private void ClearCommandHistory() {
        commandHistoryLbl.text = string.Empty;
        commandHistory.Clear();
    }
    private void ShowCommandHistory() {
        commandHistoryGO.SetActive(true);
    }
    private void HideCommandHistory() {
        commandHistoryGO.SetActive(false);
    }
    public void SubmitCommand() {
        string command = consoleLbl.text;
        string[] words = command.Split(' ');
        string mainCommand = words[0];

        var reg = new Regex("\".*?\"");
        var parameters = reg.Matches(command).Cast<Match>().Select(m => m.Value).ToArray();
        //List<string> parameters = matches.Cast<string>().ToList();
        for (int i = 0; i < parameters.Length; i++) {
            string currParameter = parameters[i];
            string trimmed = currParameter.Trim(new char[] { '"' });
            parameters[i] = trimmed;
        }

        if (_consoleActions.ContainsKey(mainCommand)) {
            _consoleActions[mainCommand](parameters);
        } else {
            AddCommandHistory(command);
            AddErrorMessage("Error: there is no such command as " + mainCommand + "![-]");
        }
    }
    private void AddCommandHistory(string history) {
        commandHistoryLbl.text += history + "\n";
        commandHistory.Add(history);
        currentHistoryIndex = commandHistory.Count - 1;
        ShowCommandHistory();
    }
    private void AddErrorMessage(string errorMessage) {
        errorMessage += ". Use /help for a list of commands";
        commandHistoryLbl.text += "<color=#FF0000>" + errorMessage + "</color>\n";
        ShowCommandHistory();
    }
    private void AddSuccessMessage(string successMessage) {
        commandHistoryLbl.text += "<color=#00FF00>" + successMessage + "</color>\n";
        ShowCommandHistory();
    }

    private void Update() {
        if (isShowing && consoleInputField.text != "" && Input.GetKeyDown(KeyCode.Return)) {
            SubmitCommand();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            int newIndex = currentHistoryIndex - 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            int newIndex = currentHistoryIndex + 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
    }

    private void CheckForWrongCharacterData() {
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                Character character = currArea.charactersAtLocation[j];
                if (character.isDead) {
                    Debug.LogWarning("There is still a dead character at " + currArea.name + " : " + character.name);
                    UIManager.Instance.Pause();
                }
            }
            for (int j = 0; j < currArea.possibleSpecialTokenSpawns.Count; j++) {
                SpecialToken token = currArea.possibleSpecialTokenSpawns[j];
                if (token.structureLocation == null) {
                    Debug.LogWarning("There is token at " + currArea.name + " that doesn't have a structure location : " + token.name);
                    UIManager.Instance.Pause();
                }
            }
        }
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (!currCharacter.isDead) {
                if (currCharacter.faction == null) {
                    Debug.LogWarning("There is an alive character with a null faction! " + currCharacter.name);
                    UIManager.Instance.Pause();
                }
            }
        }
    }

    #region Misc
    private void ShowHelp(string[] parameters) {
        for (int i = 0; i < _consoleActions.Count; i++) {
            AddCommandHistory(_consoleActions.Keys.ElementAt(i));
        }
    }
    public void AddText(string text) {
        consoleInputField.text += " " + text;
    }
    #endregion

    #region Faction Relationship
    private void ChangeFactionRelationshipStatus(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1ParameterString = parameters[0];
        string faction2ParameterString = parameters[1];
        string newRelStatusString = parameters[2];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1ParameterString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2ParameterString, out faction2ID);

        string faction1Name = faction1ParameterString;
        string faction2Name = faction2ParameterString;

        FACTION_RELATIONSHIP_STATUS newRelStatus;

        if (isFaction1Numeric) {
            faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        } else {
            faction1 = FactionManager.Instance.GetFactionBasedOnName(faction1Name);
        }

        if (isFaction2Numeric) {
            faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        } else {
            faction2 = FactionManager.Instance.GetFactionBasedOnName(faction2Name);
        }

        try {
            newRelStatus = (FACTION_RELATIONSHIP_STATUS)Enum.Parse(typeof(FACTION_RELATIONSHIP_STATUS), newRelStatusString, true);
        } catch {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        if (faction1 == null || faction2 == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(faction1, faction2);
        rel.SetRelationshipStatus(newRelStatus);

        AddSuccessMessage("Changed relationship status of " + faction1.name + " and " + faction2.name + " to " + rel.relationshipStatus.ToString());
    }
    #endregion

    #region Landmarks
    private void CenterOnLandmark(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of Center on Landmark");
            return;
        }
        string landmarkParameterString = parameters[0];
        int landmarkID;

        bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
        BaseLandmark landmark = null;
        if (isLandmarkParameterNumeric) {
            landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
        } else {
            landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
        }

        if (landmark == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        landmark.CenterOnLandmark();
        //UIManager.Instance.ShowLandmarkInfo(landmark);
        //character.CenterOnCharacter();
    }
    #endregion
    
    #region Characters
    private void KillCharacter(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

        Character character = null;

        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }

        character.Death();
    }
    private void CenterOnCharacter(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of Center on Character");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of Center on Character");
            return;
        }
        UIManager.Instance.ShowCharacterInfo(character);
        //character.CenterOnCharacter();
    }
    private void ShowLogs(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ShowLogs");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of ShowLogs");
            return;
        }

        string logSummary = character.name + "'s logs: ";
        List<string> logs = CharacterManager.Instance.GetCharacterLogs(character);
        for (int i = 0; i < logs.Count; i++) {
            logSummary += "\n" + logs[i];
        }
        AddSuccessMessage(logSummary);
    }
    private void LogLocationHistory(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogLocationHistory");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of LogLocationHistory");
            return;
        }

        string logSummary = character.name + "'s location history: ";
        List<string> logs = character.ownParty.specificLocationHistory;
        for (int i = 0; i < logs.Count; i++) {
            logSummary += "\n" + logs[i];
        }
        AddSuccessMessage(logSummary);
    }
    private void GetCharactersWithItem(string[] parameters) {
        if (parameters.Length != 1) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of GetCharactersWithItem");
            return;
        }
        string itemParameterString = parameters[0];

        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.tokenInInventory != null && currCharacter.tokenInInventory.tokenName.ToLower() == itemParameterString.ToLower()) {
                characters.Add(currCharacter);
            }
        }
        string summary = "Characters that have " + itemParameterString + ": ";
        if (characters.Count == 0) {
            summary += "\nNONE";
        } else {
            for (int i = 0; i < characters.Count; i++) {
                summary += "\n" + characters[i].name;
            }
        }
        AddSuccessMessage(summary);
    }
    private void AddTraitToCharacter(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AddTraitToCharacter");
            return;
        }
        string characterParameterString = parameters[0];
        string traitParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage("There is no character named " + characterParameterString);
            return;
        }

        if (AttributeManager.Instance.allTraits.ContainsKey(traitParameterString)) {
            character.AddTrait(AttributeManager.Instance.allTraits[traitParameterString]);
        } else {
            switch (traitParameterString) {
                case "Criminal":
                    character.AddTrait(new Criminal());
                    break;
                default:
                    AddErrorMessage("There is no trait called " + traitParameterString);
                    return;
            }
        }
        AddSuccessMessage("Added " + traitParameterString + " to " + character.name);
    }
    private void TransferCharacterToFaction(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of TransferCharacterToFaction");
            return;
        }
        string characterParameterString = parameters[0];
        string factionParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        if (character == null) {
            AddErrorMessage("There is no character named " + characterParameterString);
            return;
        }
        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
        if (faction == null) {
            AddErrorMessage("There is no faction named " + factionParameterString);
            return;
        }

        character.ChangeFactionTo(faction);
        AddSuccessMessage("Transferred " + character.name + " to " + faction.name);
    }
    #endregion

    #region Faction
    private void LogFactionLandmarkInfo(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /lfli");
            return;
        }
        string factionParameterString = parameters[0];
        int factionID;

        bool isFactionParameterNumeric = int.TryParse(factionParameterString, out factionID);
        Faction faction = null;
        if (isFactionParameterNumeric) {
            faction = FactionManager.Instance.GetFactionBasedOnID(factionID);
            if (faction == null) {
                AddErrorMessage("There was no faction with id " + factionID);
                return;
            }
        } else {
           faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
            if (faction == null) {
                AddErrorMessage("There was no faction with name " + factionParameterString);
                return;
            }
        }

        string text = faction.name + "'s Landmark Info: ";
        for (int i = 0; i < faction.landmarkInfo.Count; i++) {
            BaseLandmark currLandmark = faction.landmarkInfo[i];
			text += "\n" + currLandmark.landmarkName + " (" + currLandmark.tileLocation.name + ") ";
        }
        
        AddSuccessMessage(text);
    }
    #endregion

    #region Area
    private void LogSupplyHistory(string[] parameters) {
        if (parameters.Length != 1) { 
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string areaParameterString = parameters[0];
        int areaID;
        
        bool isAreaParameterNumeric = int.TryParse(areaParameterString, out areaID);

        Area area = null;
        if (isAreaParameterNumeric) {
            area = LandmarkManager.Instance.GetAreaByID(areaID);
        } else {
            area = LandmarkManager.Instance.GetAreaByName(areaParameterString);
        }

        string text = area.name + "'s Supply History: ";
        for (int i = 0; i < area.supplyLog.Count; i++) {
            text += "\n" + area.supplyLog[i];
        }
        AddSuccessMessage(text);
    }
    private void LogAreaCharactersHistory(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogAreaCharactersHistory");
            return;
        }

        string areaParameterString = parameters[0];
        int areaID;
        bool isAreaParameterNumeric = int.TryParse(areaParameterString, out areaID);

        Area area = null;
        if (isAreaParameterNumeric) {
            area = LandmarkManager.Instance.GetAreaByID(areaID);
        } else {
            area = LandmarkManager.Instance.GetAreaByName(areaParameterString);
        }

        string text = area.name + "'s Characters History: ";
        for (int i = 0; i < area.charactersAtLocationHistory.Count; i++) {
            text += "\n" + area.charactersAtLocationHistory[i];
        }
        AddSuccessMessage(text);
    }
    #endregion

    #region Interactions
    private void SubscribeToInteraction(string[] parameters) {

    }
    #endregion

    #region Minion
    private void InitializeMinion() {
        levelInput.text = "1";
        ConstructRaceDropdown();
        ConstructClassDropdown();
    }
    private void ConstructRaceDropdown() {
        raceDropdown.ClearOptions();
        string[] races = System.Enum.GetNames(typeof(RACE));
        raceDropdown.AddOptions(races.ToList());
    }
    private void ConstructClassDropdown() {
        List<string> allClasses = new List<string>();
        string path = Utilities.dataPath + "CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allClasses.Add(Path.GetFileNameWithoutExtension(file));
        }
        classDropdown.ClearOptions();
        classDropdown.AddOptions(allClasses);
    }
    public void AddMinion() {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text);
        string className = classDropdown.options[classDropdown.value].text;
        int level = int.Parse(levelInput.text);
        if(race != RACE.NONE && level > 0) {
            Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, race);
            if(level > 1) {
                minion.character.LevelUp(level - 1);
            }
            PlayerManager.Instance.player.AddMinion(minion);
        }
    }
    #endregion
}
