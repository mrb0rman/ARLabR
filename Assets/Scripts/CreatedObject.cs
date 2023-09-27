using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatedObject : MonoBehaviour
{
    private int _number = -1;

    public string Name {
        get {
            if (_number >= 0)
            {
                return _displayName + " " + _number.ToString();
            }
            else
            {
                return _displayName;
            }
        }
    }

    public string Description {
        get {
            return _description;
        }
    }

    public Material MTMaterial => _material;
    public Material DefaultMaterial;
    
    [SerializeField] private string _displayName;
    [SerializeField] private string _description;
    [SerializeField] private Material _material;
    public void GiveNumber(int number)
    {
        _number = number;
    }
    
    
}
