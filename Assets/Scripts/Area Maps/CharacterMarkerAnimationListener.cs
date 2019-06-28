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
            if (parentMarker.character.characterClass.rangeType == RANGE_TYPE.RANGED) {
                CreateProjectile(combatState.currentClosestHostile);
            } else {
                combatState.OnAttackHit(combatState.currentClosestHostile);
            }
        }
    }

    private void CreateProjectile(Character target) {
        if (currentProjectile != null) {
            return; //only 1 projectile at a time!
        }
        //Create projectile here and set the on hit action to combat state OnAttackHit
        GameObject projectileGO = GameObject.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity, parentMarker.projectileParent);
        projectileGO.transform.localPosition = Vector3.zero;
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        projectile.SetTarget(target.marker.transform);
        projectile.onHitAction = OnProjectileHit;
        currentProjectile = projectileGO;
    }

    /// <summary>
    /// Called when an attack that this character does, hits another character.
    /// </summary>
    /// <param name="character">The character that was hit.</param>
    private void OnProjectileHit(Character character) {
        currentProjectile = null;
        if (parentMarker.character.stateComponent.currentState is CombatState) {
            CombatState combatState = parentMarker.character.stateComponent.currentState as CombatState;
            combatState.OnAttackHit(character);
        } else {
            string attackSummary = GameManager.Instance.TodayLogString() + parentMarker.character.name + " hit " + character.name + ", outside of combat state";
            character.OnHitByAttackFrom(parentMarker.character, ref attackSummary);
            parentMarker.character.PrintLogIfActive(attackSummary);
        }
    }
}
