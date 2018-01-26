using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ConsoleMenu : UIMenu {

    public bool isShowing = false;

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    private int currentHistoryIndex;

    [SerializeField] private UILabel consoleLbl;
    [SerializeField] private UIInput consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private UILabel commandHistoryLbl;

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/change_faction_rel_stat", ChangeFactionRelationshipStatus}
        };
    }

    public void ShowConsole() {
        isShowing = true;
        this.gameObject.SetActive(true);
        ClearCommandField();
        consoleInputField.isSelected = true;
    }

    public void HideConsole() {
        isShowing = false;
        this.gameObject.SetActive(false);
        consoleInputField.isSelected = false;
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
        if (_consoleActions.ContainsKey(mainCommand)) {
            _consoleActions[mainCommand](words);
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
        commandHistoryLbl.text += "[FF0000]" + errorMessage + "[-]\n";
        ShowCommandHistory();
    }
    private void AddSuccessMessage(string successMessage) {
        commandHistoryLbl.text += "[00FF00]" + successMessage + "[-]\n";
        ShowCommandHistory();
    }

    private void Update() {
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

    #region Faction Relationship
    private void ChangeFactionRelationshipStatus(string[] parameters) {
        if (parameters.Length != 4) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1IDString = parameters[1];
        string faction2IDString = parameters[2];
        string newRelStatusString = parameters[3];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1IDString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2IDString, out faction2ID);

        string faction1Name = parameters[1];
        string faction2Name = parameters[2];

        RELATIONSHIP_STATUS newRelStatus;

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
            newRelStatus = (RELATIONSHIP_STATUS)Enum.Parse(typeof(RELATIONSHIP_STATUS), newRelStatusString, true);
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
        rel.ChangeRelationshipStatus(newRelStatus);

        AddSuccessMessage("Changed relationship status of " + faction1.name + " and " + faction2.name + " to " + rel.relationshipStatus.ToString());
    }
    #endregion



}
