using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TicTacToe : MonoBehaviour
{
    [Header("Settings")]
    public bool autoplay = false;
    public bool autoRestart = false;
    public bool sendAvailableMoves = false;

    [Header("Language Models")]
    public LanguageModel languageModelX;
    public LanguageModel languageModelO;
    
    [Header("References")]
    public Button nextBtn;
    public TMP_Text boardStateText;
    public TTT_LLMGameLogger loggerPlayerX;
    public TTT_LLMGameLogger loggerPlayerO;
    public TTT_Field[] fields;

    private bool isXTurn = true;
    private bool nextTurnReady = false;
    private bool gameEnded = false;
    private bool autoTriggerNext = false;

    public void Start()
    {
        ResetField();
        
        loggerPlayerO.modelname = languageModelO.name;
        loggerPlayerX.modelname = languageModelX.name;
    }

    private void Update()
    {
        if(autoTriggerNext)
        {
            autoTriggerNext = false;
            Next();
        }
        
        nextBtn.interactable = nextTurnReady || gameEnded;
        boardStateText.text = GetBoardAsString();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Next();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetField();
        }
    }

    public void Next()
    {
        if (gameEnded)
        {
            ResetField();
        }

        if (!nextTurnReady) return;
        var llm = isXTurn ? languageModelX : languageModelO;

        string board = GetBoardAsString();
        var prompt = $"You are Player {(isXTurn ? "X" : "O")}. This is the board state:\n{board}";
        
        if (sendAvailableMoves)
        {
            prompt += $"\nList of available moves:[{GetAvailableMoves()}]";
        }
        
        Debug.Log($"Prompt: {prompt}");

        nextTurnReady = false;
        llm.Generate(prompt, LlmCallback);
    }

    private void LogEvent(EventLogType type)
    {
        var logger = isXTurn ? loggerPlayerX : loggerPlayerO;
        logger.events.Add(new TTT_Event {LogType = type});
    }

    private void LlmCallback(string response)
    {
        HideAllErrorsOnField();
        Debug.Log($"Response: {response}");

        int move = ParseMove(response);
        if (move != -1)
        {
            var selectedIndex = move - 1;
            if (IsValidMove(selectedIndex))
            {
                SetField(selectedIndex, isXTurn ? TTT_Field.State.X : TTT_Field.State.O);
                LogEvent(EventLogType.ValidMove);
                
                var boardWinState = CheckWinCondition();

                if (boardWinState == BoardWinState.X)
                {
                    Debug.Log("X wins!");
                    LogEvent(EventLogType.Win);
                    gameEnded = true;
                }
                else if (boardWinState == BoardWinState.O)
                {
                    Debug.Log("O wins!");
                    LogEvent(EventLogType.Win);
                    gameEnded = true;
                }
                else if (boardWinState == BoardWinState.Draw)
                {
                    Debug.Log("Draw!");
                    LogEvent(EventLogType.Draw);
                    gameEnded = true;
                }

                if (gameEnded)
                {
                    if (!autoRestart)
                    {
                        return;
                    }
                }

                isXTurn = !isXTurn;
                nextTurnReady = true;
            }
            // If there is any error, the next turn is ready but we dont switch player
            else
            {
                fields[selectedIndex].ShowError();
                LogEvent(EventLogType.IllegalMove);
                Debug.LogWarning($"AI attempted illegal move: {move}");
                nextTurnReady = true;
            }
        }
        else
        {   
            LogEvent(EventLogType.InvalidMove);
            Debug.LogError("No valid move found in AI response");
            nextTurnReady = true;
        }

        if (autoplay)
        {
            autoTriggerNext = true;
        }
    }

    public enum BoardWinState
    {
        X,
        O,
        Draw,
        None
    }

    private BoardWinState CheckWinCondition()
    {
        // Check rows
        for (int i = 0; i < 9; i += 3)
        {
            if (fields[i].state != TTT_Field.State.Empty &&
                fields[i].state == fields[i + 1].state &&
                fields[i].state == fields[i + 2].state)
            {
                // TODO: Show the winning line
                fields[i].ShowWin();
                fields[i + 1].ShowWin();
                fields[i + 2].ShowWin();
                return fields[i].state == TTT_Field.State.X ? BoardWinState.X : BoardWinState.O;
            }
        }

        // Check columns
        for (int i = 0; i < 3; i++)
        {
            if (fields[i].state != TTT_Field.State.Empty &&
                fields[i].state == fields[i + 3].state &&
                fields[i].state == fields[i + 6].state)
            {
                // TODO: Show the winning line
                fields[i].ShowWin();
                fields[i + 3].ShowWin();
                fields[i + 6].ShowWin();
                return fields[i].state == TTT_Field.State.X ? BoardWinState.X : BoardWinState.O;
            }
        }

        // Check diagonals
        if (fields[0].state != TTT_Field.State.Empty &&
            fields[0].state == fields[4].state &&
            fields[0].state == fields[8].state)
        {
            // TODO: Show the winning line
            fields[0].ShowWin();
            fields[4].ShowWin();
            fields[8].ShowWin();
            return fields[0].state == TTT_Field.State.X ? BoardWinState.X : BoardWinState.O;
        }

        if (fields[2].state != TTT_Field.State.Empty &&
            fields[2].state == fields[4].state &&
            fields[2].state == fields[6].state)
        {
            // TODO: Show the winning line
            fields[2].ShowWin();
            fields[4].ShowWin();
            fields[6].ShowWin();
            return fields[2].state == TTT_Field.State.X ? BoardWinState.X : BoardWinState.O;
        }

        // Check for draw
        bool isDraw = true;
        for (int i = 0; i < 9; i++)
        {
            if (fields[i].state == TTT_Field.State.Empty)
            {
                isDraw = false;
                break;
            }
        }

        if (isDraw)
        {
            return BoardWinState.Draw;
        }

        // If we've reached here, the game is still ongoing
        return BoardWinState.None;
    }
    
    public string GetAvailableMoves()
    {
        List<int> availableMoves = new List<int>();

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].state == TTT_Field.State.Empty)
            {
                availableMoves.Add(i + 1); // Adding 1 to convert to 1-based index
            }
        }

        return string.Join(",", availableMoves);
    }

    private int ParseMove(string response)
    {
        Match match = Regex.Match(response, @"\b[1-9]\b");

        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        return -1; // No valid move found
    }

    private bool IsValidMove(int index)
    {
        return index is >= 0 and < 9 && fields[index].state == TTT_Field.State.Empty;
    }

    private string GetBoardAsString()
    {
        string board = "";
        foreach (var field in fields)
        {
            switch (field.state)
            {
                case TTT_Field.State.Empty:
                    board += "?";
                    break;
                case TTT_Field.State.X:
                    board += "X";
                    break;
                case TTT_Field.State.O:
                    board += "O";
                    break;
            }
        }

        return board.Insert(3, "\n").Insert(7, "\n");
    }

    public void SetField(int index, TTT_Field.State state)
    {
        fields[index].state = state;
    }

    private void ResetField()
    {
        foreach (var field in fields)
        {
            field.state = TTT_Field.State.Empty;
        }

        //50/50
        isXTurn = Random.Range(0f, 1f) > 0.5f;
        nextTurnReady = true;
        gameEnded = false;
        
        HideAllWinsOnField();
        HideAllErrorsOnField();
    }

    public void HideAllErrorsOnField()
    {
        foreach (var field in fields)
        {
            field.HideError();
        }
    }


    public void HideAllWinsOnField()
    {
        foreach (var field in fields)
        {
            field.HideWin();
        }
    }
}