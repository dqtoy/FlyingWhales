using UnityEngine;
using System.Collections;
using ECS;

public class Depressed : CharacterTag {
    public Depressed(Character character) : base(character, CHARACTER_TAG.DEPRESSED) {

    }
}
