using UnityEngine;
using UnityEngine.UI;


public class ColorController : MonoBehaviour
{
    [HideInInspector] public ColorModel[] model;

    [SerializeField] private ButtonView[] _button;
    private ColorConfig _colorConfig;

    private float _timer = 3f;

    private void Start()
    {
        _colorConfig = Resources.Load<ColorConfig>("ColorConfig");
        model = new ColorModel[3];
    }

    private void Update()
    {
        if(_timer < 0)
        {
            for (int i = 0; i < 3; ++i)
            {
                ColorBlock cb = _button[i].Button.colors;
                model[i] = RandomColorSelection();

                cb.normalColor = model[i].color;
                cb.selectedColor = model[i].color;
                _button[i].Button.colors = cb;
                _button[i].Text.text = model[i].name.ToString() + " " + _button[i].Name;
            }
            _timer = 5f;
        }
        _timer -= Time.deltaTime;
    }

    private ColorModel RandomColorSelection()
    {
        var model = _colorConfig.Get((ColorName)Random.Range(0, _colorConfig.ColorModel.Count));

        return model;
    }
}
