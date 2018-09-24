using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDialogMenu : UIMenu {

    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI characterDialogLbl;
    [SerializeField] private ScrollRect choicesScrollRect;
    [SerializeField] private TextTyper textTyper;

    private Button[] choiceBtns;

    internal override void Initialize() {
        choiceBtns = Utilities.GetComponentsInDirectChildren<Button>(choicesScrollRect.content.gameObject);
        Messenger.AddListener<ECS.Character, string, List<CharacterDialogChoice>>(Signals.SHOW_CHARACTER_DIALOG, ShowCharacterDialog);
    }

    public void ShowCharacterDialog(ECS.Character character, string message, List<CharacterDialogChoice> choices) {
        characterPortrait.GeneratePortrait(character, 256, true);
        characterDialogLbl.text = message;
        if (choices.Count > 3) {
            throw new System.Exception("There are too many choices provided!");
        }
        for (int i = 0; i < choiceBtns.Length; i++) {
            Button currButton = choiceBtns[i];
            CharacterDialogChoice currChoice = choices.ElementAtOrDefault(i);
            if (currChoice == null) {
                currButton.gameObject.SetActive(false);
            } else {
                currButton.gameObject.SetActive(true);
                currButton.GetComponentInChildren<TextMeshProUGUI>().text = currChoice.buttonTitle;
                currButton.onClick.RemoveAllListeners();
                currButton.onClick.AddListener(currChoice.onClickAction);
                currButton.onClick.AddListener(CloseMenu);
            }
        }
        this.gameObject.SetActive(true);
        GameManager.Instance.SetPausedState(true);
        UIManager.Instance.SetTimeControlsState(false);
        textTyper.Execute();
    }

    public override void CloseMenu() {
        UIManager.Instance.SetTimeControlsState(true);
        //GameManager.Instance.SetPausedState(false);
        base.CloseMenu();
    }
}
