using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameMode : MonoBehaviour, IInteractionManagerMode
{
    private const int _startTime = 60;
    [SerializeField] private ARPlaneManager _aRPlaneManager;
    [SerializeField] private GameObject _ui;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private GameCreatedObject[] gameCreatedObjects;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _buttonStart;
    [SerializeField] private GameObject _buttonRestart;

    private List<GameObject> _listObject = new List<GameObject>();
    private int _currentTime;
    private int score;

    public void Activate()
    {
        _ui.SetActive(true);
        _buttonStart.SetActive(true);
        _info.SetActive(true);

        _infoText.text = "После нажатия Start найдите 3 объекта с метками до истчения таймера";
        foreach(var item in gameCreatedObjects)
        {
            item.IsUse = false;
        }
        _currentTime = _startTime;
        _timeText.text = _currentTime.ToString();
    }

    public void Deactivate()
    {
        _ui.SetActive(false);
        StopAllCoroutines();
        ClearScene();
    }

    public void TouchInteraction(Touch[] touches)
    {
        foreach(var touch in touches)
        {
            Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(touch.position);
            RaycastHit hitObject;
            if (!Physics.Raycast(ray, out hitObject))
            {
                throw new MissingComponentException("[GAME_MODE] Physics.Raycast!");
                return;
            }
                

            if (!hitObject.collider.CompareTag("GameCreatedObject"))
            {
                throw new MissingComponentException("[GAME_MODE] hitObject!");
                return;
            }

            // if we hit a spawned object tag, try to get info from it
            GameObject selectedObject = hitObject.collider.gameObject;
            var _selectedObject = selectedObject.GetComponent<GameCreatedObject>();
            if (!_selectedObject)
                throw new MissingComponentException("[GAME_MODE] " + selectedObject.name + " has no description!");
            if (_selectedObject.IsTarget)
            {
                Destroy(selectedObject);
                score++;
            }
            return;
        }
    }

    public void BackToDefaultScreen()
    {
        InteractionManager.Instance.SelectMode(0);
    }

    public void StartGame()
    {
        _info.SetActive(false);
        _buttonStart.SetActive(false);
        _currentTime = _startTime;
        _timeText.text = _currentTime.ToString();
        score = 0;
        StartCoroutine(Tick());
    }

    public void RestartGame()
    {
        foreach(var item in _listObject)
        {
            Destroy(item.gameObject);
        }
        _listObject.Clear();
        StartGame();
        _info.SetActive(false);
        _buttonRestart.SetActive(false);
    }
    
    private IEnumerator Tick()
    {
        while(_currentTime > 0 && score < 3)
        {
            TimeDecrease();
            if (_listObject.Count < 13)
            {
                SpawnObject();
            }
            yield return new WaitForSeconds(1f);
        }
        EndGame();
    }
    
    private void TimeDecrease()
    {
        _currentTime--;
        _timeText.text = _currentTime.ToString();
    }
    
    private void SpawnObject()
    {
        var plane = GetRandomARPlane();
        
        var form = gameCreatedObjects[_listObject.Count];
        form.IsUse = true;

        var newObject = Instantiate(
            form.gameObject,
           plane.transform.position, 
            Quaternion.Euler(Random.Range(0, 359), 
                Random.Range(0, 359), 
                Random.Range(0, 359)));
        newObject.transform.SetParent(plane.transform);
        newObject.transform.position = GetRandomPosition(plane);

        newObject.AddComponent<ARAnchor>();

        _listObject.Add(newObject);
    }


    private void EndGame()
    {
        if (score >= 3)
        {
            _infoText.text = "Win";
        }
        else
        {
            _infoText.text = "Lose";
        }
        _info.SetActive(true);
        _buttonRestart.SetActive(true);
        ClearScene();
    }

    private void ClearScene()
    {
        foreach(var item in _listObject)
        {
            Destroy(item.gameObject);
        }
        _listObject.Clear();
    }

    private ARPlane GetRandomARPlane()
    {
        var arplaneCollection = _aRPlaneManager.trackables;
        int scorePlane = 0;
        int numperPlane = Random.Range(0, arplaneCollection.count);

        foreach (var plane in arplaneCollection)
        {
            if (scorePlane == numperPlane)
            {
                return plane;
            }
            scorePlane++;
        }

        return null;
    }
    private Vector3 GetRandomPosition(ARPlane plane)
    {
        var randomBoundary = Random.Range(0, plane.boundary.Length);
        var lenghtVector = (plane.centerInPlaneSpace - plane.boundary[randomBoundary]).magnitude;

        var randomPosition = Vector3.zero;
        randomPosition.x = Random.Range(plane.center.x, plane.center.x + lenghtVector);
        randomPosition.z = Random.Range(plane.center.z, plane.center.z + lenghtVector);
        randomPosition.y = plane.transform.position.y;

        return randomPosition;
    }
}
