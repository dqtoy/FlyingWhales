using UnityEngine;
using System.Collections;
using ECS;

public class Predator : CharacterTag {
    public Predator(Character character) : base(character, CHARACTER_TAG.PREDATOR) {
        //_tagTasks.Add(new HuntPrey(_character));
    }
}
