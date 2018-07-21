using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSquadEditorItem : MonoBehaviour {

    public ECS.Character character;

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Text characterNameLbl;
    [SerializeField] private Text otherInfoLbl;

    public void SetCharacter(ECS.Character character) {
        this.character = character;
    }

    private void Update() {
        if (character != null) {
            characterNameLbl.text = character.name;
            otherInfoLbl.text = character.role.roleType.ToString() + "/" + character.characterClass.className;
        }
    }
}
