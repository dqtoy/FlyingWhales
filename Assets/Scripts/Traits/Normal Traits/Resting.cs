using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Resting : Trait {
        public override bool isNotSavable {
            get { return true; }
        }

        private Character _character;
        public Resting() {
            name = "Resting";
            description = "This character is resting.";
            thoughtText = "[Character] is resting.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //hindersMovement = true;
            hindersWitness = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            if (sourceCharacter is Character) {
                _character = sourceCharacter as Character;
                //Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
            }
            base.OnAddTrait(sourceCharacter);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            //Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
            _character = null;
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            RecoverHP();
        }
        #endregion

        private void RecoverHP() {
            _character.HPRecovery(0.02f);
        }
    }
}

