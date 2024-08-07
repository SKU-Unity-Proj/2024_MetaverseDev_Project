using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartPosMove : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var cc = other.gameObject.GetComponent<CharacterController>();

            cc.enabled = false;
            other.transform.localPosition = new Vector3(2f, 2.3f, -15190f);
            cc.enabled = true;
        }
    }
}
