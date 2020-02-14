using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Incubus : SeducerSummon {

    public const string ClassName = "Incubus";
    
    public Incubus() : base (SUMMON_TYPE.Incubus, GENDER.MALE) {

    }
    public Incubus(SaveDataCharacter data) : base(data) {
    }
    public override string GetClassForRole(CharacterRole role) {
        return ClassName;
    }
}