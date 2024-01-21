using UniRx;
using UnityEngine;

abstract public class MinigamePresenter<TModel, TView, TPresenter, TSettingsSO> : MonoBehaviour
    where TPresenter : MonoBehaviour
    where TView : MonoBehaviour
    where TModel : Minigame
{
    static public TPresenter instance => _inst ??= FindObjectOfType<TPresenter>();
    static TPresenter _inst;

    [SerializeField] protected TView prefabViewAndModel;
    [SerializeField] [HideInInspector] protected TView view;
    [SerializeField] [HideInInspector] protected TModel model;

    protected CompositeDisposable disposables = new();

    protected void InstantiateViewAndModel()
    {
        view = Instantiate(prefabViewAndModel, transform);
        model = view.GetComponent<TModel>();
    }

    public void MainStartGame(TSettingsSO gameSettings)
    {
        InstantiateViewAndModel();

        disposables.Dispose();
        disposables = new();

        Subscribe();
        SubscribeOnGameOver();

        StartGame(gameSettings);
    }

    abstract protected void Subscribe();

    abstract protected void StartGame(TSettingsSO gameSettings);

    void SubscribeOnGameOver()
    {
        model.gameOver
            .Subscribe(GameOverCallback)
            .AddTo(disposables);
    }

    void GameOverCallback(GameOver g)
    {
        bool
            win =  (g == GameOver.Win),
            lose = (g == GameOver.Lose),
            any =  (win || lose);

        if (any)
        {
            DestroyViewAndModel();
        }

        if (win)
        {
            MessageBroker.Default.Publish(
                new Msg_MinigameResult() { success = true });
        }
        else if (lose)
        {
            MessageBroker.Default.Publish(
                new Msg_MinigameResult() { success = false });
        }
    }

    void DestroyViewAndModel()
    {
        GameObject.Destroy(view.gameObject);
    }
}
