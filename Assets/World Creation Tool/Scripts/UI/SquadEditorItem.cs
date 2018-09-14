using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;

public class SquadEditorItem : MonoBehaviour {

    public Squad squad;

    [SerializeField] private InputField squadNameField;
    [SerializeField] private Dropdown emblemBGDropdown;
    [SerializeField] private Dropdown emblemDropdown;
    [SerializeField] private Image colorImage;
    [SerializeField] private ColorPickerControl squadColoPicker;

    public RectTransform leaderContainer;
    public RectTransform membersContainer;

    public void SetSquad(Squad squad) {
        LoadEmblemChoices();
        this.squad = squad;
        squadNameField.text = squad.name;

        emblemBGDropdown.value = Utilities.GetOptionIndex(emblemBGDropdown, CharacterManager.Instance.GetEmblemBGIndex(squad.emblemBG).ToString());
        emblemDropdown.value = Utilities.GetOptionIndex(emblemDropdown, CharacterManager.Instance.GetEmblemIndex(squad.emblem).ToString());
        colorImage.color = squad.squadColor;
        squadColoPicker.CurrentColor = squad.squadColor;
    }

    public void OnLeaderDropped(Transform transform) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.SetLeader(charItem.character);
        }
    }

    public void OnMemberDropped(Transform transform) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.AddMember(charItem.character);
        }
    }

    public void OnSquadNameChange(string newName) {
        squad.SetName(newName);
    }

    public void DeleteSquad() {
        CharacterManager.Instance.DeleteSquad(squad);
    }

    #region Emblem
    private void LoadEmblemChoices() {
        emblemBGDropdown.ClearOptions();
        List<Dropdown.OptionData> emblemBGOptions = new List<Dropdown.OptionData>();
        for (int i = 0; i < CharacterManager.Instance.emblemBGs.Count; i++) {
            EmblemBG currEmblem = CharacterManager.Instance.emblemBGs[i];
            Dropdown.OptionData currData = new Dropdown.OptionData();
            currData.image = currEmblem.frame;
            currData.text = i.ToString();
            emblemBGOptions.Add(currData);
        }
        emblemBGDropdown.AddOptions(emblemBGOptions);

        emblemDropdown.ClearOptions();
        List<Dropdown.OptionData> emblemOptions = new List<Dropdown.OptionData>();
        for (int i = 0; i < CharacterManager.Instance.emblemSymbols.Count; i++) {
            Sprite currEmblemSymbol = CharacterManager.Instance.emblemSymbols[i];
            Dropdown.OptionData currData = new Dropdown.OptionData();
            currData.image = currEmblemSymbol;
            currData.text = i.ToString();
            emblemOptions.Add(currData);
        }
        emblemDropdown.AddOptions(emblemOptions);
    }
    public void SetEmblemBG(int choice) {
        //int chosenBGIndex = System.Int32.Parse(emblemDropdown.options[choice].text);
        squad.SetEmblemBG(CharacterManager.Instance.emblemBGs[choice]);
    }
    public void SetEmblem(int choice) {
        //int chosenBGIndex = System.Int32.Parse(emblemDropdown.options[choice].text);
        squad.SetEmblem(CharacterManager.Instance.emblemSymbols[choice]);
    }
    public void SetSquadColor(Color color) {
        squad.SetSquadColor(color);
    }
    #endregion
}
