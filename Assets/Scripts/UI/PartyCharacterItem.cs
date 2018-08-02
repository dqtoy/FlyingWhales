﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyCharacterItem : PooledObject {

    private ICharacter character;

    [SerializeField] private Image bg;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private AffiliationsObject affiliations;

    public void SetCharacter(ICharacter character) {
        this.character = character;
        portrait.GeneratePortrait(character, IMAGE_SIZE.X64, true, true);
        nameLbl.text = character.name;
        if (character is ECS.Character) {
            lvlClassLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
            affiliations.SetDisablePartyState(true);
            affiliations.Initialize(character as ECS.Character);
            affiliations.gameObject.SetActive(true);
            lvlClassLbl.gameObject.SetActive(true);
        } else {
            lvlClassLbl.gameObject.SetActive(false);
            affiliations.gameObject.SetActive(false);
        }
    }

    public void SetBGColor(Color color) {
        bg.color = color;
    }
}