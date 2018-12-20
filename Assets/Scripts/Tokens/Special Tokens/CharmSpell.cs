using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmSpell : SpecialToken {

    public CharmSpell() : base(SPECIAL_TOKEN.CHARM_SPELL) {
        quantity = 4;
    }
}
