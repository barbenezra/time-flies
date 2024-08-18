using Game.Scripts.Managers;
using UnityEngine;

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

        if (newState.GetType() != currentState?.GetType())
        {
            ChangeState(newState);
        }
    }

    void Update()
    {
        currentState?.UpdateState(this);
    }

    private void SetController(IState state)
    {
        currentStateGameObject?.SetActive(false);
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
        SoundManager.Instance.Play("Grow");
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