using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent((typeof(ARPlaneManager)))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARTrackedImageManager))]
[RequireComponent(typeof(ARFaceManager))]
public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _modeObjects;
    private Camera _arCamera;
    public Camera ARCamera {
        get { 
            return _arCamera;
        }
    }

    public enum ManagerMode { Planes, Images, Faces }
    public ManagerMode CurrentManagerMode {
        get {
            return _currentManagerMode;
        }
    }
    private ManagerMode _currentManagerMode = ManagerMode.Planes;

    [SerializeField] private GameObject _detectPlaneHintObject;
    private bool _firstPlaneDetected = false;

    private IInteractionManagerMode[] _modes;
    private IInteractionManagerMode _currentMode = null;

    private ARRaycastManager _aRRaycastManager;
    private ARPlaneManager _aRPlaneManager;
    private ARAnchorManager _aRAnchorManager;
    private ARTrackedImageManager _arImageManager;
    private ARFaceManager _arFaceManager;
    private ARCameraManager _arCameraManager;
    private List<ARRaycastHit> _raycastHits;

    private const int DEFAULT_MODE = 0;

    #region Singleton
    /// <summary>
    /// Instance of our Singleton
    /// </summary>
    public static InteractionManager Instance {
        get {
            return _instance;
        }
    }
    private static InteractionManager _instance;

    public void InitializeSingleton()
    {
        // Destroy any duplicate instances that may have been created
        if (_instance != null && _instance != this)
        {
            Debug.Log("destroying singleton");
            Destroy(this);
            return;
        }
        _instance = this;
    }
    #endregion

    private void Awake()
    {
        InitializeSingleton();

        // setup variables
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();
        _aRPlaneManager = GetComponent<ARPlaneManager>();
        _aRAnchorManager = GetComponent<ARAnchorManager>();
        _arImageManager = GetComponent<ARTrackedImageManager>();
        _arFaceManager = GetComponent<ARFaceManager>();


        _arCameraManager = GetComponentInChildren<ARCameraManager>();
        if (!_arCameraManager)
            throw new MissingComponentException("ARCameraManager component not found!");

        // get interfaces from game objects
        _modes = new IInteractionManagerMode[_modeObjects.Length];
        for (int i = 0; i < _modeObjects.Length; i++)
        {
            _modes[i] = _modeObjects[i].GetComponent<IInteractionManagerMode>();

            if (_modes[i] == null)
                throw new MissingComponentException("Missing mode component on " + _modeObjects[i].name);
            Debug.Log("[INTERACTION_MANAGER] Found mode = " + _modes[i]);
        }

        SetManagerMode(_currentManagerMode);
    }

    private void OnEnable()
    {
        _aRPlaneManager.planesChanged += OnPlanesChanged;
        _aRAnchorManager.anchorsChanged += OnAnchorsChanged;
    }

    private void OnDisable()
    {
        _aRPlaneManager.planesChanged -= OnPlanesChanged;
        _aRAnchorManager.anchorsChanged -= OnAnchorsChanged;
    }

    public void SetManagerMode(ManagerMode newMode)
    {
        _currentManagerMode = newMode;
        switch (newMode)
        {
            case ManagerMode.Planes:
                ShowPlanes(true);
                _arImageManager.enabled = false;
                _arFaceManager.enabled = false;
                break;
            case ManagerMode.Images:
                ShowPlanes(false);
                _arImageManager.enabled = true;
                _arFaceManager.enabled = false;
                break;
            case ManagerMode.Faces:
                ShowPlanes(false);
                _arImageManager.enabled = false;
                _arFaceManager.enabled = true;
                break;
            default:
                break;
        }
    }

    private void ShowPlanes(bool state)
    {
        foreach (ARPlane plane in _aRPlaneManager.trackables)
            plane.gameObject.SetActive(state);
        _aRPlaneManager.enabled = state;
    }

    public void ShowFrontalCamera(bool state)
    {
        _aRRaycastManager.enabled = !state;
        _aRAnchorManager.enabled = !state;
        if (state)
        {
            _arCameraManager.requestedFacingDirection = CameraFacingDirection.User;
        }
        else
        {
            _arCameraManager.requestedFacingDirection = CameraFacingDirection.World;
        }
    }


    private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
    {
        if (args.added.Count > 0)
        {
            foreach (ARAnchor anchor in args.added)
            {
                Debug.Log("[INTERACTION_MANAGER]: added anchor " + anchor.name);
            }
        }

        if (args.updated.Count > 0)
        {
            foreach (ARAnchor anchor in args.updated)
            {
                Debug.Log("[INTERACTION_MANAGER]: updated anchor " + anchor.name);
            }
        }

        if (args.removed.Count > 0)
        {
            foreach (ARAnchor anchor in args.removed)
            {
                Debug.Log("[INTERACTION_MANAGER]: removed anchor " + anchor.name);
            }
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added.Count > 0)
        {
            foreach (ARPlane plane in args.added)
            {
                Debug.Log("[INTERACTION_MANAGER]: added plane " + plane.name);
            }

            ShowFirstScreen();
        }
        if (args.updated.Count > 0)
        {
            foreach (ARPlane plane in args.added)
            {
                Debug.Log("[INTERACTION_MANAGER]: updated plane " + plane.name);
            }
        }
        if (args.removed.Count > 0)
        {
            foreach (ARPlane plane in args.added)
            {
                Debug.Log("[INTERACTION_MANAGER]: removed plane " + plane.name);
            }
        }
    }

    private void Start()
    {
        // get camera in children
        _arCamera = GetComponentInChildren<Camera>();
        if (!_arCamera)
            throw new MissingComponentException("[INTERACTION_MANAGER] Camera not found in children of Interaction manager!");
    }

    private void ShowFirstScreen()
    {
        if (_firstPlaneDetected)
            return;
        else
            _firstPlaneDetected = true;
        _detectPlaneHintObject.SetActive(false);

        // reset current screen
        ReturnToDefaultMode();
        UpdateModes();
    }

    /// <summary>
    /// This method activates the selected mode and deactivates the rest
    /// </summary>
    private void UpdateModes()
    {
        for (int i = 0; i < _modes.Length; i++)
        {
            if (_currentMode == _modes[i])
            {
                _modes[i].Activate();
            }
            else
            {
                _modes[i].Deactivate();
            }
        }
    }

    public void SelectMode(int modeNumber)
    {
        _currentMode = _modes[modeNumber];
        UpdateModes();
    }

    public void ReturnToDefaultMode()
    {
        SelectMode(DEFAULT_MODE);
    }

    private void Update()
    {
        // if no mode selected, we do nothing
        if (_currentMode == null)
            return;

        if (Input.touchCount > 0)
        {
            // if we have touches, send them to the current mode
            _currentMode.TouchInteraction(Input.touches);
        }
    }

    /// <summary>
    /// Sends a ray from the screen touch position and returns the result. Method is used like this as a shortener to raycast manager.
    /// </summary>
    /// <param name="touchPosition">The ray position on the 2D screen</param>
    /// <param name="trackable">Types of objects to intersect ray with, planes by default</param>
    /// <returns></returns>
    public List<ARRaycastHit> GetARRaycastHits(Vector2 touchPosition, TrackableType trackable = TrackableType.Planes)
    {
        _aRRaycastManager.Raycast(
            screenPoint: touchPosition,
            hitResults: _raycastHits,
            trackableTypes: trackable
        );
        return _raycastHits;
    }
}