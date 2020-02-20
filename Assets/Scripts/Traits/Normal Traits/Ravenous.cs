using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Ravenous : Trait {

        //public ITraitable traitable { get; private set; }
        public Ravenous() {
            name = "Ravenous";
            description = "This is ravenous.";
            type = TRAIT_TYPE.PERSONALITY;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        
        #endregion
    }
}
