using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : Job {

    public Worker(Character character) : base(character, JOB.WORKER) {
    }
}
