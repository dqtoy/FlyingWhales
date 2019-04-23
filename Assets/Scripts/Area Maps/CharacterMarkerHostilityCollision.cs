using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerHostilityCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    #region Triggers
    //public void OnTriggerEnter2D(Collider2D collision) {
    //    POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
    //    if (collidedWith != null && collidedWith.poi != null
    //        && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
    //        && collidedWith.poi != parentMarker.character) {
    //        //&& collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure
    //        parentMarker.AddHostileInRange(collidedWith.poi as Character);
    //    }
    //}
    public void OnTriggerExit2D(Collider2D collision) {
        if (!parentMarker.character.IsInOwnParty()) {
            return;
        }
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null 
            && collidedWith.poi != null
            && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
            && collidedWith.poi != parentMarker.character) {
            Character target = collidedWith.poi as Character;
            if (!target.IsInOwnParty()) {
                return;
            }
            parentMarker.RemoveHostileInRange(collidedWith.poi as Character);
        }
    }
    //public void OnTriggerStay2D(Collider2D collision) {
    //    POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
    //    if (collidedWith != null
    //        && collidedWith.poi != null
    //        && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
    //        && collidedWith.poi != parentMarker.character) {

    //        if (collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure) {
    //            parentMarker.AddHostileInRange(collidedWith.poi as Character);
    //        } else if (collidedWith.gridTileLocation.structure != parentMarker.character.currentStructure) {
    //            parentMarker.RemoveHostileInRange(collidedWith.poi as Character);
    //        }

    //    }
    //}
    #endregion
}
