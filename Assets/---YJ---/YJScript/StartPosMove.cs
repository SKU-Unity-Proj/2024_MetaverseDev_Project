using UnityEngine;
using System.Collections;

public class StartPosMove : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var cc = other.gameObject.GetComponent<CharacterController>();

            StartCoroutine(MoveAndEnableCharacterController(other.transform, cc, new Vector3(2f, 2.3f, -15190f)));
        }
    }

    IEnumerator MoveAndEnableCharacterController(Transform playerTransform, CharacterController cc, Vector3 targetPosition)
    {
        // Disable CharacterController
        cc.enabled = false;

        // Move the player to the target position
        playerTransform.position = targetPosition;

        // Wait for the end of the frame to ensure the position update is complete
        yield return new WaitForEndOfFrame();

        // Enable CharacterController
        cc.enabled = true;
    }
}
