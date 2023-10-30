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
    [SerializeField] private GameCreatedObject[] gameCreatedObjects;
    [SerializeField] private GameObject _info;

    private List<GameObject> _listObject = new List<GameObject>();
    private int _currentTime;

    public void Activate()
    {
        _ui.SetActive(true);
        _currentTime = _startTime;
        _timeText.text = _currentTime.ToString();
    }

    // Update is called once per frame
    public void Deactivate()
    {
        StopAllCoroutines();
        _ui.SetActive(false);

        foreach(var item in _listObject)
        {
            Destroy(item.gameObject);
        }
        _listObject.Clear();
       
    }

    public void TouchInteraction(Touch[] touches)
    {
        return;
    }

    public void BackToDefaultScreen()
    {
        InteractionManager.Instance.SelectMode(0);
    }

    public void StartGame()
    {
        _info.SetActive(false);
        StartCoroutine(Tick());
        StartCoroutine(SpawnTick());
    }

    private IEnumerator Tick()
    {
        while(_currentTime > 0)
        {
            TimeDecrease();
            yield return new WaitForSeconds(1f);
            if(_currentTime <= 0)
            {
                Deactivate();
            }
        }
    }
    private IEnumerator SpawnTick()
    {
        while (_currentTime > 0 && _listObject.Count < 13)
        {
            SpawnObject();
            yield return new WaitForSeconds(1f);
        }
    }

    private void TimeDecrease()
    {
        _currentTime--;
        _timeText.text = _currentTime.ToString();
    }

    private void SpawnObject()
    {
        var form = gameCreatedObjects[Random.Range(0, 12)];
        if(form.IsUse)
        {
            SpawnObject();
        }
        form.IsUse = true;

        var newObject = Instantiate(
            form.gameObject, 
            new Vector3(Random.Range(-12, 12), Random.Range(-12, 12), Random.Range(-12, 12)), 
            Quaternion.Euler(Random.Range(0, 359), Random.Range(0, 359), Random.Range(0, 359)));
        

        newObject.AddComponent<ARAnchor>();

        _listObject.Add(newObject);
    }
}
