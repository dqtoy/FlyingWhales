using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSquadEditorItem : MonoBehaviour {

    public Character character;

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Text characterNameLbl;
    [SerializeField] private Text otherInfoLbl;

    public void SetCharacter(Character character) {
        this.character = character;
        portrait.GeneratePortrait(character, 64);
    }

    private void Update() {
        if (character != null) {
            characterNameLbl.text = character.name;
            if (character.isFactionless) {
                characterNameLbl.text += "(Neutral)";
            } else {
                characterNameLbl.text += "(" + character.faction.name + ")";
            }
            otherInfoLbl.text = character.role.roleType.ToString() + "/" + character.characterClass.className;
        }
    }
}
