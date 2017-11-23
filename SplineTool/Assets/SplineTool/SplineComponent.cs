using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineComponent : MonoBehaviour {

    public List<Spline> splines;

    public void Reset() {
        splines = new List<Spline> {
            new Spline()
        };
    }
}
