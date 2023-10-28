using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageDetectionMode : MonoBehaviour, IInteractionManagerMode
{
    [SerializeField] private GameObject _ui;
    [SerializeField] private List<GameObject> _objectsToPlace;
    [SerializeField] private ARTrackedImageManager _aRTrackedImageManager;

    private int _refImageCount;
    private Dictionary<string, GameObject> _allObjects;
    private IReferenceImageLibrary _refLibrary;

    private void OnEnable()
    {
        _aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    private void OnDisable()
    {
        _aRTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void Start()
    {
        _refLibrary = _aRTrackedImageManager.referenceLibrary;
        _refImageCount = _refLibrary.count;
        LoadObjectDictionary();
    }

    private void LoadObjectDictionary()
    {
        _allObjects = new Dictionary<string, GameObject>();
        for (int i = 0; i < _refImageCount; i++)
        {
            GameObject newOverlay = new GameObject();
            newOverlay = _objectsToPlace[i];
            if (_objectsToPlace[i].gameObject.scene.rootCount == 0)
                newOverlay = Instantiate(_objectsToPlace[i], transform.localPosition, Quaternion.identity);

            _allObjects.Add(_refLibrary[i].name, newOverlay);
            newOverlay.SetActive(false);
        }
    }

    private void ActivateTrackedObject(string imageName)
    {
        Debug.Log("[IMAGE_DETECTION]: Tracked the target: " + imageName);
        _allObjects[imageName].SetActive(true);
        _allObjects[imageName].transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
    }

    private void UpdateTrackedObject(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            _allObjects[trackedImage.referenceImage.name].SetActive(true);
            _allObjects[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
            _allObjects[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;
        }
        else
        {
            _allObjects[trackedImage.referenceImage.name].SetActive(false);
        }
    }


    private void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var addedImage in args.added)
        {
            ActivateTrackedObject(addedImage.referenceImage.name);
        }

        foreach (var updated in args.updated)
        {
            UpdateTrackedObject(updated);
        }

        foreach (var trackedImage in args.removed)
        {
            Destroy(trackedImage.gameObject);
        }
    }

    public void Activate()
    {
        _ui.SetActive(true);
        InteractionManager.Instance.SetManagerMode(InteractionManager.ManagerMode.Images);
    }

    public void Deactivate()
    {
        InteractionManager.Instance.SetManagerMode(InteractionManager.ManagerMode.Planes);
        _ui.SetActive(false);
        foreach (var key in _allObjects)
            key.Value.SetActive(false);
    }

    public void TouchInteraction(Touch[] touches)
    {
        return;
    }

    public void BackToDefaultScreen()
    {
        InteractionManager.Instance.SelectMode(0);
    }
}
