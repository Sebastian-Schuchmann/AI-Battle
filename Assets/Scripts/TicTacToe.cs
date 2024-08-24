using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LLMs.Src;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class TicTacToe : MonoBehaviour
{
    public event Action<ReplayStep> OnMove;
    
    [Header("Settings")]
    public bool autoplay = false;
    public bool autoRestart = false;
    public bool sendAvailableMoves = false;
    public float timeBetweenRounds = 3f;
    public bool illegalOrInvalidMovesCountAsMoves = false;

    [Header("Language Models")]
    public LanguageModel languageModelX;
    public LanguageModel languageModelO;
    
    [Header("References")]
    public TMP_Text boardStateText;
    public TTT_LLMGameLogger loggerPlayerX;
    public TTT_LLMGameLogger loggerPlayerO;
    public TTT_Field[] fields;
    public GameObject turnIndicatorX, turnIndicatorO;

    private bool isXTurn = true;
    private bool nextTurnReady = false;
    private bool gameEnded = false;
    private bool autoTriggerNext = false;

    private bool replayMode;
/*
 *
 *You are Player O. This is the board state:\n['X','O','X ']\n['O','X','O']\n['O','X',' ']\nList of available moves:[9]
 */
    public void Start()
    {
        if (languageModelX is ReplayLLM replayLlm)
        {
            replayLlm.Parse();
            loggerPlayerO.modelname = replayLlm.replay.modelnameO;
            loggerPlayerX.modelname = replayLlm.replay.modelnameX;
            replayMode = true;
            
            Debug.Log("Replay Mode");
        }
        else
        {
            loggerPlayerO.modelname = languageModelO.name;
            loggerPlayerX.modelname = languageModelX.name;
        }
        
        ResetField();
        if (autoplay) StartCoroutine(Next());
    }

    private void Update()
    {
        if(autoTriggerNext)
        {
            autoTriggerNext = false;
            StartCoroutine(Next());
        }
        
        boardStateText.text = GetBoardAsString();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Next());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetField();
        }
        
        turnIndicatorX.SetActive(isXTurn);
        turnIndicatorO.SetActive(!isXTurn);
    }

    private ReplayStep currentReplayStep;

    public IEnumerator Next()
    {
        if (gameEnded)
        {
            gameEnded = false;
            yield return new WaitForSeconds(timeBetweenRounds);
            ResetField();
        }

        if (!nextTurnReady) yield break;
        nextTurnReady = false;

        if (currentReplayStep != null)
        {
            currentReplayStep.boardStateAfter = GetBoardAsString();
            if(!replayMode ) OnMove?.Invoke(currentReplayStep);
            currentReplayStep = null;
        }

        currentReplayStep = new ReplayStep();
        currentReplayStep.isXTurn = isXTurn;
            
        var llm = isXTurn ? languageModelX : languageModelO;
        currentReplayStep.modelname = llm.name;

        string board = GetBoardAsString();
        currentReplayStep.boardStateBefore = board;
        
        var prompt = $"You are Player {(isXTurn ? "X" : "O")}. This is the board state:\n{board}";
        currentReplayStep.prompt = prompt;
        
        if (sendAvailableMoves)
        {
            prompt += $"\nList of available moves:[{GetAvailableMoves()}]";
        }
        
        Debug.Log($"Prompt: {prompt}");

        llm.Generate(prompt, LlmCallback);
    }

    private void LogEvent(EventLogType type)
    {
        var logger = isXTurn ? loggerPlayerX : loggerPlayerO;
        logger.events.Add(new TTT_Event {LogType = type});
    }
    
    private void LogResponse(string response)
    {
        var logger = isXTurn ? loggerPlayerX : loggerPlayerO;
        logger.AddResponse(response);
    }

    private void LlmCallback(string response)
    {
        currentReplayStep.response = response;
        
        if (response == null)
        {
            Debug.LogError("Error in response");
            nextTurnReady = true;
            
            if (autoplay)
            {
                autoTriggerNext = true;
            }
            
            return;
        }
       
        HideAllErrorsOnField();
        LogResponse(response);
        
        Debug.Log($"Response: {response}");

        int move = ParseMove(response);
        if (move != -1)
        {
            var selectedIndex = move;
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
            else
            {
                fields[selectedIndex].ShowError();
                LogEvent(EventLogType.IllegalMove);
                Debug.LogWarning($"AI attempted illegal move: {move}");
                nextTurnReady = true;
                
                if (illegalOrInvalidMovesCountAsMoves)
                {
                    isXTurn = !isXTurn;
                    Debug.LogWarning($"Switching player turn due to illegal move: {move}");
                }
            }
        }
        else
        {   
            LogEvent(EventLogType.InvalidMove);
            Debug.LogWarning("No valid move found in AI response");
            nextTurnReady = true;
            
            if (illegalOrInvalidMovesCountAsMoves)
            {
                isXTurn = !isXTurn;
                Debug.LogWarning($"Switching player turn due to illegal move: {move}");
            }
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
                availableMoves.Add(i);
            }
        }

        return string.Join(",", availableMoves);
    }
    
    public int[] GetAvailableMovesAsArray()
    {
        List<int> availableMoves = new List<int>();

        for (int i = 0; i < fields.Length; i++)
        {   
            if (fields[i].state == TTT_Field.State.Empty)
            {
                availableMoves.Add(i);
            }
        }

        return availableMoves.ToArray();
    }

    private int ParseMove(string response)
    {
        //Each system prompt has its own regex options because the requested response format can be different
        var regex = isXTurn ? languageModelX.systemPrompt.RegexOptions : languageModelO.systemPrompt.RegexOptions;
        return regex.ParseMove(response);
    }
    
    private bool IsValidMove(int index)
    {
        return index is >= 0 and < 9 && fields[index].state == TTT_Field.State.Empty;
    }

    /* TODO: Implement this function
     * [' ','X','O']
     * ['O','X','X']
     * ['O',' ','O']
     */
    private string GetBoardAsString()
    {
        string[] rows = new string[3];
        for (int i = 0; i < 3; i++)
        {
            char[] row = new char[3];
            for (int j = 0; j < 3; j++)
            {
                int index = i * 3 + j;
                switch (fields[index].state)
                {
                    case TTT_Field.State.Empty:
                        row[j] = ' ';
                        break;
                    case TTT_Field.State.X:
                        row[j] = 'X';
                        break;
                    case TTT_Field.State.O:
                        row[j] = 'O';
                        break;
                }
            }
            rows[i] = $"['{row[0]}','{row[1]}','{row[2]}']";
        }
        return string.Join("\n", rows);
    }

    private string GetBoardAsString_old()
    {
        string board = "";
        foreach (var field in fields)
        {
            switch (field.state)
            {
                case TTT_Field.State.Empty:
                    board += ".";
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
        if (replayMode)
        {
            isXTurn = (languageModelX as ReplayLLM).GetCurrentIsXTurn();
        }
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