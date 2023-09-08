using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject _spawnObjectPrefab;

    private ARRaycastManager _aRRaycastManager;
    private List<ARRaycastHit> _listRaycastHits;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _listRaycastHits = new List<ARRaycastHit>();
    }
    
    private void Update()
    {
        if(Input.touchCount > 0)
        {
            TouchControll(Input.GetTouch(0));
        }
    }

    private void TouchControll(Touch touch)
    {
        if(touch.phase == TouchPhase.Began)
        {
            SpawnObject(touch);
        }
    }

    private void SpawnObject(Touch touch)
    {
        _aRRaycastManager.Raycast(touch.position, _listRaycastHits, TrackableType.Planes);
        Instantiate(_spawnObjectPrefab, _listRaycastHits[0].pose.position, Quaternion.identity);
    }
}
