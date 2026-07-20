using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FamiliarAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 5f;
    public float rotationSpeed = 2f;
    public float runSpeed = 2.5f; 

    private Rigidbody rb;
    private bool isGrounded = false;
    private bool isActing = false;
    private bool isPlaced = false;

    private Transform arCameraTransform; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Disable gravity and physics until we are placed in AR!
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Dynamically find the AR Camera
        UnityEngine.XR.ARFoundation.ARCameraManager camManager = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARCameraManager>();
        if (camManager != null) arCameraTransform = camManager.transform;
    }

    public void Activate()
    {
        // Now turn on physics!
        isPlaced = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        StartCoroutine(BehaviorLoop());
    }

    private IEnumerator BehaviorLoop()
    {
        while (isPlaced)
        {
            float idleTime = Random.Range(1.5f, 4.0f);
            yield return new WaitForSeconds(idleTime);

            if (!isActing)
            {
                int randomAction = Random.Range(0, 3);
                if (randomAction == 0 && isGrounded) Jump();
                else if (randomAction == 1) StartCoroutine(LookAround());
            }
        }
    }

    private void Jump()
    {
        isGrounded = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private IEnumerator LookAround()
    {
        isActing = true;
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

    public void ReactToWhistle()
    {
        if (arCameraTransform == null) return;
        
        // SAFETY: If the familiar fell into the void, teleport it back up to the camera level first!
        if (transform.position.y < arCameraTransform.position.y - 2f)
        {
            transform.position = arCameraTransform.position - (Vector3.up * 0.5f);
        }

        StopAllCoroutines(); 
        StartCoroutine(RunToCamera());
    }

    private IEnumerator RunToCamera()
    {
        isActing = true;
        Vector3 targetPos = arCameraTransform.position + (arCameraTransform.forward * 1.5f);
        targetPos.y = transform.position.y;

        while (Vector3.Distance(transform.position, targetPos) > 0.2f)
        {
            Vector3 lookDir = targetPos - transform.position;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * rotationSpeed * 2);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * runSpeed);
            yield return null;
        }

        Vector3 userPos = arCameraTransform.position;
        userPos.y = transform.position.y;
        transform.LookAt(userPos);
        if (isGrounded) Jump();

        isActing = false;
        StartCoroutine(BehaviorLoop());
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }
}