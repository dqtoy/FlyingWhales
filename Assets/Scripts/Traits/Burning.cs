using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Burning : Trait {

    private ITraitable owner;
    public override bool isPersistent { get { return true; } }

    private GameObject burningEffect;

    public Burning() {
        name = "Burning";
        description = "This is burning.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        owner = addedTo;
        if (addedTo is LocationGridTile) {
            LocationGridTile tile = addedTo as LocationGridTile;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(tile);
        } else if (addedTo is TileObject) {
            TileObject obj = addedTo as TileObject;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(obj);
            obj.SetPOIState(POI_STATE.INACTIVE);
        } else if (addedTo is SpecialToken) {
            SpecialToken token = addedTo as SpecialToken;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(token);
            token.SetPOIState(POI_STATE.INACTIVE);
        } else if (addedTo is Character) {
            Character character = addedTo as Character;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(character);
        }
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        ObjectPoolManager.Instance.DestroyObject(burningEffect);
    }
    #endregion

    private void PerTick() {
        //Burning characters reduce their current hp by 2% of maxhp every tick. 
        //They also have a 6% chance to remove Burning effect but will not gain a Burnt trait afterwards. 
        //If a character dies and becomes a corpse, it may still continue to burn.
        if (owner is Character) {
            Character character = owner as Character;
            if (!character.isDead) {
                character.AdjustHP(-(int)(character.maxHP * 0.02f), true);
            }
            if (Random.Range(0, 100) < 6) {
                owner.RemoveTrait(this);
            }
        } else {
            //Every tick, a Burning tile or object also has a 3% chance to remove Burning effect. 
            //Afterwards, it will have a Burnt trait, which disables its Flammable trait (meaning it can no longer gain a Burning status).
            if (Random.Range(0, 100) < 3) {
                owner.RemoveTrait(this);
                owner.AddTrait("Burnt");
                return; //do not execute other effecs.
            }
        }

        //Every tick, a Burning tile, object or character has a 15% chance to spread to an adjacent flammable tile, flammable character, 
        //flammable object or the object in the same tile. 
        if (Random.Range(0, 100) < 15) {
            List<ITraitable> choices = new List<ITraitable>();
            LocationGridTile origin = owner.gridTileLocation;
            choices.AddRange(origin.GetAllTraitablesOnTileWithTrait("Flammable"));
            List<LocationGridTile> neighbours = origin.FourNeighbours();
            for (int i = 0; i < neighbours.Count; i++) {
                choices.AddRange(neighbours[i].GetAllTraitablesOnTileWithTrait("Flammable"));
            }
            choices = choices.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet") == null).ToList();
            if (choices.Count > 0) {
                ITraitable chosen = choices[Random.Range(0, choices.Count)];
                chosen.AddTrait("Burning");
            }
        }
       
    }
}
