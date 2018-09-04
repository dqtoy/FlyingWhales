using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Drunk : CharacterTag {
    public Drunk(Character character) : base(character, CHARACTER_TAG.DRUNK) {

    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.AddHours(16);
        SchedulingManager.Instance.AddEntry(gameDate, () => SoberUp());
    }
    #endregion

    private void SoberUp() {
        character.RemoveCharacterAttribute(this);
    }
}
