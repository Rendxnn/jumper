using System;
using UnityEngine;

// Helper to bind a Cinemachine camera's Follow/LookAt targets without taking a direct dependency
// on the Cinemachine assembly. Works with Cinemachine 2 (CinemachineVirtualCamera)
// and Cinemachine 3 (CinemachineCamera).
public class CinemachineFollowBinder : MonoBehaviour
{
    [Header("Targets")]
    public Transform followTarget;          // Typically your Player root
    public Transform lookAtTarget;          // Optional

    [Header("Camera Source")]
    [Tooltip("If left empty, the first Cinemachine camera in the scene will be used")]
    public GameObject cameraObject;         // GameObject holding the Cinemachine camera component
    public bool autoFindAtRuntime = true;   // Try to find a camera automatically if not assigned

    private Component _cmCamera;
    private Type _cmType;

    private void Awake()
    {
        EnsureCameraComponent();
    }

    private void Start()
    {
        ApplyBindings();
    }

    private void Reset()
    {
        // Try to auto-fill follow target with a Player tag if present
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            followTarget = player.transform;
    }

    private void EnsureCameraComponent()
    {
        if (_cmCamera != null && _cmType != null) return;

        if (cameraObject == null && autoFindAtRuntime)
        {
            // Search all scene objects for a CM camera component
            var all = FindObjectsOfType<GameObject>(true);
            foreach (var go in all)
            {
                var comp = GetCinemachineComponent(go);
                if (comp != null)
                {
                    cameraObject = go;
                    _cmCamera = comp;
                    _cmType = comp.GetType();
                    break;
                }
            }
        }
        else if (cameraObject != null)
        {
            _cmCamera = GetCinemachineComponent(cameraObject);
            _cmType = _cmCamera != null ? _cmCamera.GetType() : null;
        }
    }

    private Component GetCinemachineComponent(GameObject go)
    {
        // Try CM2 type name
        var t = Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
        if (t != null)
        {
            var c = go.GetComponent(t);
            if (c != null) return c;
        }
        // Try CM3 type name
        t = Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
        if (t != null)
        {
            var c = go.GetComponent(t);
            if (c != null) return c;
        }
        return null;
    }

    public void ApplyBindings()
    {
        EnsureCameraComponent();
        if (_cmCamera == null || _cmType == null)
        {
            Debug.LogWarning("Cinemachine camera not found. Ensure the package is installed and a CM camera exists in the scene.");
            return;
        }

        // Set Follow if property exists
        var followProp = _cmType.GetProperty("Follow");
        if (followProp != null)
        {
            followProp.SetValue(_cmCamera, followTarget);
        }

        // Set LookAt if property exists and a target is provided
        if (lookAtTarget != null)
        {
            var lookAtProp = _cmType.GetProperty("LookAt");
            if (lookAtProp != null)
                lookAtProp.SetValue(_cmCamera, lookAtTarget);
        }
    }
}

