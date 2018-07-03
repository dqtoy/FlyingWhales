using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ReleaseCharacterQuest : Quest {
    public ReleaseCharacterQuest() : base(QUEST_TYPE.RELEASE_CHARACTER) {
    }

    #region overrides
    public override CharacterAction GetQuestAction(Character character, CharacterQuestData data) {

        return base.GetQuestAction(character,data);
    }
    #endregion
}
