using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonStone : SpecialObject {

	public DemonStone() : base (SPECIAL_OBJECT_TYPE.DEMON_STONE){ }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        Minion minion = PlayerManager.Instance.player.CreateNewMinionRandomClass();
        PlayerManager.Instance.player.AddMinion(minion);
    }
    #endregion
}
