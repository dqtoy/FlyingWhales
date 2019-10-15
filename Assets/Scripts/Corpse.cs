using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Corpse : TileObject {
    public Character character { get; private set; }

    public Corpse(LocationStructure location) {
        SetStructureLocation(location);
        Initialize(TILE_OBJECT_TYPE.CORPSE);
    }

    public override void OnClickAction() {
        base.OnClickAction();
        UIManager.Instance.ShowCharacterInfo(character);
    }

    public override string ToString() {
        return "Corpse of " + character.name;
    }

    public void SetCharacter(Character character) {
        this.character = character;
    }
}

public class SaveDataCorpse : SaveDataTileObject {
    public int characterID;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Corpse obj = tileObject as Corpse;
        characterID = obj.character.id;
    }

    public override TileObject Load() {
        Corpse obj = base.Load() as Corpse;
        obj.SetCharacter(CharacterManager.Instance.GetCharacterByID(characterID));
        return obj;
    }
}