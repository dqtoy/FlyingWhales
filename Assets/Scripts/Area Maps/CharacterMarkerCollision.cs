using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    public void OnTriggerEnter2D(Collider2D collision) {
        POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null 
            && collidedWith.poi != parentMarker.character 
            && collidedWith.poi.gridTileLocation.structure == parentMarker.character.currentStructure) {

            //Debug.Log(this.parentMarker.name + " trigger enter with " + collidedWith.poi.name);
            parentMarker.AddPOIAsInRange(collidedWith.poi);
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
            if (collidedWith.poi.gridTileLocation.structure == parentMarker.character.currentStructure
                && !parentMarker.inRangePOIs.Contains(collidedWith.poi)) {
                parentMarker.AddPOIAsInRange(collidedWith.poi);
            } else if (collidedWith.poi.gridTileLocation.structure != parentMarker.character.currentStructure
                && parentMarker.inRangePOIs.Contains(collidedWith.poi)) {
                parentMarker.RemovePOIFromInRange(collidedWith.poi);
            }
            
        }
    }
}
