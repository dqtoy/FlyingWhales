using UnityEngine;
using System.Collections;

public class CitizenInfoUI : MonoBehaviour {

	public CharismaTraitObject charismaTraitObj;
	public EfficiencyTraitObject efficiencyTraitObj;
	public IntelligenceTraitObject intelligenceTraitObj;

    public void SetTraits(Citizen citizen) {
        this.charismaTraitObj.SetTrait(citizen.charisma);
        this.efficiencyTraitObj.SetTrait(citizen.efficiency);
        this.intelligenceTraitObj.SetTrait(citizen.intelligence);
    }

	//public void SetKingTraits(King king){
	//	this.charismaTraitObj.SetTrait (king.charisma);
	//	this.efficiencyTraitObj.SetTrait (king.efficiency);
	//	this.intelligenceTraitObj.SetTrait (king.intelligence);

	//}
	//public void SetGovernorTraits(Governor governor){
	//	this.charismaTraitObj.SetTrait (governor.charisma);
	//	this.efficiencyTraitObj.SetTrait (governor.efficiency);
	//	this.intelligenceTraitObj.SetTrait (governor.intelligence);
	//}

}
