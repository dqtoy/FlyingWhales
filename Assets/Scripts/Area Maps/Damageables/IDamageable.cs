using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public interface IDamageable {
    string name { get; }
	int currentHP { get; }
    int maxHP { get; }
    ProjectileReceiver projectileReceiver { get; } //the object to be targeted when a character decides to do damage to this object
    LocationGridTile gridTileLocation { get; }

    void AdjustHP(int amount, bool triggerDeath = false, object source = null);
    void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary);
}
