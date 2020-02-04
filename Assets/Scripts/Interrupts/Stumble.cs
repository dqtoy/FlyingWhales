using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stumble : Interrupt {
        public Stumble() : base(INTERRUPT.Stumble) {
            duration = 2;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            int randomHpToLose = UnityEngine.Random.Range(1, 6);
            float percentMaxHPToLose = randomHpToLose / 100f;
            int actualHPToLose = Mathf.CeilToInt(actor.maxHP * percentMaxHPToLose);
            Debug.Log("Stumble of " + actor.name + " percent: " + percentMaxHPToLose + ", max hp: " + actor.maxHP + ", lost hp: " + actualHPToLose);
            actor.AdjustHP(-actualHPToLose);
            if (actor.currentHP <= 0) {
                actor.Death("Stumble");
            }
            return true;
        }
        #endregion
    }
}