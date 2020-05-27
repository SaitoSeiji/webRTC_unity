using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonUnitDisplayer : MonoBehaviour
{
    [SerializeField] Button _myButton;
    [SerializeField] Text _myText;
    [SerializeField] Text _additionalText;
    [SerializeField] Image _myImage;
    
    public void SetDisplayData(string mainText,string addtionalText,Sprite _imageSprite)
    {
        _myText.text = mainText;
        if (_myImage != null)
        {
            _myImage.sprite = _imageSprite;
        }

        if (_additionalText != null)
        {
            _additionalText.text = addtionalText;
        }
    }

    public void SetButtonColors(bool isActive)
    {
        if (isActive) return;
        var colors = _myButton.colors;
        colors.normalColor = _myButton.colors.disabledColor;
        colors.selectedColor = (colors.disabledColor + colors.selectedColor) / 2;
        _myButton.colors = colors;
    }

    public void SetOnClick(UnityEvent ue)
    {
        _myButton.onClick.AddListener(()=>ue.Invoke());
    }
}
