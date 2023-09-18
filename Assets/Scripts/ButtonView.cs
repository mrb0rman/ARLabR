using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonView : MonoBehaviour
{
    public Button Button => button;
    public TMP_Text Text => text;
    public string Name;

    [SerializeField] private Button button;
    [SerializeField] private  TMP_Text text;
}
