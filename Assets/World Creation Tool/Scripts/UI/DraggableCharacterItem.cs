﻿using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraggableCharacterItem : MonoBehaviour {

    public Character character { get; private set; }

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Text nameLbl;
    [SerializeField] private Text roleClassLbl;

    public void SetCharacter(Character character) {
        this.character = character;
    }

    private void Update() {
        if (character != null) {
            portrait.GeneratePortrait(character, 64);
            nameLbl.text = character.name;
            if (character.faction == null) {
                nameLbl.text += "(Factionless)";
            } else {
                nameLbl.text += "(" + character.faction.name + ")";
            }
            roleClassLbl.text = character.role.roleType.ToString() + "/" + character.characterClass.className;
        }
    }

}