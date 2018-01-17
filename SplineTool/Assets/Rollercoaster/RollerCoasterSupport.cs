using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RollerCoasterSupport : MonoBehaviour {

    public Vector3 pos;
    public Transform footer;
    public Transform beam;

	// Update is called once per frame
	void Update () {
        if (beam && footer) {
            if (transform.position != pos) {
                pos = transform.position;
                Vector3 footerPos = pos;
                footerPos.y = 0;
                Vector3 beamPos = pos;
                beamPos.y = .5f * pos.y;
                Vector3 beamScale = new Vector3(0.035f, .5f * pos.y, 0.035f);
                footer.position = footerPos;
                beam.position = beamPos;
                beam.localScale = beamScale;
            }
        }
	}
}
