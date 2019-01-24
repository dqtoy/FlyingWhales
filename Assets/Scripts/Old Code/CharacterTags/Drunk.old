using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Drunk : CharacterAttribute {
    public Drunk() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DRUNK) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.AddDays(16);
        SchedulingManager.Instance.AddEntry(gameDate, () => SoberUp());
    }
    #endregion

    private void SoberUp() {
        character.RemoveAttribute(this);
    }
}
