using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;

public class TraderAvatar : CitizenAvatar {

    #region Behaviour Tree Tasks
    [Task]
    private bool IsTraderDead() {
        return this.citizenRole.citizen.isDead;
    }

    [Task]
    private bool IsTradeEventDone() {
        return !this.citizenRole.gameEventInvolvedIn.isActive;
    }

    [Task]
    private bool HasTraderReachedTarget() {
        if (this.citizenRole.location == this.citizenRole.targetLocation) {
            this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
            return true;
        }
        return false;
    }

//    [Task]
//    private bool HasTraderCollidedWithHostileGeneral() {
//        if (this.collidedWithHostile) {
//            this.collidedWithHostile = false;
//            this._trader.tradeEvent.KillTrader();
//            return true;
//        }
//        return false;
//    }

    [Task]
    private void MoveToNextTile() {
        Move();
        Task.current.Succeed();
    }
    #endregion


}
