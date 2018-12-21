using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookOfTheDead : SpecialToken {

    public BookOfTheDead() : base(SPECIAL_TOKEN.BOOK_OF_THE_DEAD) {
        quantity = 1;
        weight = 20;
    }
}
