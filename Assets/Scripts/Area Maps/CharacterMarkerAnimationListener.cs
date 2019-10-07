using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterMarkerAnimationListener : MonoBehaviour {

    [SerializeField] private CharacterMarker parentMarker;

    [SerializeField] private GameObject projectilePrefab;
    private GameObject currentProjectile;

    public void OnAttackExecuted() {
        //Debug.Log(parentMarker.name + " attacked!");
        if (parentMarker.character.stateComponent.currentState is CombatState) {
            CombatState combatState = parentMarker.character.stateComponent.currentState as CombatState;
            combatState.isExecutingAttack = false;
            if (parentMarker.character.characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (parentMarker.character.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                    CreateMagicalHit(combatState.currentClosestHostile, combatState);
                } else {
                    CreateProjectile(combatState.currentClosestHostile, combatState);
                }
            } else {
                combatState.OnAttackHit(combatState.currentClosestHostile);
            }
        }
    }

    public void OnAttackAnimationTriggered() {
        StartCoroutine(CheckAttackExecuted());
    }
    private IEnumerator CheckAttackExecuted() {
        yield return new WaitForSeconds(parentMarker.attackExecutedTime);
        OnAttackExecuted();
    }

    private void CreateProjectile(IPointOfInterest target, CombatState state) {
        if (currentProjectile != null) {
            return; //only 1 projectile at a time!
        }
        if (target.isDead) {
            return;
        }
        //Create projectile here and set the on hit action to combat state OnAttackHit
        GameObject projectileGO = GameObject.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity, parentMarker.projectileParent);
        projectileGO.transform.localPosition = Vector3.zero;
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        projectile.SetTarget(target.projectileReciever.transform, target, state);
        projectile.onHitAction = OnProjectileHit;
        currentProjectile = projectileGO;
    }
    private void CreateMagicalHit(IPointOfInterest target, CombatState state) {
        GameManager.Instance.CreateFireEffectAt(target);
        state.OnAttackHit(target);
        //if (parentMarker.character.stateComponent.currentState is CombatState) {
        //    CombatState combatState = parentMarker.character.stateComponent.currentState as CombatState;
        //    combatState.OnAttackHit(target);
        //} 
        //else {
        //    string attackSummary = GameManager.Instance.TodayLogString() + parentMarker.character.name + " hit " + target.name + ", outside of combat state";
        //    target.OnHitByAttackFrom(parentMarker.character, null, ref attackSummary);
        //    parentMarker.character.PrintLogIfActive(attackSummary);
        //}
    }

    /// <summary>
    /// Called when an attack that this character does, hits another character.
    /// </summary>
    /// <param name="target">The character that was hit.</param>
    /// <param name="fromState">The projectile was created from this combat state.</param>
    private void OnProjectileHit(IPointOfInterest target, CombatState fromState) {
        currentProjectile = null;
        //fromState.OnAttackHit(character);
        if (parentMarker.character.stateComponent.currentState is CombatState) {
            CombatState combatState = parentMarker.character.stateComponent.currentState as CombatState;
            combatState.OnAttackHit(target);
        } else {
            string attackSummary = GameManager.Instance.TodayLogString() + parentMarker.character.name + " hit " + target.name + ", outside of combat state";
            target.OnHitByAttackFrom(parentMarker.character, fromState, ref attackSummary);
            parentMarker.character.PrintLogIfActive(attackSummary);
        }
    }
}
