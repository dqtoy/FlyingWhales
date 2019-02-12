using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Events;

public class ConsoleMenu : UIMenu {

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    private int currentHistoryIndex;

    [SerializeField] private GameObject consoleGO;
    [SerializeField] private Text consoleLbl;
    [SerializeField] private InputField consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private TextMeshProUGUI commandHistoryLbl;

    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown classDropdown;
    [SerializeField] private InputField levelInput;

    [SerializeField] private GameObject fullDebugGO;
    [SerializeField] private TextMeshProUGUI fullDebugLbl;
    [SerializeField] private TextMeshProUGUI fullDebug2Lbl;

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
            {"/i_toggle_sub", ToggleSubscriptionToInteraction },
            {"/add_trait_character", AddTraitToCharacter },
            {"/transfer_character_faction", TransferCharacterToFaction },
            {"/show_full_debug", ShowFullDebug },
            {"/force_action", ForceCharacterInteraction },
            {"/t_freeze_char", ToggleFreezeCharacter },
        };

#if UNITY_EDITOR
        Messenger.AddListener(Signals.DAY_ENDED, CheckForWrongCharacterData);
        Messenger.AddListener<Interaction>(Signals.INTERACTION_INITIALIZED, ListenForInteraction);
#endif
        InitializeMinion();
    }

    private void Update() {
        fullDebugLbl.text = string.Empty;
        fullDebug2Lbl.text = string.Empty;
        if (GameManager.Instance.showFullDebug) {
            FullDebugInfo();
        }
        fullDebugGO.SetActive(!string.IsNullOrEmpty(fullDebugLbl.text) || !string.IsNullOrEmpty(fullDebug2Lbl.text));

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

    #region Full Debug
    private void FullDebugInfo() {
        fullDebugLbl.text = string.Empty;
        fullDebug2Lbl.text = string.Empty;
        if (UIManager.Instance != null && UIManager.Instance.characterInfoUI.isShowing) {
            fullDebugLbl.text += GetMainCharacterInfo();
            fullDebug2Lbl.text += GetSecondaryCharacterInfo();
        } else if (UIManager.Instance != null && UIManager.Instance.areaInfoUI.isShowing) {
            fullDebugLbl.text += GetMainAreaInfo();
            fullDebug2Lbl.text += GetSecondaryAreaInfo();
        }
    }
    private string GetMainCharacterInfo() {
        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        string text = character.name + "'s info:";
        text += "\n<b>Gender:</b> " + character.gender.ToString();
        text += "\n<b>Race:</b> " + character.race.ToString();
        text += "\n<b>Class:</b> " + character.characterClass.className;
        text += "\n<b>Is Dead?:</b> " + character.isDead.ToString();
        text += "\n<b>Current Location:</b> " + character.currentStructure?.ToString() ?? "None";
        text += "\n<b>Home Location:</b> " + character.homeStructure?.ToString() ?? "None";
        text += "\n<b>Faction:</b> " + character.faction?.name ?? "None";
        text += "\n<b>Next Tick:</b> " + character.currentInteractionTick.ToString();
        text += "\n<b>Override:</b> " + character.forcedInteraction?.ToString() ?? "None";
        if (character.currentParty.icon != null) {
            text += "\n<b>Target Location:</b> " + character.currentParty.icon.targetLocation?.name ?? "None";
            text += "\n<b>Target Structure:</b> " + character.currentParty.icon.targetStructure?.ToString() ?? "None";
        }
        return text;
    }
    private string GetSecondaryCharacterInfo() {
        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        string text = character.name + "'s Relationships " + character.relationships.Count.ToString();
        int counter = 0;
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
            text += "\n\n" + counter.ToString() + kvp.Value.GetSummary();
            counter++;
        }

        text += "\n" + character.name + "'s Location History:";
        for (int i = 0; i < character.locationHistory.Count; i++) {
            text += "\n\t" + character.locationHistory[i];
        }
        return text;
    }

    private string GetMainAreaInfo() {
        Area area = UIManager.Instance.areaInfoUI.activeArea;
        string text = area.name + "'s info:";
        text += "\n<b>Owner:</b> " + area.owner?.name ?? "None";
        text += "\n<b>Race:</b> " + area.raceType.ToString();
        text += "\n<b>Residents:</b> " + area.areaResidents.Count + "/" + area.residentCapacity;
        if (area.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            for (int i = 0; i < area.structures[STRUCTURE_TYPE.DWELLING].Count; i++) {
                Dwelling dwelling = area.structures[STRUCTURE_TYPE.DWELLING][i] as Dwelling;
                text += "\n\t" + dwelling.ToString() + " Residents";
                for (int j = 0; j < dwelling.residents.Count; j++) {
                    text += "\n\t\t- " + dwelling.residents[j].name;
                }
            }
        }
        text += "\n<b>Corpses:</b> ";
        if (area.corpsesInArea.Count > 0) {
            for (int i = 0; i < area.corpsesInArea.Count; i++) {
                text += "\n\t" + area.corpsesInArea[i].character.name;
            }
        } else {
            text += "None";
        }
        return text;
    }
    private string GetSecondaryAreaInfo() {
        Area area = UIManager.Instance.areaInfoUI.activeArea;
        string text = area.name + "'s Structures:";
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                text += "\n<b>" + structure.ToString() + "</b>";
                text += "\n\tPoints of interest: ";
                for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                    text += "\n\t\t-" + structure.pointsOfInterest[j].ToString();
                }
                text += "\n\tTraits: ";
                for (int j = 0; j < structure.traits.Count; j++) {
                    text += "\n\t\t-" + structure.traits[j].ToString();
                }
            }
        }
       
        return text;
    }
    #endregion

    public void ShowConsole() {
        isShowing = true;
        consoleGO.SetActive(true);
        ClearCommandField();
        consoleInputField.Select();
    }

    public void HideConsole() {
        isShowing = false;
        consoleGO.SetActive(false);
        //consoleInputField.foc = false;
        HideCommandHistory();
        ClearCommandHistory();
        consoleInputField.DeactivateInputField();
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
                    //UIManager.Instance.Pause();
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
                if (currCharacter.homeStructure == null) {
                    Debug.LogWarning("There is an alive character with a null home structure! " + currCharacter.name);
                    UIManager.Instance.Pause();
                }
                if (currCharacter.currentStructure == null) {
                    Debug.LogWarning("There is an alive character with a null current structure! " + currCharacter.name);
                    //UIManager.Instance.Pause();
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
    public void ShowFullDebug(string[] parameters) {
        GameManager.Instance.showFullDebug = !GameManager.Instance.showFullDebug;
        AddSuccessMessage("Show Full Debug Info Set to " + GameManager.Instance.showFullDebug.ToString());
    }
    public void MoveNextPage(TextMeshProUGUI text) {
        text.pageToDisplay += 1;
    }
    public void MovePreviousPage(TextMeshProUGUI text) {
        text.pageToDisplay -= 1;
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
            if (currCharacter.isHoldingItem && currCharacter.tokenInInventory.tokenName.ToLower() == itemParameterString.ToLower()) {
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
    private void ForceCharacterInteraction(string[] parameters) {
        string interactionTypeStr = parameters[0];
        string characterStr = parameters[1];
        string targetCharacterStr = parameters.ElementAtOrDefault(2);
        string otherCharacterStr = parameters.ElementAtOrDefault(3);

        INTERACTION_TYPE interactionType;
        if (!System.Enum.TryParse<INTERACTION_TYPE>(interactionTypeStr, out interactionType)) {
            AddErrorMessage("There is no interaction with type " + interactionTypeStr);
            return;
        }
        Character character = CharacterManager.Instance.GetCharacterByName(characterStr);
        if (character == null) {
            AddErrorMessage("There is no character with name " + characterStr);
            return;
        }
        Character targetCharacter = CharacterManager.Instance.GetCharacterByName(targetCharacterStr);
        Character otherCharacter = CharacterManager.Instance.GetCharacterByName(otherCharacterStr);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, character.specificLocation);
        if (targetCharacter != null) {
            interaction.SetTargetCharacter(targetCharacter);
        }
        if (otherCharacter != null) {
            interaction.SetOtherCharacter(otherCharacter);
        }
        if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER) {
            (interaction as UseItemOnCharacter).SetItemToken(character.tokenInInventory);
        }
        character.SetForcedInteraction(interaction);

        AddSuccessMessage("Set " + character.name + "'s override to " + interaction.name);
    }
    private void ToggleFreezeCharacter(string[] parameter) {
        if (parameter.Length < 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ToggleFreezeCharacter");
            return;
        }
        string characterParameter = parameter[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameter);
        if (character == null) {
            AddErrorMessage("There is no character with name " + characterParameter);
            return;
        }

        if (character.doNotDisturb == 0) {
            character.AdjustDoNotDisturb(1);
        } else {
            character.AdjustDoNotDisturb(-1);
        }
        AddSuccessMessage("Adjusted " + character.name + " do not disturb to " + character.doNotDisturb);
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
    private List<INTERACTION_TYPE> typesSubscribedTo = new List<INTERACTION_TYPE>();
    private void ToggleSubscriptionToInteraction(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SubscribeToInteraction");
            return;
        }

        string typeParameterString = parameters[0];
        INTERACTION_TYPE type;
        if (typeParameterString.Equals("All")) {
            if (typesSubscribedTo.Count > 0) {
                typesSubscribedTo.Clear();
                AddSuccessMessage("Unsubscribed from ALL interactions");
            } else {
                typesSubscribedTo.AddRange(Utilities.GetEnumValues<INTERACTION_TYPE>());
                AddSuccessMessage("Subscribed to ALL interactions");
            }
        } else if (Enum.TryParse<INTERACTION_TYPE>(typeParameterString, out type)) {
            if (typesSubscribedTo.Contains(type)) {
                typesSubscribedTo.Remove(type);
                AddSuccessMessage("Unsubscribed from " + type + " interactions");
            } else {
                typesSubscribedTo.Add(type);
                AddSuccessMessage("Subscribed to " + type + " interactions");
            }
        } else {
            AddErrorMessage("There is no interaction of type " + typeParameterString);
        }
    }
    private void ListenForInteraction(Interaction interaction) {
        if (typesSubscribedTo.Contains(interaction.type)) {
            Messenger.Broadcast<string, int, UnityAction>(Signals.SHOW_NOTIFICATION,
                "<b>" + interaction.name + "</b> event happened at <b>" + interaction.interactable.name + "</b>.",
                10,
                () => UIManager.Instance.ShowAreaInfo(interaction.interactable));
            UIManager.Instance.Pause();
        }
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
