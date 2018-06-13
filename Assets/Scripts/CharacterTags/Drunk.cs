using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Drunk : CharacterTag {
    public Drunk(Character character) : base(character, CHARACTER_TAG.DRUNK) {

    }
}
