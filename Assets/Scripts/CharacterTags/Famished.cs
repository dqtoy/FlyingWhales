using UnityEngine;
using System.Collections;
using ECS;

public class Famished : CharacterTag {
    public Famished(Character character) : base(character, CHARACTER_TAG.FAMISHED) {

    }
}
