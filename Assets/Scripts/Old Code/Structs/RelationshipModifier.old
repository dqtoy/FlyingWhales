using UnityEngine;
using System.Collections;

public class RelationshipModifier {
	public int modifier;
	public string reason;
	public string summary;
	public RELATIONSHIP_MODIFIER identifier;

	public RelationshipModifier (int modifier, string reason, RELATIONSHIP_MODIFIER identifier){
		this.modifier = modifier;
		this.reason = reason;
		this.identifier = identifier;
		SetSummary ();
	}

	internal void AdjustModifier(int amount){
		this.modifier += amount;
		SetSummary ();
	}
	internal void SetModifier(int amount){
		this.modifier = amount;
		SetSummary ();
	}
	private void SetSummary(){
		if(this.modifier < 0) {
			this.summary = this.modifier.ToString() + " " + this.reason;
		} else if (this.modifier > 0) {
			this.summary = "+" + this.modifier.ToString() + " " + this.reason;
		}
	}
}
