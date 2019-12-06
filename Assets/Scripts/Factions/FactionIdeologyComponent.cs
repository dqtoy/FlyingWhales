using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionIdeologyComponent {
    public Faction owner { get; private set; }
    public FactionIdeology currentIdeology { get; private set; }

    public FactionIdeologyComponent(Faction owner) {
        this.owner = owner;
    }

    public void SwitchToIdeology(FACTION_IDEOLOGY ideologyType) {
        if(currentIdeology != null && currentIdeology.ideologyType == FACTION_IDEOLOGY.INCLUSIVE && ideologyType == FACTION_IDEOLOGY.INCLUSIVE) { return; }
        currentIdeology = CreateIdeology(ideologyType);
        currentIdeology.SetRequirements(owner);
        ReEvaluateFactionMembers();
    }
    public bool DoesCharacterFitCurrentIdeology(Character character) {
        if(currentIdeology == null) { return true; }
        return currentIdeology.DoesCharacterFitIdeology(character);
    }

    private FactionIdeology CreateIdeology(FACTION_IDEOLOGY ideologyType) {
        string ideologyStr = ideologyType.ToString();
        var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(ideologyStr);
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            FactionIdeology data = System.Activator.CreateInstance(type) as FactionIdeology;
            return data;
        } else {
            throw new System.Exception(ideologyStr + " has no data!");
        }
    }

    private void ReEvaluateFactionMembers() {
        for (int i = 0; i < owner.characters.Count; i++) {
            Character member = owner.characters[i];
            if(member == owner.leader) { continue; }
            owner.CheckIfCharacterStillFitsIdeology(member);
        }
    }
}