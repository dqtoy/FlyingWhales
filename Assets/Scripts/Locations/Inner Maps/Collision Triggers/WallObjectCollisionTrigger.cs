using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObjectCollisionTrigger : BaseCollisionTrigger<WallObject> {
    
    void Awake() {
        mainCollider = gameObject.AddComponent<BoxCollider2D>();
        gameObject.layer = LayerMask.NameToLayer("Area Maps Collision");
        (mainCollider as BoxCollider2D).size = new Vector2(0.7f, 0.7f);
    }

}
