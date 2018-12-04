using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HibernateAction : CharacterAction {

    public HibernateAction() : base(ACTION_TYPE.HIBERNATE) {

    }

    #region Overrides
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        //for (int i = 0; i < party.characters.Count; i++) {
        //    Monster monster = party.characters[i] as Monster;
        //    monster.SetSleeping(true);
        //}
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        //for (int i = 0; i < party.icharacters.Count; i++) {
        //    party.icharacters[i].AdjustHP(5);
        //}
    }
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        //for (int i = 0; i < party.characters.Count; i++) {
        //    Monster monster = party.characters[i] as Monster;
        //    monster.SetSleeping(false);
        //}
    }
    public override CharacterAction Clone() {
        HibernateAction action = new HibernateAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
