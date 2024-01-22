using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestLotViewCamera : MonoBehaviour
{
    const int HitscanLength = 2000;

    Camera _camera;
    Transform _camTransform;
    Vector3 mouseholdPosition;
    float initialXCamRot;

    //level detection system
    /// <summary>
    /// A set of mesh colliders that the camera will shoot rays at to detect where the mouse cursor is in 3D world space
    /// and also where the floor is to adjust its Y position to see the floor appropriately
    /// <para/>See: <see cref="SetCameraDetectionMeshes(int, IEnumerable{MeshCollider})"/>
    /// </summary>
    Dictionary<int, Collider[]> floorElevationMeshes;

    // Start is called before the first frame update
    void Start()
    {
        floorElevationMeshes = new Dictionary<int, Collider[]>();
        _camTransform = GetComponentInParent<Transform>();
        _camera = GetComponentInParent<Camera>();        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) // right or middle click
        {
            mouseholdPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(2))
                initialXCamRot = _camTransform.rotation.eulerAngles.x;
        }

        var mouseChange = mouseholdPosition - Input.mousePosition;

        if (Input.GetMouseButton(1)) // Right Click camera translation
        {
            var change = mouseChange / 10;
            _camTransform.TransformDirection(change);
            change *= Time.deltaTime;

            _camTransform.position =
                new Vector3(_camTransform.position.x - change.x,
                            _camTransform.position.y,
                            _camTransform.position.z - change.y);
        }
        else if (Input.GetMouseButton(2)) // Middle Mouse Click Camera pitch
        {
            float pitchChange = (mouseChange / 1).y;
            var transformedXRot = Math.Max(0, Math.Min(90, initialXCamRot - pitchChange));
            var currentAngle = _camTransform.rotation.eulerAngles;
            currentAngle = new Vector3(transformedXRot, currentAngle.y, currentAngle.z);
            _camTransform.rotation = Quaternion.Euler(currentAngle);
        }
    }

    /// <summary>
    /// Sets the mesh colliders used to detect the height of the floor the camera is looking at.
    /// <para/>
    /// </summary>
    /// <param name="Colliders"></param>
    public void SetCameraDetectionMeshes(int Floor, IEnumerable<Collider> Colliders)
    {
        floorElevationMeshes.Remove(Floor);
        floorElevationMeshes.Add(Floor, Colliders.Where(x => x != default).ToArray());
    }

    public static Ray GetMouseCursor3DRay(Camera SelectedCamera)
    {
        //transform scr pos to world
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 20;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //get directional vector 
        var direction = (mousePosition - SelectedCamera.transform.position); direction.Normalize();

        //build a ray from camera to terrain
        return new Ray(SelectedCamera.transform.position, direction);
    }

    /// <summary>
    /// Potentially expensive!! use with caution
    /// </summary>
    /// <param name="MaxFloor"></param>
    /// <param name="WorldPosition"></param>
    /// <returns></returns>
    public bool TranslateScreen2WorldPos(int MaxFloor, out Vector3 WorldPosition, out int Floor)
    {
        WorldPosition = new Vector3();
        Floor = -1;
        
        if (!floorElevationMeshes.Any())
        {
            Debug.LogWarning("No terrain meshes added to the lot camera so hit detection is disabled!");
            return false;
        }        

        Ray camRay = GetMouseCursor3DRay(_camera);

        //Check each floor by casting the above ray at each collider.
        //TODO: (NEEDS REFACTOR TO USE ELEVATION IN LOT DATA)
        //also just in general this iteration is really not great should only be used on frames where theres actual movement
        RaycastHit hitInfo = default;
        bool hitComplete = false;

        //go top down searching for which floor we're looking at
        for(int floor = MaxFloor; floor >= floorElevationMeshes.Keys.Min(); floor--)
        {
            if (!floorElevationMeshes.ContainsKey(floor)) continue;
            foreach(var collider in floorElevationMeshes[floor])
            {
                if (collider is MeshCollider mCollider)
                {
                    var meshRef = collider.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    mCollider.sharedMesh = meshRef;
                }
                if (collider.Raycast(camRay, out hitInfo, HitscanLength))
                {
                    hitComplete = true;
                    Floor = floor;
                    break;
                }
            }
            if (hitComplete) break;
        }
        if (!hitComplete) return false;

        WorldPosition = hitInfo.point;
        return true;
    }
}
