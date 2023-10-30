using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameMode : MonoBehaviour, IInteractionManagerMode
{
    private const int _startTime = 60;

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
        Touch touch = touches[0];
        bool overUI = touch.position.IsPointOverUIObject();
        
        if (touch.phase != TouchPhase.Began || overUI)
            return;
        
        Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(touch.position);
        RaycastHit hitObject;
        if (!Physics.Raycast(ray, out hitObject))
            return;

        if (!hitObject.collider.CompareTag("GameCreatedObject"))
            return;

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
        StartCoroutine(SpawnTick());
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
        while(_currentTime > 0)
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

    private IEnumerator SpawnTick()
    {
        while(_listObject.Count < 13)
        {
            SpawnObject();
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void TimeDecrease()
    {
        _currentTime--;
        _timeText.text = _currentTime.ToString();
    }

    private void SpawnObject()
    {
        var form = gameCreatedObjects[_listObject.Count];
        form.IsUse = true;

        var newObject = Instantiate(
            form.gameObject, 
            new Vector3(Random.Range(-12, 12), Random.Range(-12, 12), Random.Range(-12, 12)), 
            Quaternion.Euler(Random.Range(0, 359), Random.Range(0, 359), Random.Range(0, 359)));
        
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
}
