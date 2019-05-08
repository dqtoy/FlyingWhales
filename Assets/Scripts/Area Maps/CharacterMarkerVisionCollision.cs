using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerVisionCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    private List<IPointOfInterest> poisInRangeButDiffStructure = new List<IPointOfInterest>();

    private void OnEnable() {
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    private void OnDisable() {
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }

    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        if (!parentMarker.character.IsInOwnParty()) {
            return;
        }
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            if (collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character target = collidedWith.poi as Character;
                if (!target.IsInOwnParty()) {
                    return;
                }
            }
            string collisionSummary = parentMarker.name + " collided with " + collidedWith.poi.name;
            if (collidedWith is GhostCollisionTrigger) {
                //ignored same structure requirement for ghost collisions
                GhostCollisionHandling(collidedWith as GhostCollisionTrigger);
            } else {
                //when this collides with a poi trigger
                //check if the poi trigger is in the same structure as this
                if (collidedWith.poi.gridTileLocation.structure == parentMarker.character.gridTileLocation.structure) {
                    //if it is, just follow the normal procedure when a poi becomes in range
                    collisionSummary += "\n-has same structure as " + parentMarker.character.name + " adding as in range";
                    NormalEnterHandling(collidedWith.poi);
                } else {
                    //if it is not, add the poi to the list of pois in different structures instead
                    //once there, it can only be removed from there if the poi exited this trigger or the poi moved 
                    //to the same structure that this character is in
                    collisionSummary += "\n-has different structure with " + parentMarker.character.name + " queuing...";
                    AddPOIAsInRangeButDifferentStructure(collidedWith.poi);
                }
            }
            //Debug.Log(collisionSummary);
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (!parentMarker.character.IsInOwnParty()) {
            return;
        }
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            if (collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character target = collidedWith.poi as Character;
                if (!target.IsInOwnParty()) {
                    return;
                }
            }
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
        }
    }
    //public void OnTriggerStay2D(Collider2D collision) {
    //    POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
    //    if (collidedWith != null && collidedWith.poi != null
    //        && collidedWith.poi != parentMarker.character) {

    //        //Debug.Log(this.parentMarker.name + " trigger stay with " + collidedWith.poi.name);
    //        if (collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure) {
    //            if (collidedWith is GhostCollisionTrigger) {
    //                GhostCollisionHandling(collidedWith as GhostCollisionTrigger);
    //                //this is for cases when the actual collision trigger of the poi was destroyed while this marker had it in it's range
    //                parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
    //            } else {
    //                parentMarker.AddPOIAsInVisionRange(collidedWith.poi);
    //            }
    //        } else if (collidedWith.gridTileLocation.structure != parentMarker.character.currentStructure) {
    //            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
    //        }
    //    }
    //}
    #endregion

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
    private bool ChatHandling(Character targetCharacter) {
        if(targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER) 
            || parentMarker.character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)
            || (targetCharacter.stateComponent.currentState != null && (targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.FLEE 
            || targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE))
            || (parentMarker.character.stateComponent.currentState != null && (parentMarker.character.stateComponent.currentState.characterState == CHARACTER_STATE.FLEE
            || parentMarker.character.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE))) {
            return false;
        }
        if(!parentMarker.character.IsHostileWith(targetCharacter)) {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                if (!parentMarker.character.isChatting && !targetCharacter.isChatting) {
                    parentMarker.character.ChatCharacter(targetCharacter);
                    return true;
                }
            }
        }
        return false;
    }
    //private void NormalCollisionHandling(IPointOfInterest poi) {
    //    if(poi is Character) {
    //        Character targetCharacter = poi as Character;
    //        if(!parentMarker.AddHostileInRange(targetCharacter)) {
    //            ChatHandling(targetCharacter);
    //        }
    //    }
    //}
    private void NormalEnterHandling(IPointOfInterest poi) {
        parentMarker.AddPOIAsInVisionRange(poi);
        if (GameManager.Instance.gameHasStarted) {
            if (parentMarker.character.stateComponent.currentState != null) {
                if (!parentMarker.character.stateComponent.currentState.OnEnterVisionWith(poi)) {
                    if (poi is Character) {
                        Character targetCharacter = poi as Character;
                        if (!targetCharacter.isDead) {
                            if (!parentMarker.AddHostileInRange(targetCharacter)) {
                                if (!parentMarker.character.CreateJobsOnEnterVisionWith(targetCharacter)) {
                                    ChatHandling(targetCharacter);
                                }
                            }
                        }
                    }
                }
            } else {
                if (poi is Character) {
                    Character targetCharacter = poi as Character;
                    if (!targetCharacter.isDead) {
                        if (!parentMarker.AddHostileInRange(targetCharacter)) {
                            if (!parentMarker.character.CreateJobsOnEnterVisionWith(targetCharacter)) {
                                ChatHandling(targetCharacter);
                            }
                        }
                    }
                }
            }
        }
    }

    #region Different Structure Handling
    private void AddPOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Add(poi);
    }
    private void RemovePOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Remove(poi);
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (poisInRangeButDiffStructure.Contains(character) && structure == parentMarker.character.currentStructure) {
            NormalEnterHandling(character);
            RemovePOIAsInRangeButDifferentStructure(character);
        } else if (character.id == parentMarker.character.id) {
                //if the character that changed structure is the one that changed structures
                //check all pois that were in different structures and revalidate them
                for (int i = 0; i < poisInRangeButDiffStructure.Count; i++) {
                    IPointOfInterest poi = poisInRangeButDiffStructure[i];
                    if (poi.gridTileLocation == null) {
                        RemovePOIAsInRangeButDifferentStructure(poi);
                    } else if (poi.gridTileLocation.structure == parentMarker.character.currentStructure) {
                        NormalEnterHandling(poi);
                        RemovePOIAsInRangeButDifferentStructure(poi);
                    }
                }
            }
        }
    #endregion

    [ContextMenu("Log Diff Struct")]
    public void LogCharactersInDifferentStructures() {
        string summary = parentMarker.character.name + "'s diff structure pois";
        for (int i = 0; i < poisInRangeButDiffStructure.Count; i++) {
            summary += "\n" + poisInRangeButDiffStructure[i].name;
        }
        Debug.Log(summary);
    }

    #region Utilities
    public void OnDeath() {
        poisInRangeButDiffStructure.Clear();
        OnDisable();
    }
    #endregion
}
