using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    IState currentState;

    private void Start()
    {
        ChangeState(new BabyState());
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.OnExit(this);
        }
        
        currentState = newState;
        currentState.OnEnter(this);
    }
}

public interface IState
{
    public void OnEnter(PlayerState controller);

    public void UpdateState(PlayerState controller);

    public void OnExit(PlayerState controller);
}

public class BabyState : IState
{
    public void OnEnter(PlayerState sc)
    { 
    }

    public void UpdateState(PlayerState sc)
    {
        Debug.Log("I'm baby");
    }

    public void OnExit(PlayerState sc)
    {
    }
}

public class AdultState : IState
{
    public void OnEnter(PlayerState sc)
    { 
        Vector3 theScale = sc.transform.localScale;
        theScale.x *= 2;
        sc.transform.localScale = theScale;
    }

    public void UpdateState(PlayerState sc)
    {
        Debug.Log("I'm adult");
    }

    public void OnExit(PlayerState sc)
    {
        Vector3 theScale = sc.transform.localScale;
        theScale.x /= 2;
        sc.transform.localScale = theScale;
    }
}