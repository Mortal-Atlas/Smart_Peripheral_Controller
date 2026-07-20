using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARFamiliarPlacer : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag your Grey Box Familiar here")]
    public GameObject familiarObject;

    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Check if the user tapped the screen
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // CRITICAL: Prevent placing the familiar if the user is tapping a UI button!
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return; 
                }

                // Shoot a raycast from the touch position to detect AR planes (the floor/tables)
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // Get the pose (position and rotation) of where the raycast hit the physical floor
                    Pose hitPose = hits[0].pose;

                    // Activate the familiar and move it to the floor!
                    if (familiarObject != null)
                    {
                        familiarObject.SetActive(true);
                        familiarObject.transform.position = hitPose.position;

                        // Make it face the camera (the user) when placed
                        Vector3 lookPos = Camera.main.transform.position;
                        lookPos.y = hitPose.position.y; // Keep it flat on the ground
                        familiarObject.transform.LookAt(lookPos);
                        familiarObject.transform.Rotate(0, 180, 0); // Spin it so its "front" faces you
                        
                        Debug.Log("[AR Placer] Familiar successfully spawned on the floor!");
                    }
                }
            }
        }
    }
}