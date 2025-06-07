using UnityEngine;

public enum GameState
{
    None,
    Roaming,
    Battle
}


public class GameStateController : MonoBehaviour
{

    public GameState CurrentState { get; private set; }

    public bool IsRoaming() => CurrentState == GameState.Roaming;
    public bool IsInBattle() => CurrentState == GameState.Battle;

    void Start()
    {
        SetState(GameState.Roaming);
    }

    void Update()
    {
        //manual switching state
        bool clickedSpace = Input.GetKeyDown(KeyCode.Space);
        if (clickedSpace)
        {
            if (CurrentState == GameState.Roaming)
            {
                SetState(GameState.Battle);
            }
            else if (CurrentState == GameState.Battle)
            {
                SetState(GameState.Roaming);
            }
        }
    }


    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning("Attempted to set the game state to the same value: " + newState);
            return;
        }

        CurrentState = newState;
        Debug.Log("Game state changed to: " + newState);

        SwipeChainSelectAttack swipeChainSelectAttack = FindFirstObjectByType<SwipeChainSelectAttack>();
        MainCharacerController mainCharacerController = FindFirstObjectByType<MainCharacerController>();

        bool enableSwipe = newState == GameState.Battle;
        bool enableMainChar = newState == GameState.Roaming;

        if (swipeChainSelectAttack != null)
            swipeChainSelectAttack.enabled = enableSwipe;

        if (mainCharacerController != null)
            mainCharacerController.enabled = enableMainChar;
    }
}
