using UnityEngine;
using UniRx;
using System.Linq;

public class PerceptionMinigamePresenter
    : MinigamePresenter<PerceptionMinigame,
                        PerceptionMinigameView,
                        PerceptionMinigamePresenter,
                        PerceptionSO>
{
    public Torch prefabTorch;

    protected override void Subscribe()
    {
        model.energy
            .ObserveRatio()
            .Subscribe(tup =>
                       view.energyBar.fillAmount = tup.Item3)
            .AddTo(disposables);
    }

    protected override void StartGame(PerceptionSO perceptionSO)
    {
        view.mapSymbol.sprite = perceptionSO.mapImage;

        model.Reset();
        model
            .GetTorchSpawnArgs(perceptionSO, view.mapSymbol.rectTransform.sizeDelta)
            .Select(args => InstantiateTorch(args.Item1, args.Item2))
            .Map(t =>
                 t.button.onClick.AddListener(() =>
                 {
                     if (!t.isLit)
                         t.LightUp();
                     else
                     {
                         var wouldUnwitness = t.GetWouldBeUnWitnessedTorches();

                         bool
                             selfUnwitness = (t.witnessCount.Value == 1),
                             safe = !(wouldUnwitness.Any() || selfUnwitness);

                         if (wouldUnwitness.Any())
                             wouldUnwitness.Map(view.StartBlink);

                         if (selfUnwitness)
                             view.StartBlink(t);

                         if (safe)
                         {
                             model.energy.Add(t.Douse());
                         }
                     }
                 })
            );

        model.Initialize();
    }

    private Torch InstantiateTorch(Vector2 point, bool isPivot)
    {
        Torch newTorch = Instantiate(prefabTorch,
                                     point,
                                     Quaternion.identity,
                                     view.mapSymbol.transform);

        newTorch.transform.localScale = Vector3.one;
        newTorch.transform.localPosition = point;
        newTorch.isPivot = isPivot;
        model.torches.Add(newTorch);

        return newTorch;
    }

    private void Update()
    {
        if (model?.GameIsOn ?? false)
        {
            view.UpdateTorchShader(model.torches);
        }
    }
}
