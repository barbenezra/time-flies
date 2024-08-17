using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Helpers;
using Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    IState currentState;

    [SerializeField] CharacterController2D BabyController;
    [SerializeField] CharacterController2D ChildController;
    [SerializeField] CharacterController2D AdultController;
    [SerializeField] CharacterController2D OldController;
    
    private GameObject currentStateGameObject;

    private void OnEnable()
    {
        AgeManager.Instance.OnAgeChanged += OnAgeChanged;
    }

    private void OnDisable()
    {
        AgeManager.Instance.OnAgeChanged -= OnAgeChanged;
    }

    void OnAgeChanged(int age)
    {
        IState newState = age switch
        {
            > 60 => new OldState(),
            > 18 => new AdultState(),
            > 6 => new ChildState(),
            _ => new BabyState()
        };

        if (newState != currentState)
        {
            ChangeState(newState);
        }
    }

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
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeState(new BabyState());
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeState(new ChildState());
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeState(new AdultState());
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeState(new OldState());
        }
    }

    private void SetController(IState state)
    {
        if (currentStateGameObject)
        {
            currentStateGameObject.SetActive(false);
        }
        
        CharacterController2D controller = GetController(state);
        currentStateGameObject = controller.gameObject;
        currentStateGameObject.SetActive(true);
        GetComponent<CharacterController2D>().SetController(controller);
    }

    private CharacterController2D GetController(IState state)
    {
        if (state == null) return null;
        
        if (state.GetType() == typeof(BabyState)) return BabyController;
        if (state.GetType() == typeof(ChildState)) return ChildController;
        if (state.GetType() == typeof(AdultState)) return AdultController;
        if (state.GetType() == typeof(OldState)) return OldController;

        return null;
    }

    private void ChangeState(IState newState)
    {
        SetController(newState);

        currentState?.OnExit(this);

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