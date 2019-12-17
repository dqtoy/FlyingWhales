using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

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
        if (collision.gameObject.CompareTag(InnerMapManager.InvisibleToCharacterTag)) {
            return; //object is invisible to character
        }
        IVisibleCollider collidedWith = collision.gameObject.GetComponent<IVisibleCollider>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            if (collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character target = collidedWith.poi as Character;
                if (!target.IsInOwnParty()) {
                    return;
                }
            }
            
            string collisionSummary = parentMarker.name + " collided with " + collidedWith.poi.name;
            if (collidedWith.poi.gridTileLocation == null) {
                return; //ignore, Usually happens if an item is picked up just as this character sees it.
            }
            //when this collides with a poi trigger
            //check if the poi trigger is in the same structure as this
            if (collidedWith.poi.gridTileLocation.structure == parentMarker.character.gridTileLocation.structure || collidedWith.IgnoresStructureDifference()) {
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
    public void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag(InnerMapManager.InvisibleToCharacterTag)) {
            return; //object is invisible to character
        }
        IVisibleCollider collidedWith = collision.gameObject.GetComponent<IVisibleCollider>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
        }
    }
    #endregion
    
    private void NormalEnterHandling(IPointOfInterest poi) {
        Character targetCharacter = null;
        if (poi is Character) {
            targetCharacter = poi as Character;
        }
        parentMarker.AddPOIAsInVisionRange(poi);
        if(targetCharacter != null && parentMarker.character.traitContainer.GetNormalTrait<Trait>("Resting", "Unconscious") == null) {
            parentMarker.AddHostileInRange(targetCharacter);
        } else if (poi is TornadoTileObject && parentMarker.character.traitContainer.GetNormalTrait<Trait>("Elemental Master") == null) {
            parentMarker.AddAvoidInRange(poi);
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
