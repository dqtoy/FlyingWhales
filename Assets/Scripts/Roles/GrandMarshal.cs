using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrandMarshal : Role {

    public GrandMarshal(Citizen citizen) : base(citizen) {

    }

    internal override void OnDeath() {
        base.OnDeath();
        Kingdom previousKingdomOfMarshal = this.citizen.city.kingdom;
        //Remove Family of chancellor from city
        List<Citizen> familyOfMarshal = new List<Citizen>();
        familyOfMarshal.AddRange(this.citizen.GetRelatives(-1));
        familyOfMarshal.Add(this.citizen);
        for (int i = 0; i < familyOfMarshal.Count; i++) {
            Citizen currFamilyMember = familyOfMarshal[i];
            previousKingdomOfMarshal.RemoveCitizenFromKingdom(currFamilyMember, currFamilyMember.city);
        }
        previousKingdomOfMarshal.CreateNewMarshalFamily();
    }
}
