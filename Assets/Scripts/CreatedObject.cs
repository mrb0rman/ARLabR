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

    public Renderer Renderer => _renderer;
    public Material DefaultMaterial;
    public ParticleSystem ParticleSystem;
    
    [SerializeField] private string _displayName;
    [SerializeField] private string _description;
    [SerializeField] private Renderer _renderer;
    public void GiveNumber(int number)
    {
        _number = number;
    }
    
    
}
