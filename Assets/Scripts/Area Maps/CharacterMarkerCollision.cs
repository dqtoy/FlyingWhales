using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    public void OnTriggerEnter2D(Collider2D collision) {
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        //if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == parentMarker.character) {
        //    Debug.Log(this.parentMarker.name + " trigger enter with " + collidedWith.poi.name + ". GO - " + collidedWith.gameObject.name + "Type - " + collidedWith.GetType().ToString());
        //}
        if (collidedWith != null && collidedWith.poi != null 
            && collidedWith.poi != parentMarker.character 
            && collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure) {

            if (collidedWith is GhostCollisionTrigger) {
                GhostCollisionHandling(collidedWith as GhostCollisionTrigger);
            } else {
                parentMarker.AddPOIAsInRange(collidedWith.poi);
                //if (collidedWith.poi is Character) {
                //    HostilityHandling(collidedWith.poi as Character);
                //}
            }
            //Debug.Log(this.parentMarker.name + " trigger enter with " + collidedWith.poi.name);
        }
    }

    public void OnTriggerExit2D(Collider2D collision) {
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null 
            && collidedWith.poi != parentMarker.character) {

            //Debug.Log(this.parentMarker.name + " trigger exit with " + collidedWith.poi.name);
            parentMarker.RemovePOIFromInRange(collidedWith.poi);
        }
    }

    public void OnTriggerStay2D(Collider2D collision) {
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {

            //Debug.Log(this.parentMarker.name + " trigger stay with " + collidedWith.poi.name);
            if (collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure) {
                if (collidedWith is GhostCollisionTrigger) {
                    GhostCollisionHandling(collidedWith as GhostCollisionTrigger);
                    //this is for cases when the actual collision trigger of the poi was destroyed while this marker had it in it's range
                    parentMarker.RemovePOIFromInRange(collidedWith.poi);
                } else {
                    parentMarker.AddPOIAsInRange(collidedWith.poi);
                }
            } else if (collidedWith.gridTileLocation.structure != parentMarker.character.currentStructure) {
                parentMarker.RemovePOIFromInRange(collidedWith.poi);
            }
            
        }
    }

    private void GhostCollisionHandling(GhostCollisionTrigger collidedWith) {
        string ghostCollisionSummary = parentMarker.character.name + " collided with a ghost collider! " + collidedWith.poi.name;
        //when a character collides with a ghost collision trigger
        IAwareness awareness = parentMarker.character.GetAwareness(collidedWith.poi);
        if (awareness != null) { //it will check if it is aware of the associated poi
            //if it is aware of the poi
            ghostCollisionSummary += "\n" + parentMarker.character.name + " is aware of " + collidedWith.poi.name;
            if (awareness.knownGridLocation == collidedWith.gridTileLocation) { //check if the character's known location is the location of this trigger
                //if it is, remove the poi from the characters awareness (Object Missing)
                parentMarker.character.RemoveAwareness(collidedWith.poi);
                ghostCollisionSummary += "\n" + parentMarker.character.name + "'s known location of " + collidedWith.poi.name + " is same as this ghost colliders position, removing it from it's awareness...";
                if (parentMarker.character.currentAction != null && parentMarker.character.currentAction.poiTarget == collidedWith.poi) {
                    //trigger target missing state
                    parentMarker.character.currentAction.ExecuteTargetMissing();
                }
            }
        }
        Debug.Log(ghostCollisionSummary);
    }
    private void HostilityHandling(Character character) {
        if (parentMarker.character.IsHostileWith(character)) {
            if (parentMarker.character.currentAction != null && parentMarker.character.currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC) {
                //if the owner of this collider is already assaulting someone, ignore
                return;
            }
            if (character.currentAction != null && character.currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC) {
                //if the character in question is already assaulting someone, ignore
                return;
            }
        }
    }
}
