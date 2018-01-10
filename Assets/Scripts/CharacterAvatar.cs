using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;

public class CharacterAvatar : PooledObject{

    private List<Character> _characters;

    internal virtual void Init(Role citizenRole) {
        //this.citizenRole = citizenRole;
        //this.citizenRole.citizenAvatar = this;
        //this.citizenID = citizenRole.citizen.id;
        //this.citizenName = citizenRole.citizen.name;
        //this.roleType = citizenRole.ToString();
        //this.direction = DIRECTION.LEFT;
        //this.citizenRole.location.EnterCitizen(this.citizenRole.citizen);
        //this.smoothMovement.onMoveFinished += OnMoveFinished;
        //visibleTiles = new List<HexTile>();
        //childObjects = Utilities.GetComponentsInDirectChildren<Transform>(this.gameObject);
        //SetHasArrivedState(false);
        //if (citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
        //    SetAvatarState(true);
        //} else {
        //    SetAvatarState(false);
        //}
        //this.emblem.SetEmblem(this.citizenRole.citizen.city.kingdom);
        //ResetValues();
        ////      AddBehaviourTree();
        //UpdateUI();
        ////		StartMoving ();
    }
}
