using UnityEngine;
using UniRx;

public class StrengthMinigamePresenter
    : MinigamePresenter<StrengthMinigame,
                        StrengthMinigameView,
                        StrengthMinigamePresenter,
                        StrengthSO>
{
    [SerializeField] private KeyCode interactKey;

    protected override void Subscribe()
    {
        model.impulse
            .ObserveRatio()
            .Subscribe(tup => view.UpdatePowerMeter(tup.Item3))
            .AddTo(disposables);

        model.height
            .ObserveRatio()
            .Subscribe(tup => view.UpdateLiftHeight(tup.Item3))
            .AddTo(disposables);
    }

    protected override void StartGame(StrengthSO so)
    {
        model.Reset(so.targetHeight);

        view.UpdateTargetHeight(so.targetHeight);
        view.UpdateShaftHeight(model.height.maximum.Value);
    }

    private void Update()
    {
        if (model != null && model.GameIsOn)
        {
            if (Input.GetKeyDown(interactKey))
            {
                model.AddImpulse();
            }
        }
    }
}
