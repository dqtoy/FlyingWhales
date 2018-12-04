using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyCharacterItem : PooledObject {

    private Character character;

    [SerializeField] private Image bg;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private AffiliationsObject affiliations;

    public void SetCharacter(Character character) {
        this.character = character;
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        lvlClassLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
        affiliations.SetDisablePartyState(true);
        affiliations.Initialize(character);
        affiliations.gameObject.SetActive(true);
        lvlClassLbl.gameObject.SetActive(true);
    }

    public void SetBGColor(Color color) {
        bg.color = color;
    }
}
