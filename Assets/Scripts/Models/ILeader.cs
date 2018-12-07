﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILeader {
    
    int id { get; }
    string name { get; }
    RACE race { get; }
    ILocation specificLocation { get; }

    void LevelUp();
}
