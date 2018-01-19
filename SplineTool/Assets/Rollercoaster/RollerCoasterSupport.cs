using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RollerCoasterSupport : MonoBehaviour {

    private Vector3 pos;
    private Quaternion rot;
    public Transform mainFooter;
    public Transform mainBeam;
    public Transform secFooter;
    public Transform secBeam;
    public Transform connector;

    private float beamWidth = .045f;

    // Update is called once per frame
    void Update () {
        if (transform.position != pos || transform.rotation != rot) {
            pos = transform.position;
            rot = transform.rotation;

            Vector3 connectorPos = connector.position;

            Vector3 beamPos = connectorPos;
            beamPos.y = .5f * connectorPos.y;
            Vector3 beamScale = new Vector3(beamWidth, .5f * connectorPos.y + .02f, beamWidth);
            mainBeam.rotation = Quaternion.identity;
            mainBeam.position = beamPos;
            mainBeam.localScale = beamScale;

            Vector3 footerPos = connectorPos;
            footerPos.y = 0;
            mainFooter.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0));
            mainFooter.position = footerPos;

            if (connectorPos.y > 1.5f) {
                secFooter.gameObject.SetActive(true);
                secBeam.gameObject.SetActive(true);

                secFooter.rotation = mainFooter.rotation;
                secFooter.position = mainFooter.position + mainFooter.TransformDirection(Vector3.left).normalized * (connectorPos.y - .3f) * .5f;

                secBeam.position = Vector3.Lerp(connectorPos - Vector3.up * .3f, secFooter.position, .5f);
                secBeam.localScale = new Vector3(beamWidth, .5f * Vector3.Distance(connectorPos - Vector3.up * .3f, secFooter.position), beamWidth);
                secBeam.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, -26.565f));

                if (Mathf.Abs(Vector3.Angle(connector.up, secBeam.up)) > 90) {
                    secFooter.position = mainFooter.position - mainFooter.TransformDirection(Vector3.left).normalized * (connectorPos.y - .3f) * .5f;
                    secFooter.Rotate(0, 180, 0);
                    secBeam.position = Vector3.Lerp(connectorPos - Vector3.up * .3f, secFooter.position, .5f);
                    secBeam.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 26.565f));
                }

                secFooter.position = secFooter.position + secFooter.TransformDirection(Vector3.right).normalized * .025f;

            }
            else {
                secFooter.gameObject.SetActive(false);
                secBeam.gameObject.SetActive(false);
            }
        }
	}
}
