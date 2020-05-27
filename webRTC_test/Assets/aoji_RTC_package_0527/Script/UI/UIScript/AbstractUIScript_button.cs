using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System;

[System.Serializable]
public class ButtonData
{
    public string _buttonText;
    public UnityEvent _onClickAction = new UnityEvent();
    public Action _cursorAction;//選択肢にカーソルがあった時のアクション
    public bool isActive=true;

    public Sprite _buttonImage;
    public string _additonalText;

    public ButtonData(string _text, UnityEvent _onclick)
    {
        _buttonText = _text;
        _onClickAction = _onclick;
    }
    public ButtonData(string _text, UnityEvent _onclick,Action _cursor)
    {
        _buttonText = _text;
        _onClickAction = _onclick;
        _cursorAction = _cursor;
    }
    public ButtonData(string _text)
    {
        _buttonText = _text;
    }

    public void SetIsActive(bool flag)
    {
        isActive = flag;
    }
}
public abstract class AbstractUIScript_button : AbstractUIScript
{
    
    [SerializeField] ButtonUnitDisplayer _buttonPrefab;
    [SerializeField] int _buttonDisplayRange;
    [SerializeField,NonEditable] int _buttonRangeStartIndex;//表示範囲の始まり this=4 なら4～の_buttonDataListを表示
    [SerializeField,NonEditable] int _nowSelectButtonIndex;

    List<ButtonData> _buttonDataList = new List<ButtonData>();
    List<GameObject> _displayButtonList = new List<GameObject>();//gameObjectで持っている方が扱いやすい
    RectTransform _myPanel { get { return _MyUIBase._myPanel; } }
    ButtonController _myButtonController { get { return _MyUIBase._myButtonController; } }


    protected override void InitAction()
    {
        base.InitAction();
        _myButtonController._ChengeButtonCallback += CursorAction;
    }

    protected abstract List<ButtonData> CreateMyButtonData();

    protected override void ChengeState_toActive()
    {
        base.ChengeState_toActive();
        var list = CreateMyButtonData();
        if (list != null)
        {
            //ボタンの追加
            ResetButtonData();
            AddButtonData(list);
            SyncButtonToText();
            SetSelectButton();
        }
    }

    protected override void ChengeState_toClose()
    {
        base.ChengeState_toClose();
        //追加されたボタンの削除
        ResetButtonData();
        SyncButtonToText();
        //indexの初期化
        _nowSelectButtonIndex = 0;
        _buttonRangeStartIndex = 0;
    }

    private void Update()
    {
        ButtonIndexUpdate();
    }

    //この下はprivate
    #region rawなボタン操作メソッド
    void AddButtonData(ButtonData data)
    {
        _buttonDataList.Add(data);
    }
    void AddButtonData(List<ButtonData> data)
    {
        _buttonDataList.AddRange(data);
    }
    void ResetButtonData()
    {
        _buttonDataList = new List<ButtonData>();
    }

    void SyncButtonToText()
    {
        ResetButton();
        for (int i = 0; i < _buttonDisplayRange; i++)
        {
            if (_buttonDataList.Count <= i + _buttonRangeStartIndex) break;
            var targetData = _buttonDataList[i + _buttonRangeStartIndex];

            var bt = CreateButton();
            //if (targetData.isActive) bt.onClick.AddListener(() => targetData._onClickAction.Invoke());
            //else DisActive(bt);
            bt.SetDisplayData(targetData._buttonText, targetData._additonalText, targetData._buttonImage);
            if (targetData.isActive) bt.SetOnClick(targetData._onClickAction);
            else bt.SetButtonColors(targetData.isActive);
        }
    }

    //private void DisActive(Button bt)
    //{
    //    var colors = bt.colors;
    //    colors.normalColor = bt.colors.disabledColor;
    //    colors.selectedColor = (colors.disabledColor + colors.selectedColor) / 2;
    //    bt.colors = colors;
    //}

    ButtonUnitDisplayer CreateButton()
    {
        var add = Instantiate(_buttonPrefab, _myPanel);
        _displayButtonList.Add(add.gameObject);
        return add;
    }

    void ResetButton()
    {
        foreach (var bt in _displayButtonList)
        {
            Destroy(bt.gameObject);
        }
        _displayButtonList = new List<GameObject>();
    }
    #endregion
    #region index関連

    void ButtonIndexUpdate()
    {
        _nowSelectButtonIndex = GetCurrentButtonIndex();
        if (_displayButtonList.Count <= 0 || !_myButtonController._InputEnable) return;

        float tate = Input.GetAxisRaw("Vertical");
        if (tate == 0) return;
        bool isDown = tate < 0;

        if (CheckIndex_isRangeUpdateEnable(isDown)
            && CheckIndex_isUpdateRange(isDown))
        {
            if (isDown) _buttonRangeStartIndex++;
            else _buttonRangeStartIndex--;
            SyncButtonToText();
            SetSelectButton();
        }
    }

    int GetCurrentButtonIndex()
    {
        var select = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (_displayButtonList.Contains(select))
        {
            return _displayButtonList.IndexOf(select);
        }
        return _nowSelectButtonIndex;
    }
    
    void SetSelectButton()
    {
        //選択ボタンの設定
        if (_displayButtonList.Count > 0)
        {
            if (_nowSelectButtonIndex < _displayButtonList.Count)
            {
                _myButtonController.SetSelectButton(_displayButtonList[_nowSelectButtonIndex].gameObject);
            }
            else
            {
                _myButtonController.SetSelectButton(_displayButtonList[0].gameObject);
            }
        }
    }


    bool CheckIndex_isRangeUpdateEnable(bool down)
    {
        if (down) return _buttonRangeStartIndex < (_buttonDataList.Count - _buttonDisplayRange);
        else return _buttonRangeStartIndex > 0;
    }
    bool CheckIndex_isUpdateRange(bool down)
    {
        if (down) return _nowSelectButtonIndex + 1 == _buttonDisplayRange;
        else return _nowSelectButtonIndex == 0;
    }
    #endregion

    void CursorAction(GameObject cursor)
    {
        var index = _displayButtonList.IndexOf(cursor);
        if (index >= 0)
        {
            var targetIndex = _buttonRangeStartIndex+index;
            if (_buttonDataList.Count > targetIndex) _buttonDataList[targetIndex]._cursorAction?.Invoke();
        }
    }
}
