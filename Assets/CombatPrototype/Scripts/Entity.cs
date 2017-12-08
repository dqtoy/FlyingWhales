using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class Entity {
        protected int _id;
        protected EntityComponent[] components;
    }
}

