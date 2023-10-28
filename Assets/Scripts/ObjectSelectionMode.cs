using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectSelectionMode : MonoBehaviour, IInteractionManagerMode
{
    [Tooltip("UI objects to disable")]
    [SerializeField] private GameObject _ui;
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private TMP_Text _objectTitleText;
    [SerializeField] private TMP_Text _objectDescriptionText;

    private CreatedObject _selectedObject = null;
    private bool _needResetTouch = false;

    public void Activate()
    {
        _ui.SetActive(true);
        _descriptionPanel.SetActive(false);
        _selectedObject = null;
    }

    public void Deactivate()
    {
        _descriptionPanel.SetActive(false);
        _ui.SetActive(false);
        _selectedObject = null;
    }

    public void BackToDefaultScreen()
    {
        InteractionManager.Instance.SelectMode(0);
    }

    public void TouchInteraction(Touch[] touches)
    {
        Touch touch = touches[0];
        bool overUI = touch.position.IsPointOverUIObject();

        // this is added due to the fact that when we selected an object, we don't want to manipulate it immediately
        // we will wait until first touch becomes Ended or Canceled, then touch interactions will work properly
        if (_needResetTouch)
        {
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                _needResetTouch = false;
            else
                return;
        }

        if (_selectedObject == null)
        {
            if (touch.phase != TouchPhase.Began || overUI)
                return;

            TrySelectObject(touch.position);
            _needResetTouch = true;
        }
        else
        {
            // if there is a selected object, our logic changes according to the number of touches
            // if we touch screen with one finger, it's movement
            if (touches.Length == 1)
            {
                MoveSelectedObject(touch);
            }
            // if we touch screen with two fingers, it's rotation
            else if (touches.Length == 2)
            {
                RotateSelectedObject(touch, touches[1]);
            }
        }
    }

    private void TrySelectObject(Vector2 pos)
    {
        // fire a ray from camera to the target screen position
        Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(pos);
        RaycastHit hitObject;
        if (!Physics.Raycast(ray, out hitObject))
            return;

        if (!hitObject.collider.CompareTag("CreatedObject"))
            return;

        // if we hit a spawned object tag, try to get info from it
        GameObject selectedObject = hitObject.collider.gameObject;
        _selectedObject = selectedObject.GetComponent<CreatedObject>();
        if (!_selectedObject)
            throw new MissingComponentException("[OBJECT_SELECTION_MODE] " + selectedObject.name + " has no description!");

        ShowObjectDescription(_selectedObject);
    }

    private void ShowObjectDescription(CreatedObject targetObject)
    {
        _objectTitleText.text = targetObject.Name;
        _objectDescriptionText.text = targetObject.Description;
        _descriptionPanel.SetActive(true);
    }

    private void MoveSelectedObject(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            ARAnchor anchor = _selectedObject.GetComponent<ARAnchor>();
            if (anchor != null)
                Destroy(anchor);
            return;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            _selectedObject.AddComponent<ARAnchor>();
            return;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            _selectedObject.transform.position = InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position;
        }
    }

    private void RotateSelectedObject(Touch touch1, Touch touch2)
    {
        if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float distance = Vector2.Distance(touch1.position, touch2.position);
            float distancePrev = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
            float delta = distance - distancePrev;

            if (Mathf.Abs(delta) > 0.0f)
                delta *= 0.1f;
            else
                delta *= -0.1f;

            // when you want to rotate object by angle, multiply its quaternion-type rotation by a rotation angle quaternion
            _selectedObject.transform.rotation *= Quaternion.Euler(0.0f, delta, 0.0f);
        }
    }
}