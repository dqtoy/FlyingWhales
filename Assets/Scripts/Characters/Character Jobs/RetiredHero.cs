using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetiredHero : Job {

    public RetiredHero(CharacterRole role) : base(role) {
        _jobType = CHARACTER_JOB.RETIRED_HERO;
    }
}
