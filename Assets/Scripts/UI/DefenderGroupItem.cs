using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefenderGroupItem : PooledObject {

    public DefenderGroup defender { get; private set; }

    [SerializeField] private SlotItem[] slots;

    //private void Awake() {
    //    for (int i = 0; i < slots.Length; i++) {
    //        slots[i].SetNeededType()
    //    }
    //}

    private void OnEnable() {
        Messenger.AddListener<Area>(Signals.AREA_TOKEN_COLLECTION_CHANGED, OnAreaTokenCollectionChanged);
    }
    private void OnDisable() {
        Messenger.AddListener<Area>(Signals.AREA_TOKEN_COLLECTION_CHANGED, OnAreaTokenCollectionChanged);
    }

    private void OnAreaTokenCollectionChanged(Area area) {
        if (defender != null && defender.defendingArea.id == area.id) {
            UpdateSlots();
        }
    }

    public void SetDefender(DefenderGroup defender) {
        this.defender = defender;
        UpdateSlots();
    }

    public void UpdateSlots() {
        for (int i = 0; i < slots.Length; i++) {
            SlotItem currItem = slots[i];
            Character currCharacter = defender.party.characters.ElementAtOrDefault(i);
            if (currCharacter == null) {
                currItem.ClearSlot();
            } else {
                currItem.PlaceObject(currCharacter);
                if (defender.defendingArea.id == PlayerManager.Instance.player.playerArea.id) {
                    currItem.draggable.SetDraggable(true);
                    currItem.dropZone.SetEnabledState(true);
                } else {
                    currItem.draggable.SetDraggable(false);
                    currItem.dropZone.SetEnabledState(false);
                    currItem.portrait.UpdateUnknownVisual();
                }
            }
        }
    }

    public override void Reset() {
        base.Reset();
        defender = null;
    }
}
