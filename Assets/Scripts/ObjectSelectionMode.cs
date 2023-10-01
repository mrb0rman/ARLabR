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
        IncreasedVisibilityPlane();
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
        DecreasedVisibilityPlane();
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
            var currentColor = createdObject.Renderer.material.color;
            var heading = targetObject.transform.position - createdObject.transform.position;
            if (targetObject.Name != createdObject.Name)
            {
                currentColor = new Color(0, 0, 0, 0.2f);

                var rot = new Quaternion();
                rot.SetLookRotation(heading, Vector3.up);
                createdObject.ParticleSystem.transform.localRotation = rot;

                createdObject.ParticleSystem.Play();
            }
            else
            {
                currentColor = createdObject.DefaultMaterial.color;
                createdObject.ParticleSystem.Stop();
            }
            createdObject.Renderer.material.color = currentColor;
        }
    }

    private void CancelingSelection()
    {
        foreach (var createdObject in _objectCreationMode.ListCreatedObject)
        {
            createdObject.Renderer.material.color = createdObject.DefaultMaterial.color;
            createdObject.ParticleSystem.Stop();
        }
    }
    private void DecreasedVisibilityPlane()
    {
        var planes = GameObject.FindGameObjectsWithTag("ARPlane");
        foreach(var plane in planes)
        {
            plane.GetComponent<Renderer>().material.color = new Color(plane.GetComponent<Renderer>().material.color.r
                , plane.GetComponent<Renderer>().material.color.g
                , plane.GetComponent<Renderer>().material.color.b
                , 0.1f);
            plane.GetComponent<LineRenderer>().startWidth = 0f;
        }
    }

    private void IncreasedVisibilityPlane()
    {
        var planes = GameObject.FindGameObjectsWithTag("ARPlane");
        foreach (var plane in planes)
        {
            plane.GetComponent<Renderer>().material.color = new Color(plane.GetComponent<Renderer>().material.color.r
                , plane.GetComponent<Renderer>().material.color.g
                , plane.GetComponent<Renderer>().material.color.b
                , 0.4f);
            plane.GetComponent<LineRenderer>().startWidth = 0f;
        }
    }
}