using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemOnCharacter : Interaction {

    private SpecialToken _tokenToBeUsed;

    public UseItemOnCharacter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.USE_ITEM_ON_CHARACTER, 0) {
        _name = "Use Item On Character";
    }

    public void SetItemToken(SpecialToken specialToken) {
        _tokenToBeUsed = specialToken;
    }
}
