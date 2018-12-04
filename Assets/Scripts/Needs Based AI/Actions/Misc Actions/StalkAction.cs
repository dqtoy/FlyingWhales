using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StalkAction : CharacterAction {

    public StalkAction() : base(ACTION_TYPE.STALK) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);

        ICharacterObject characterObject = targetObject as ICharacterObject;
        Log stalkerLog = new Log(GameManager.Instance.Today(), "CharacterActions", "StalkAction", "start_stalker");
        stalkerLog.AddToFillers(iparty.mainCharacter, iparty.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stalkerLog.AddToFillers(characterObject.iparty.mainCharacter, characterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        iparty.mainCharacter.AddHistory(stalkerLog);

        Log stalkeeLog = new Log(GameManager.Instance.Today(), "CharacterActions", "StalkAction", "start_stalkee");
        stalkeeLog.AddToFillers(characterObject.iparty.mainCharacter, characterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stalkeeLog.AddToFillers(iparty.mainCharacter, iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        characterObject.iparty.mainCharacter.AddHistory(stalkeeLog);
    }
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        //This is added so that it will not call the base of OnFirstEncounter
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        Character stalker = sourceParty.mainCharacter;
        Stalker stalkerAtt = stalker.GetAttribute(ATTRIBUTE.STALKER) as Stalker;
        return stalkerAtt.stalkee.ownParty.icharacterObject;
    }
    public override CharacterAction Clone() {
        StalkAction action = new StalkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
