using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILeader {
    
    int id { get; }
    string name { get; }
    RACE race { get; }
    GENDER gender { get; }
    Area specificLocation { get; }
    Area homeArea { get; }

    void LevelUp();
}
