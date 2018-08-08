using ECS;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public NewParty party { get; private set; }
    private BaseLandmark _landmark;

    [Header("Visitors")]
    [SerializeField] private CharacterPortrait visitorPortrait;
    [SerializeField] private Image visitorParty;

    [Header("Residents")]
    [SerializeField] private CharacterPortrait residentsPortrait;
    [SerializeField] private Image residentsParty;

    [Header("Action")]
    [SerializeField] private ActionIcon actionIcon;

    public void SetParty(NewParty party, BaseLandmark landmark) {
        this.party = party;
        _landmark = landmark;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        if (_landmark.IsResident(party.owner)) {
            //resident
            if (party.icharacters.Count > 1) {
                //use party icon
                residentsParty.gameObject.SetActive(true);
                residentsPortrait.gameObject.SetActive(false);
            } else {
                //use character portrait
                residentsParty.gameObject.SetActive(false);
                residentsPortrait.gameObject.SetActive(true);
                residentsPortrait.GeneratePortrait(party.owner, IMAGE_SIZE.X64, true, true);
            }
            visitorParty.gameObject.SetActive(false);
            visitorPortrait.gameObject.SetActive(false);
        } else {
            //visitor
            if (party.icharacters.Count > 1) {
                //use party icon
                visitorParty.gameObject.SetActive(true);
                visitorPortrait.gameObject.SetActive(false);
            } else {
                //use character portrait
                visitorParty.gameObject.SetActive(false);
                visitorPortrait.gameObject.SetActive(true);
                visitorPortrait.GeneratePortrait(party.owner, IMAGE_SIZE.X64, true, true);
            }
            residentsParty.gameObject.SetActive(false);
            residentsPortrait.gameObject.SetActive(false);
        }
    }

    public override void Reset() {
        base.Reset();
        party = null;
        _landmark = null;
    }
}
