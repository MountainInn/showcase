using UniRx;
using Unity.VisualScripting;
using UnityEngine;

abstract public class StartMinigame<TModel, TView, TPresenter, TSettingsSO, TMessage> : UniRX_Unit<TMessage>
    where TModel : Minigame
    where TView : MonoBehaviour
    where TPresenter : MinigamePresenter<TModel, TView, TPresenter, TSettingsSO>
    where TMessage : IResultMsg
{
    [PortLabelHiddenAttribute]
    public ControlInput enter;

    public ControlOutput success, fail;

    [UnitHeaderInspectableAttribute]
    public TSettingsSO gameSettings;

    protected override void Definition()
    {
        enter = ControlInput("enter", (flow) =>
        {
            SubscribeToMessages();

            MinigamePresenter<TModel, TView, TPresenter, TSettingsSO>
                .instance
                .MainStartGame(gameSettings);

            return null;
        });

        success = ControlOutput("Success");
        fail = ControlOutput("Fail");
    }

    protected override void SubscribeToMessages()
    {
        MessageBroker.Default.Receive<TMessage>()
            .Subscribe(msg => Run(msg))
            .AddTo(disposables);
    }

    protected override void ConcreteRun(TMessage result, Flow flow)
    {
        ResetSubscriptions();

        if (result.success)
            flow.Run(success);
        else
            flow.Run(fail);
    }
}
