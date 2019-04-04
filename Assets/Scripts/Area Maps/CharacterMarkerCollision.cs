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
                if (collidedWith.poi is Character && GameManager.Instance.gameHasStarted) {
                    Character targetCharacter = collidedWith.poi as Character;
                    if(!HostilityHandling(targetCharacter)) {
                        ChatHandling(targetCharacter);
                    }
                }
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
    private bool HostilityHandling(Character character) {
        if (!parentMarker.character.isFactionless && parentMarker.character.role.roleType != CHARACTER_ROLE.SOLDIER
            && parentMarker.character.role.roleType != CHARACTER_ROLE.ADVENTURER) {
            //if this character is part of a faction, it can only assault if he/she is a soldier or adventurer
            return false;
        }
        //if (parentMarker.character.role.roleType != CHARACTER_ROLE.SOLDIER 
        //    && parentMarker.character.role.roleType != CHARACTER_ROLE.ADVENTURER) {
        //    return;
        //}
        if (parentMarker.character.IsHostileWith(character)) {
            string summary = GameManager.Instance.TodayLogString() + parentMarker.character.name + " hostility handling summary with " + character.name;
            if (parentMarker.character.lastAssaultedCharacter != null && parentMarker.character.lastAssaultedCharacter == character) {
                summary += "\n" + parentMarker.character.name + " already assaulted " + character.name + ", ignoring...";
                Debug.Log(summary);
                return false;
            }

            if (parentMarker.character.isWaitingForInteraction > 0) {
                summary += "\n" + parentMarker.character.name + " is waiting for someone. Ignoring " + character.name;
                Debug.Log(summary);
                return false;
            }
            if (parentMarker.character.doNotDisturb > 0) {
                summary += "\n" + parentMarker.character.name + " has a disabler trait. Ignoring " + character.name;
                Debug.Log(summary);
                return false;
            }
            if (parentMarker.character.hasAssaultPlan ||
                (parentMarker.character.currentAction != null && (parentMarker.character.currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC
                || parentMarker.character.HasPlanWithType(INTERACTION_TYPE.ASSAULT_ACTION_NPC)))) {
                //if the owner of this collider is already assaulting someone, ignore
                summary += "\n" + parentMarker.character.name + " is already assaulting someone. Ignoring " + character.name;
                Debug.Log(summary);
                return false;
            }
            if (character.hasAssaultPlan ||
                (character.currentAction != null && (character.currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC
                || character.HasPlanWithType(INTERACTION_TYPE.ASSAULT_ACTION_NPC)))) {
                //if the character in question is already assaulting someone, ignore
                summary += "\n" + character.name + " is already assaulting someone. Ignoring " + character.name;
                Debug.Log(summary);
                return false;
            }
            //if (parentMarker.character.id == 1) {
                summary += "\n" + parentMarker.character.name + " will assault " + character.name;
                parentMarker.character.AssaultCharacter(character);
            //}
            Debug.Log(summary);
            return true;
        }
        return false;
    }
    private bool ChatHandling(Character targetCharacter) {
        if(!parentMarker.character.isChatting && targetCharacter.isChatting) {
            parentMarker.character.ChatCharacter(targetCharacter);
            return true;
        }
        return false;
    }
}
