using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explorer : Job {

    private int _currentInteractionTick;
    private int _usedMonthTick;

    public Explorer(Character character) : base(character, JOB.EXPLORER) { }
}
