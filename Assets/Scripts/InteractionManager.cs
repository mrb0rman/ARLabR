using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _modeObjects;
    private Camera _arCamera;
    public Camera ARCamera {
        get { 
            return _arCamera; 
        }
    }

    private IInteractionManagerMode[] _modes;
    private IInteractionManagerMode _currentMode = null;

    private ARRaycastManager _aRRaycastManager;
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

        // get interfaces from game objects
        _modes = new IInteractionManagerMode[_modeObjects.Length];
        for (int i = 0; i < _modeObjects.Length; i++)
        {
            _modes[i] = _modeObjects[i].GetComponent<IInteractionManagerMode>();

            if (_modes[i] == null)
                throw new MissingComponentException("Missing mode component on " + _modeObjects[i].name);
            Debug.Log("[INTERACTION_MANAGER] Found mode = " + _modes[i]);
        }
    }

    private void Start()
    {
        // get camera in children
        _arCamera = GetComponentInChildren<Camera>();
        if (!_arCamera)
            throw new MissingComponentException("[INTERACTION_MANAGER] Camera not found in children of Interaction manager!");

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
