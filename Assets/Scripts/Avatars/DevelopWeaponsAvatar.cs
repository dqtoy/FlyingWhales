using UnityEngine;
using System.Collections;

public class DevelopWeaponsAvatar : GameEventAvatar {

    private void OnTriggerEnter2D(Collider2D other) {
        //if (other.tag == "Avatar" || other.tag == "General") {
        //    CitizenAvatar citizenToTrigger = other.gameObject.GetComponent<CitizenAvatar>();
        //    DevelopWeapons developWeaponsEvent = (DevelopWeapons)gameEvent;
        //    if(citizenToTrigger != null) {
        //        Kingdom kingdomOfClaimant = citizenToTrigger.citizenRole.citizen.city.kingdom;
        //        if(kingdomOfClaimant.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH) 
        //            || kingdomOfClaimant.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
        //            developWeaponsEvent.ClaimWeapon(citizenToTrigger.citizenRole.citizen.city.kingdom);
        //        }
        //    }
        //}
    }
}
