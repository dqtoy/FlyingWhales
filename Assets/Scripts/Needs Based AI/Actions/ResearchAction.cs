using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchAction : CharacterAction {
    public ResearchAction() : base(ACTION_TYPE.RESEARCH) {
    }

    #region overrides
    public override CharacterAction Clone() {
        ResearchAction action = new ResearchAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        List<Item> researchedScrolls = new List<Item>();
        for (int i = 0; i < party.characterOwner.inventory.Count; i++) {
            Item currItem = party.characterOwner.inventory[i];
            if (currItem.itemName.Contains("Scroll")) {
                Debug.Log(party.characterOwner.name + " researched " + currItem.itemName);
                if (currItem.itemName.Contains("Dispel")) {
                    //end research scrolls quest
                    QuestManager.Instance.RemoveQuestFromBoards((party.characterOwner.hiddenDesire as ResearchScrollDesire).surrenderScrollsQuest);
                    //Awaken Skazi
                    Debug.LogWarning(party.characterOwner.name + " has awakened the skazi!");
                }
                researchedScrolls.Add(currItem);
            }
        }
        //remove researched scrolls from inventory
        for (int i = 0; i < researchedScrolls.Count; i++) {
            Item currScroll = researchedScrolls[i];
            party.characterOwner.ThrowItem(currScroll, false);
        }
    }
    #endregion
}
