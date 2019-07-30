using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Succubus : SeducerSummon {

    public Succubus() : base(SUMMON_TYPE.Succubus, GENDER.FEMALE){

    }
    public Succubus(SaveDataCharacter data) : base(data) {
    }
}