using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChainAction {
    public ChainAction parentChainAction;
    public CharacterAction action;
    public IPrerequisite prerequisite;
    public List<ChainAction> satisfiedPrerequisites;
    public List<IPrerequisite> finishedPrerequisites;

    public bool IsPrerequisiteFinished(Character character, ChainAction chainAction) {
        if (character.DoesSatisfiesPrerequisite(chainAction.prerequisite)) {
            chainAction.parentChainAction.finishedPrerequisites.Add(chainAction.prerequisite);
            chainAction.parentChainAction.satisfiedPrerequisites.Remove(chainAction);
            return true;
        }
        return false;
    }
}
