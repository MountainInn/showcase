using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class PerceptionMinigame : Minigame
{
    public Volume energy;
    public float energyDrainPerSecond;
    public float growthPerSecond;
    public float torchRadiusPerPerceptionPoint;

    [HideInInspector] public List<Torch> torches = new();

    private float maxTorchRadius =>
        torchRadiusPerPerceptionPoint * perceptionPoints;

    private int perceptionPoints =>
        PlayerCharacter.instance.characterInfo.GetAttributes(AttributeName.Perception).value;

    public void Reset()
    {
        torches.DestroyAll();
        energy.Refill();
    }

    public IEnumerable<(Vector2, bool)> GetTorchSpawnArgs(PerceptionSO perceptionSO,
                                                          Vector2 mapSymbolSize)
    {
        float minRadiusFraction = .7f;
        float minDiameterFraction = .7f * 2f;

        IEnumerable<PerceptionSO.Segment> zoomedSegments =
            perceptionSO
            .GetZoomedSegments(mapSymbolSize);

        IEnumerable<(Vector2, bool)> pivotTorches =
            zoomedSegments
            .SelectMany(seg =>
                        seg.points.Select(p => (p.position, true)));

        List<(Vector2, bool)> intermidiateTorches = new();

        zoomedSegments
            .Map(seg =>
            {
                seg.points.Aggregate((a, b) =>
                {
                    Vector2
                        posA = a.position,
                        posB = b.position;

                    float
                        radiusA = a.radius;

                    Quaternion rotation =
                        Quaternion.LookRotation(posB - posA) * Quaternion.Euler(0, 0, 90);

                    float steps;
                    while ((steps = Vector2.Distance(posA, posB) / maxTorchRadius)
                           > minDiameterFraction)
                    {
                        float hypothenuse = UnityEngine.Random.Range(minRadiusFraction, 1f) * maxTorchRadius;
                        float bkatet = UnityEngine.Random.Range(-1f, 1f) * radiusA;
                        float akatet = Mathf.Sqrt(Mathf.Pow(hypothenuse, 2) - Mathf.Pow(bkatet, 2));

                        float t = akatet / hypothenuse / steps;
                        Vector2 ortho = rotation * Vector2.one * bkatet;

                        posA = Vector2.Lerp(posA, posB, t) + ortho;

                        intermidiateTorches.Add((posA, false));
                    }

                    return b;
                });
            });

        return pivotTorches.Concat(intermidiateTorches.AsEnumerable());
    }

    public void Initialize()
    {
        torches.First().witnessCount.Value = 1;

        torches.Map(t => t.lightness.Resize(maxTorchRadius));

        torches.First().LightUp();

        gameOver.Value = GameOver.None;
    }


    private void Update()
    {
        if (gameOver.Value != GameOver.None
            || !torches.Any())
            return;

        float energyDrainDelta = energyDrainPerSecond * Time.deltaTime;
        float lightnessDelta = growthPerSecond * Time.deltaTime;

        torches.Map(t =>
        {
            if (t.isLit)
            {
                energy.Subtract(energyDrainDelta);

                t.lightness.Add(lightnessDelta);

                if (!t.lightness.IsFull)
                    t.energySpentOnGrowth += energyDrainDelta;
            }
        });

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        bool allPivotTorchesWitnessed =
            torches
            .Where(t => t.isPivot)
            .All(t => t.isWitnessed);

        if (allPivotTorchesWitnessed)
        {
            gameOver.Value = GameOver.Win;
        }
        else if (energy.IsEmpty)
        {
            gameOver.Value = GameOver.Lose;
        }
    }

}
