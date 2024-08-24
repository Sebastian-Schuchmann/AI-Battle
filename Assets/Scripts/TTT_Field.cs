using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTT_Field : MonoBehaviour
{
    public enum State
    {
        Empty,
        X,
        O
    }
    
    public State state = State.Empty;
    
    public GameObject x;
    public GameObject o;
    public GameObject error;
    public GameObject win;
    
    public void ShowError()
    {
        error.SetActive(true);
    }
    
    public void HideError()
    {
        error.SetActive(false);
    }
    
    public void ShowWin()
    {
        win.SetActive(true);
    }
    
    public void HideWin()
    {
        win.SetActive(false);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Empty:
                x.SetActive(false);
                o.SetActive(false);
                break;
            case State.X:
                x.SetActive(true);
                o.SetActive(false);
                break;
            case State.O:
                x.SetActive(false);
                o.SetActive(true);
                break;
        }
    }
}
