using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvent : GameEvent {

    private Character character;

    public TestEvent() : base(GAME_EVENT.TEST_EVENT) {

    }

    public void Initialize(Character character) {
        this.character = character;
    }
}
