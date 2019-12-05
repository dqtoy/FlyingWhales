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
        currentIdeology = CreateIdeology(ideologyType);
        currentIdeology.SetRequirements(owner);
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
}