using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObjectComponent : ObjectComponent {
    public bool isInvisible;

    public void CopyDataToCharacterObject(CharacterObj characterObject) {
        characterObject.SetIsInvisible(isInvisible);
    }
}
