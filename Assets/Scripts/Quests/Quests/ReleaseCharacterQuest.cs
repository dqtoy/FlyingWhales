using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ReleaseCharacterQuest : Quest {
    public ReleaseCharacterQuest() : base(QUEST_TYPE.RELEASE_CHARACTER) {
    }

    #region overrides
    public override CharacterAction GetQuestAction(Character character, CharacterQuestData data) {
        if (true) { //if current power is greater than or equal to Required Power
            //check if there are hostiles along the path
        }
        return base.GetQuestAction(character,data);
    }
    #endregion
}
