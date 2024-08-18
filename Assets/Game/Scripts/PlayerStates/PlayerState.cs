using Game.Scripts.Managers;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    IState currentState;

    [SerializeField] CharacterController2D characterController;

    [SerializeField] CharacterController2D BabyController;
    [SerializeField] CharacterController2D ChildController;
    [SerializeField] CharacterController2D AdultController;
    [SerializeField] CharacterController2D OldController;

    private CharacterController2D currentCharacterController;

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

        if (newState.GetType() != currentState?.GetType())
        {
            ChangeState(newState);
        }
    }

    void Start()
    {
        ChangeState(new BabyState());
    }

    void Update()
    {
        currentState?.UpdateState(this);
    }

    private void SetController(IState state)
    {
        currentCharacterController?.gameObject.SetActive(false);
        currentCharacterController = GetController(state);
        currentCharacterController.gameObject.SetActive(true);

        characterController.SetController(currentCharacterController);
    }

    private CharacterController2D GetController(IState state)
    {
        if (state.GetType() == typeof(BabyState)) return BabyController;
        if (state.GetType() == typeof(ChildState)) return ChildController;
        if (state.GetType() == typeof(AdultState)) return AdultController;
        if (state.GetType() == typeof(OldState)) return OldController;

        return BabyController;
    }

    private void ChangeState(IState newState)
    {
        SetController(newState);

        currentState?.OnExit(this);

        currentState = newState;
        currentState.OnEnter(this);

        SoundManager.Instance.Play("Grow");
    }
}

public interface IState
{
    public void OnEnter(PlayerState controller);

    public void UpdateState(PlayerState controller);

    public void OnExit(PlayerState controller);
}