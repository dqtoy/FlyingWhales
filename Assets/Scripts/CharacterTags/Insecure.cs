using UnityEngine;
using System.Collections;
using ECS;

public class Insecure : CharacterTag {
    public Insecure(Character character) : base(character, CHARACTER_TAG.INSECURE) {

    }
}
