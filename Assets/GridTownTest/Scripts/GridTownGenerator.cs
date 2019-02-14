using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridTownGenerator : MonoBehaviour {

    public static GridTownGenerator Instance = null;

    [SerializeField] private AreaInnerTileMap map;

    private Dictionary<STRUCTURE_TYPE, List<LocationStructure>> testingInsideStructures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>() {
        { STRUCTURE_TYPE.DWELLING,
            new List<LocationStructure>(){
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
                {new Dwelling(null, true) },
            }
        },
        { STRUCTURE_TYPE.INN,
            new List<LocationStructure>(){
                {new LocationStructure(STRUCTURE_TYPE.INN, null, true) },
            }
        },
        { STRUCTURE_TYPE.WAREHOUSE,
            new List<LocationStructure>(){
                {new LocationStructure(STRUCTURE_TYPE.WAREHOUSE, null, true) },
            }
        },
    };
    private Dictionary<STRUCTURE_TYPE, List<LocationStructure>> testingOutsideStructures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>() {
        { STRUCTURE_TYPE.DUNGEON,
            new List<LocationStructure>(){
                {new LocationStructure(STRUCTURE_TYPE.INN, null, true) },
            }
        },
    };

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        map.GenerateInnerStructures(testingInsideStructures, testingOutsideStructures);
    }
}
