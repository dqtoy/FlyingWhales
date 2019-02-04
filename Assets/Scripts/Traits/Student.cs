using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : RelationshipTrait {

    public override string nameInUI {
        get { return "Student: " + targetCharacter.name; }
    }

    public Student(Character target) : base (target){
        name = "Student";
        description = "This character is a student of " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.POSITIVE;
        relType = RELATIONSHIP_TRAIT.STUDENT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
