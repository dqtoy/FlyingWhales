using UnityEngine;
using System.Collections;

namespace Pathfinding {
    /** Sets the destination of an AI to the position of a specified object.
	 * This component should be attached to a GameObject together with a movement script such as AIPath, RichAI or AILerp.
	 * This component will then make the AI move towards the #target set on this component.
	 *
	 * \see #Pathfinding.IAstarAI.destination
	 *
	 * \shadowimage{aidestinationsetter.png}
	 */
    [UniqueComponent(tag = "ai.destination")]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
    public class CharacterDestinationSetter : VersionedMonoBehaviour {
        /** The object that the AI should move to */
        public Vector3 target;
        CharacterAIPath ai;
        public Transform targetTrans;

        void OnEnable() {
            ai = GetComponent<CharacterAIPath>();
            // Update the destination right before searching for a path as well.
            // This is enough in theory, but this script will also update the destination every
            // frame as the destination is used for debugging and may be used for other things by other
            // scripts as well. So it makes sense that it is up to date every frame.
            if (ai != null) ai.onSearchPath += Update;
        }

        void OnDisable() {
            if (ai != null) ai.onSearchPath -= Update;
        }

        /** Updates the AI's destination every frame */
        void Update() {
            if (targetTrans != null && ai != null) {
                //int x = (int) target.x;
                //int y = (int) target.y;
                ai.destination = targetTrans.position;
                //ai.destination = target;
            } 
        }

        public void SetDestination(Vector3 destination) {
            target = destination;
            ai.destination = destination;
            ai.canSearch = true;
            //Debug.Log(ai.marker.character.name + " set destination to " + target.ToString());
        }
        public void SetTranformTarget(Transform target) {
            ai.canSearch = true;
            targetTrans = target;
        }

        public void ClearPath() {
            target = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            ai.OnClearPath();
        }
    }
}
