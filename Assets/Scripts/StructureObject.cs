using UnityEngine;
using System.Collections;
using System.Linq;

public class StructureObject : MonoBehaviour {

    private STRUCTURE_STATE _structureState;

    private GameObject[] normalParents;
    private GameObject[] ruinedParents;
    
    public void Initialize() {
        normalParents = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "Normal").Select(x => x.gameObject).ToArray();
        ruinedParents = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "Ruined").Select(x => x.gameObject).ToArray();
    }

    public void SetStructureState(STRUCTURE_STATE structureState) {
        _structureState = structureState;
        if(structureState == STRUCTURE_STATE.NORMAL) {
            for (int i = 0; i < normalParents.Length; i++) {
                normalParents[i].SetActive(true);
            }
            for (int i = 0; i < ruinedParents.Length; i++) {
                ruinedParents[i].SetActive(false);
            }
        } else {
            for (int i = 0; i < normalParents.Length; i++) {
                normalParents[i].SetActive(false);
            }
            for (int i = 0; i < ruinedParents.Length; i++) {
                ruinedParents[i].SetActive(true);
            }
        }
    }
}
