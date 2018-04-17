using UnityEngine;
using System.Collections;
using ECS;

public class Exhausted : CharacterTag {
    public Exhausted(Character character) : base(character, CHARACTER_TAG.EXHAUSTED) {

    }
}
