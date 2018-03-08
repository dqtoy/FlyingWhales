using UnityEngine;
using System.Collections;

public class LostHeir : CharacterTag {
	public LostHeir(ECS.Character character): base(character, CHARACTER_TAG.LOST_HEIR){
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		ECS.Item heirloomNecklace = ItemManager.Instance.CreateNewItemInstance ("Heirloom Necklace");
		_character.PickupItem (heirloomNecklace);
	}
	#endregion
}
