using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterMarkerAnimationListener : MonoBehaviour {

    [SerializeField] private CharacterMarker parentMarker;

	public void OnAttackHit() {
        //Debug.Log(parentMarker.name + " finished attack!");
        //if (parentMarker.isInCombatTick && parentMarker.currentlyEngaging != null) {
        //    parentMarker.currentlyEngaging.marker.Play("Hit");
        //    parentMarker.currentlyEngaging.marker.lastHitBy = parentMarker.character;
        //}
    }

    public void OnHitFinished() {
        //Debug.Log("Hit animation of " + parentMarker.character.name + " finished");
        //parentMarker.lastHitBy.marker.CombatTick();
    }
}
