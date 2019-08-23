using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerVisionCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    public bool isInitialized;

    public List<IPointOfInterest> poisInRangeButDiffStructure = new List<IPointOfInterest>();

    private void OnDisable() {
        if (parentMarker.inVisionPOIs != null) {
            parentMarker.ClearPOIsInVisionRange();
        }
        if (parentMarker.hostilesInRange != null) {
            parentMarker.ClearHostilesInRange();
        }
        if (parentMarker.avoidInRange != null) {
            parentMarker.ClearAvoidInRange();
        }
    }

    public void Initialize() {
        isInitialized = true;
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public void Reset() {
        isInitialized = false;
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        OnDisable();
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
            
            if (collidedWith is GhostCollisionTrigger) {
                //ignored same structure requirement for ghost collisions
                GhostCollisionHandling(collidedWith as GhostCollisionTrigger);
            } else {
                string collisionSummary = parentMarker.name + " collided with " + collidedWith.poi.name;
                if (collidedWith.poi.gridTileLocation == null) {
                    return; //ignore, Usually happens if an item is picked up just as this character sees it.
                }
                //when this collides with a poi trigger
                //check if the poi trigger is in the same structure as this
                if (collidedWith.poi.gridTileLocation.structure == parentMarker.character.gridTileLocation.structure) {
                    //if it is, just follow the normal procedure when a poi becomes in range
                    collisionSummary += "\n-has same structure as " + parentMarker.character.name + " adding as in range";
                    NormalEnterHandling(collidedWith.poi);
                } else {
                    //if it is not, check both character's structure types

                    //if both characters are in an open space, add them as normal
                    if (collidedWith.poi.gridTileLocation != null && collidedWith.poi.gridTileLocation.structure != null 
                        && parentMarker.character.gridTileLocation != null && parentMarker.character.gridTileLocation.structure != null
                        && collidedWith.poi.gridTileLocation.structure.structureType.IsOpenSpace() && parentMarker.character.gridTileLocation.structure.structureType.IsOpenSpace()) {
                        collisionSummary += "\n-has different structure with " + parentMarker.character.name + " but both are in open space, allowing vision collision.";
                        NormalEnterHandling(collidedWith.poi);
                    }
                    //if not, add the poi to the list of pois in different structures instead
                    //once there, it can only be removed from there if the poi exited this trigger or the poi moved 
                    //to the same structure that this character is in
                    else {
                        collisionSummary += "\n-has different structure with " + parentMarker.character.name + " queuing...";
                        AddPOIAsInRangeButDifferentStructure(collidedWith.poi);
                    }
                }
                //if (collidedWith.poi is Character) {
                //    Debug.Log(collisionSummary);
                //}
            }
            
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
            if (collidedWith.poi is Character) {
                Character targetCharacter = collidedWith.poi as Character;
                Invisible invisible = targetCharacter.GetNormalTrait("Invisible") as Invisible;
                if (invisible != null && !invisible.charactersThatCanSee.Contains(parentMarker.character)) {
                    invisible.RemoveInRangeOfVisionCharacter(parentMarker.character);
                }
            }
        }
    }
    #endregion

    private void GhostCollisionHandling(GhostCollisionTrigger collidedWith) {
        //string ghostCollisionSummary = parentMarker.character.name + " collided with a ghost collider! " + collidedWith.poi.name;
        //when a character collides with a ghost collision trigger
        if (parentMarker.character.HasAwareness(collidedWith.poi)) { //it will check if it is aware of the associated poi
            //if it is aware of the poi
            //ghostCollisionSummary += "\n" + parentMarker.character.name + " is aware of " + collidedWith.poi.name;
            parentMarker.character.RemoveAwareness(collidedWith.poi);
            //ghostCollisionSummary += "\n" + parentMarker.character.name + "'s known location of " + collidedWith.poi.name + " is same as this ghost colliders position, removing it from it's awareness...";
            if (parentMarker.character.currentAction != null && parentMarker.character.currentAction.poiTarget == collidedWith.poi) {
                //trigger target missing state
                parentMarker.character.currentAction.ExecuteTargetMissing();
            }
        }
        //Debug.Log(ghostCollisionSummary);
    }
    public bool ChatHandling(Character targetCharacter) {
        if (targetCharacter.isDead
            || targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)
            || parentMarker.character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)
            || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT)
            || (parentMarker.character.stateComponent.currentState != null && parentMarker.character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT)
            || targetCharacter.role.roleType == CHARACTER_ROLE.BEAST
            || parentMarker.character.role.roleType == CHARACTER_ROLE.BEAST
            || targetCharacter.faction == PlayerManager.Instance.player.playerFaction
            || parentMarker.character.faction == PlayerManager.Instance.player.playerFaction) {
            return false;
        }
        if(!parentMarker.character.IsHostileWith(targetCharacter)) {
            int roll = UnityEngine.Random.Range(0, 100);
            int chance = 12;

            if (roll < chance) {
                if (!parentMarker.character.isChatting && !targetCharacter.isChatting) {
                    parentMarker.character.ChatCharacter(targetCharacter);
                    return true;
                }
            }
        }
        return false;
    }
    public bool ForceChatHandling(Character targetCharacter) {
        if (targetCharacter.isDead
            || targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)
            || parentMarker.character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)
            || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT)
            || (parentMarker.character.stateComponent.currentState != null && parentMarker.character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT)
            || targetCharacter.role.roleType == CHARACTER_ROLE.BEAST
            || parentMarker.character.role.roleType == CHARACTER_ROLE.BEAST
            || targetCharacter.faction == PlayerManager.Instance.player.playerFaction
            || parentMarker.character.faction == PlayerManager.Instance.player.playerFaction) {
            return false;
        }
        if (!parentMarker.character.IsHostileWith(targetCharacter)) {
            if (!parentMarker.character.isChatting && !targetCharacter.isChatting) {
                parentMarker.character.ChatCharacter(targetCharacter);
                return true;
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
        Character targetCharacter = null;
        if (poi is Character) {
            targetCharacter = poi as Character;
            Invisible invisible = targetCharacter.GetNormalTrait("Invisible") as Invisible;
            if (invisible != null && !invisible.charactersThatCanSee.Contains(parentMarker.character)) {
                invisible.AddInRangeOfVisionCharacter(parentMarker.character);
                return;
            }
        }
        parentMarker.AddPOIAsInVisionRange(poi);
        if (GameManager.Instance.gameHasStarted) {
            if (parentMarker.character.stateComponent.currentState != null) {
                if (!parentMarker.character.stateComponent.currentState.OnEnterVisionWith(poi)) {
                    if (targetCharacter != null) {
                        if (!parentMarker.AddHostileInRange(targetCharacter)) {
                            if (!parentMarker.character.CreateJobsOnEnterVisionWith(targetCharacter, true)) {
                                ChatHandling(targetCharacter);
                            }
                        }
                    } else {
                        parentMarker.character.CreateJobsOnEnterVisionWith(poi, true);
                    }
                }
            } else {
                if (targetCharacter != null) {
                    if (!parentMarker.AddHostileInRange(targetCharacter)) {
                        if (!parentMarker.character.CreateJobsOnEnterVisionWith(targetCharacter, true)) {
                            ChatHandling(targetCharacter);
                        }
                    }
                } else {
                    parentMarker.character.CreateJobsOnEnterVisionWith(poi, true);
                }
            }
        }
    }

    #region Different Structure Handling
    public void AddPOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Add(poi);
    }
    public void RemovePOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Remove(poi);
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
         //if the character that arrived at the new structure is in this character different structure list
         //check if that character now has the same structure as this character,
        if (poisInRangeButDiffStructure.Contains(character) && structure == parentMarker.character.currentStructure) {
            //if it does, add as normal
            NormalEnterHandling(character);
            RemovePOIAsInRangeButDifferentStructure(character);
        }
        //else if the character that arrived at the new structure is in this character's vision list and the character no longer has the same structure as this character, 
        else if (parentMarker.inVisionCharacters.Contains(character) && structure != parentMarker.character.currentStructure) {
            //if both characters are in open space, do not remove from vision
            if (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()) {
                return;
            }
            //remove from vision and hostile range
            parentMarker.RemovePOIFromInVisionRange(character);
            //parentMarker.RemoveHostileInRange(character);
            //parentMarker.RemoveAvoidInRange(character);
            AddPOIAsInRangeButDifferentStructure(character);
        }
        //if the character that changed structures is this character
        else if (character.id == parentMarker.character.id) {
            //check all pois that were in different structures and revalidate them
            List<IPointOfInterest> pois = new List<IPointOfInterest>(poisInRangeButDiffStructure);
            for (int i = 0; i < pois.Count; i++) {
                IPointOfInterest poi = pois[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    RemovePOIAsInRangeButDifferentStructure(poi);
                } else if (poi.gridTileLocation.structure == parentMarker.character.currentStructure
                    || (poi.gridTileLocation.structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    NormalEnterHandling(poi);
                    RemovePOIAsInRangeButDifferentStructure(poi);
                }
            }
            //also check all pois in vision
            pois = new List<IPointOfInterest>(parentMarker.inVisionPOIs);
            for (int i = 0; i < pois.Count; i++) {
                IPointOfInterest poi = pois[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    parentMarker.RemovePOIFromInVisionRange(poi);
                    //if (poi is Character) {
                    //    //parentMarker.RemoveHostileInRange(poi as Character);
                    //    parentMarker.RemoveAvoidInRange(poi as Character);
                    //}
                } else if (poi.gridTileLocation.structure != parentMarker.character.currentStructure 
                    && (!poi.gridTileLocation.structure.structureType.IsOpenSpace() || !parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    //if the character in vision no longer has the same structure as the character, and at least one of them is not in an open space structure
                    parentMarker.RemovePOIFromInVisionRange(poi);
                    //if (poi is Character) {
                    //    //parentMarker.RemoveHostileInRange(poi as Character);
                    //    parentMarker.RemoveAvoidInRange(poi as Character);
                    //}
                    AddPOIAsInRangeButDifferentStructure(poi);
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
