using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FamiliarAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 5f;
    public float rotationSpeed = 2f;
    
    private Rigidbody rb;
    private bool isGrounded = false;
    private bool isActing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure gravity is on and it doesn't tip over like a ragdoll
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        StartCoroutine(BehaviorLoop());
    }

    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            // Wait for a random few seconds before doing the next action
            float idleTime = Random.Range(1.5f, 4.0f);
            yield return new WaitForSeconds(idleTime);

            if (!isActing)
            {
                int randomAction = Random.Range(0, 3); // 0, 1, or 2

                if (randomAction == 0 && isGrounded)
                {
                    Jump();
                }
                else if (randomAction == 1)
                {
                    StartCoroutine(LookAround());
                }
                // If 2, do nothing (stay idle)
            }
        }
    }

    private void Jump()
    {
        isGrounded = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log("[FamiliarAI] Hop!");
    }

    private IEnumerator LookAround()
    {
        isActing = true;
        
        // Pick a random direction to look (between -90 and 90 degrees from current)
        float randomAngle = Random.Range(-90f, 90f);
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y + randomAngle, 0);
        
        float time = 0;
        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        isActing = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Simple check to see if we landed on a surface
        isGrounded = true;
    }
}