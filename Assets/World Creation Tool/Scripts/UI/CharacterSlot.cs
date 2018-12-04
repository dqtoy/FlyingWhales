
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSlot : MonoBehaviour {

    public delegate void OnCharacterItemDropped(Character character);
    public OnCharacterItemDropped onCharacterItemDropped;


    public void OnCharacterSlotDropped(Transform droppedTransform) {
        DraggableCharacterItem item = droppedTransform.GetComponent<DraggableCharacterItem>();
        if (item != null) {
            if (onCharacterItemDropped != null) {
                onCharacterItemDropped(item.character);
            }
        }
    }
}
