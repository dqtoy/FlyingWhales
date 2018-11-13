using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempParty {
    protected List<ICharacter> _icharacters;
    
    #region getters/setters
    public List<ICharacter> icharacters {
        get { return _icharacters; }
    }
    #endregion
}
