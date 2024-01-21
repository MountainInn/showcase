using UniRx;
using Unity.VisualScripting;

abstract public class UniRX_Unit<TMessage> : Unity.VisualScripting.Unit
{
    protected CompositeDisposable disposables = new();
    protected StateStack.MachineLayer layer;

    abstract protected void SubscribeToMessages();
    abstract protected void ConcreteRun(TMessage msg, Flow flow);

    public override void Instantiate(GraphReference instance)
    {
        base.Instantiate(instance);
        layer = StateStack.GetLayer(graph.title);
    }

    protected void ResetSubscriptions()
    {
        disposables?.Dispose();
        disposables = new CompositeDisposable();
    }

    protected void Run(TMessage msg)
    {
        var graphRef = layer.scriptMachine.GetReference().AsReference();

        Flow flow = Flow.New(graphRef);

        ConcreteRun(msg, flow);
    }

    protected void SetCurrentNodeGuid()
    {
        layer.currentNodeGuid = this.guid;
    }
}
