using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollisionTrigger : POICollisionTrigger {

     public override LocationGridTile gridTileLocation { get { return poi.gridTileLocation; } }
}
