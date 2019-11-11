using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Game {
//    public enum GoapResult {
//        SUCCESS, FAILED, RUNNING
//    }

//    public class GoapAction {
//        private List<AtomAction> atomActions;
//        private int currentIndex;

//        public void Start() {
//            this.currentIndex = 0;
//        }

//        public GoapResult Update() {
//            GoapResult startResult = this.atomActions[this.currentIndex].Start();
//            if(startResult == GoapResult.FAILED || startResult == GoapResult.SUCCESS) {
//                return startResult;
//            }

//            // Needs update
//            GoapResult updateResult = this.atomActions[this.currentIndex].Update();
//            if(updateResult != GoapResult.RUNNING) {
//                return updateResult;
//            }
//        }
//    }

//    class GoapAgent : MonoBehaviour {
//        // 
//    }

//    public abstract class AtomAction {
//        public abstract GoapResult Start(GoapAgent agent);

//        public abstract GoapResult Update(GoapAgent agent);
//    }

//    public class CustomAction : ComponentAction<RuinarchAgent> {
//        public override GoapResult Start(GoapAgent agent) {
//            base.Start(agent); // Cache component here

//            // Use component
//            this.CachedComponent.BlahBlah();
//            return GoapResult.SUCCESS;
//        }

//        public override GoapResult Update(GoapAgent agent) {
//        }
//    }

//    public abstract class ComponentAction<T> : AtomAction where T: MonoBehaviour {
//        private T cachedComponent;

//        public override GoapResult Start(GoapAgent agent) {
//            this.cachedComponent = agent.GetComponent<T>();
//        }

//        protected T CachedComponent {
//            get {
//                return cachedComponent;
//            }
//        }
//    }

//    public struct Condition {
//        public string name;
//        public bool value;
//    }

//    public abstract class ConditionResolver {
//        public abstract bool IsMet(GoapAgent agent);
//    }

//    class GoapDomain {
//        private Dictionary<string, ConditionResolver> resolverMap;
//    }

//    //public class BigManager : MonoBehaviour, ConditionResolver {

//    //}
//}
