using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName ="ColorConfig", menuName = "Config/ColorConfig", order = 0)]
public class ColorConfig : ScriptableObject
{
    public List<ColorModel> ColorModel => colorModel;

    [SerializeField] private List<ColorModel> colorModel;

    private Dictionary<ColorName, ColorModel> _dictColorModel = new Dictionary<ColorName, ColorModel>();

    [NonSerialized] bool _isInit = false;

    public ColorModel Get(ColorName name)
    {
        if(!_isInit)
        {
            Init();
        }

        return _dictColorModel[name];
    }
    private void Init()
    {
        _isInit = !_isInit;

        foreach(var model in colorModel)
        {
            _dictColorModel.Add(model.name, model);
        }
    }

}

[Serializable]
public struct ColorModel
{
    public ColorName name;
    public Color color;
    public Material material;
}

public enum ColorName
{
    Red,
    Green,
    Blue,
    White,
    Gray,
    Pink,
    Yellow,
    Violet
}

