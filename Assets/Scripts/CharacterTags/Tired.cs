using UnityEngine;
using System.Collections;
using ECS;

public class Tired : CharacterTag {
    public Tired(Character character) : base(character, CHARACTER_TAG.TIRED) {

    }
}
