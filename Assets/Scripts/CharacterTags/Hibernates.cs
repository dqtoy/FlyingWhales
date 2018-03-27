using UnityEngine;
using System.Collections;
using ECS;

public class Hibernates : CharacterTag {
    public Hibernates(Character character) : base(character, CHARACTER_TAG.HIBERNATES) {
        _tagTasks.Add(new Hibernate(_character));
    }
}
