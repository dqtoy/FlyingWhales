
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNamePlate {

    [SerializeField] private UILabel characterNameLbl;
    [SerializeField] private UI2DSprite roleSprite;

    private Character _character;

    public void ShowNamePlate(Character character) {
        _character = character;
        characterNameLbl.text = string.Empty;
        // if (character.role != null) {
        //     characterNameLbl.text = Utilities.NormalizeStringUpperCaseFirstLetterOnly(character.role.roleType.ToString()) + " ";
        //     roleSprite.sprite2D = CharacterManager.Instance.GetSpriteByRole(character.role.roleType);
        //     roleSprite.gameObject.SetActive(true);
        // } else {
        //     roleSprite.gameObject.SetActive(false);
        // }
        characterNameLbl.text += character.name;
    }

    private void Update() {
        //follow the character's icon
        if (_character != null) {

        }
    }
}
