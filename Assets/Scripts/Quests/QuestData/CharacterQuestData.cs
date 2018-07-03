using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterQuestData {

    protected Quest _parentQuest;

    public CharacterQuestData(Quest parentQuest) {
        _parentQuest = parentQuest;
    }
}
