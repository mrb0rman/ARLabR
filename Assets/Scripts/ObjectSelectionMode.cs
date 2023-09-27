using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectSelectionMode : MonoBehaviour, IInteractionManagerMode
{
    [SerializeField] private GameObject _ui;
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private TMP_Text _objectTitleText;
    [SerializeField] private TMP_Text _objectDescriptionText;
    [SerializeField] private ObjectCreationMode _objectCreationMode;

    public void Activate()
    {
        _ui.SetActive(true);
        _descriptionPanel.SetActive(false);
    }

    public void Deactivate()
    {
        _descriptionPanel.SetActive(false);
        _ui.SetActive(false);

        CancelingSelection();
    }

    public void BackToDefaultScreen()
    {
        InteractionManager.Instance.SelectMode(0);
    }

    public void TouchInteraction(Touch[] touches)
    {
        Touch touch = touches[0];
        bool overUI = touch.position.IsPointOverUIObject();

        if (touch.phase == TouchPhase.Began)
        {
            if (!overUI)
            {
                TrySelectObject(touch.position);
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
        CreatedObject objectDescription = selectedObject.GetComponent<CreatedObject>();
        if (!objectDescription)
            throw new MissingComponentException("[OBJECT_SELECTION_MODE] " + selectedObject.name + " has no description!");

        ShowObjectDescription(objectDescription);
        ObjectSelection(objectDescription);
    }

    private void ShowObjectDescription(CreatedObject targetObject)
    {
        _objectTitleText.text = targetObject.Name;
        _objectDescriptionText.text = targetObject.Description;
        _descriptionPanel.SetActive(true);
    }

    private void ObjectSelection(CreatedObject targetObject)
    {
        foreach (var createdObject in _objectCreationMode.ListCreatedObject)
        {
            var currentColor = createdObject.MTMaterial.color;
            if (targetObject.Name != createdObject.Name)
            {
                currentColor = new Color(0,0,0, 0.1f);
            }
            else
            {
                currentColor = new Color(createdObject.DefaultMaterial.color.r
                    , createdObject.DefaultMaterial.color.g
                    , createdObject.DefaultMaterial.color.b
                    , 1f);
            }
            createdObject.MTMaterial.color = currentColor;
        }
    }

    private void CancelingSelection()
    {
        foreach (var createdObject in _objectCreationMode.ListCreatedObject)
        {
            var currentColor = createdObject.MTMaterial.color;
            currentColor = new Color(createdObject.DefaultMaterial.color.r
                , createdObject.DefaultMaterial.color.g
                , createdObject.DefaultMaterial.color.b
                , 1f);
            createdObject.MTMaterial.color = currentColor;
        }
    }
}
