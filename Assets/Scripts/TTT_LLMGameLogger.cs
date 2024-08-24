using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum EventLogType
{
    ValidMove,
    IllegalMove,
    InvalidMove,
    Win,
    Draw
}

public class TTT_Event
{
    
    
    public EventLogType LogType;
}

public class TTT_LLMGameLogger : MonoBehaviour
{
    public string modelname;
    public List<TTT_Event> events = new List<TTT_Event>();

    public TMP_Text modelnameText;
    public TMP_Text illegalMoveCountText;
    public TMP_Text invalidMoveCountText;
    public TMP_Text winCountText;

    private void Update()
    {
        modelnameText.text = modelname;
        illegalMoveCountText.text = "Illegal moves::" + events.FindAll(e => e.LogType == EventLogType.IllegalMove).Count.ToString();
        invalidMoveCountText.text = "Invalid moves:" + events.FindAll(e => e.LogType == EventLogType.InvalidMove).Count.ToString();
        winCountText.text = "Wins:" + events.FindAll(e => e.LogType == EventLogType.Win).Count.ToString();
    }
}
