using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrandChancellor : Role {

    public GrandChancellor(Citizen citizen) : base(citizen) {

    }

    internal override void OnDeath() {
        base.OnDeath();
        Kingdom previousKingdomOfChancellor = this.citizen.city.kingdom;
        //Remove Family of chancellor from city
        List<Citizen> familyOfChancellor = new List<Citizen>();
        familyOfChancellor.AddRange(this.citizen.GetRelatives(-1));
        familyOfChancellor.Add(this.citizen);
        for (int i = 0; i < familyOfChancellor.Count; i++) {
            Citizen currFamilyMember = familyOfChancellor[i];
            previousKingdomOfChancellor.RemoveCitizenFromKingdom(currFamilyMember, currFamilyMember.city);
        }
        previousKingdomOfChancellor.CreateNewChancellorFamily();
    }
}
