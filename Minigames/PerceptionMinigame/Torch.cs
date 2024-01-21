using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System.Linq;

public class Torch : MonoBehaviour
{
    public bool isLit, isPivot;
    public bool isWitnessed => (witnessCount.Value > 0);
    public float energySpentOnGrowth;
    public Button button;
    public ReactiveProperty<int> witnessCount = new();

    public Sights sights;

    [HideInInspector] public Volume lightness = new(0);

    void OnValidate()
    {
        if (GetComponentInChildren<Sights>() == null)
        {
            sights = new GameObject("Sights")
                .AddComponent<Sights>();

            sights.transform.SetParent(transform);
            sights.transform.position = default;
            sights.transform.localScale = Vector3.one;
        }
    }

    void Start()
    {
        sights = GetComponentInChildren<Sights>();
        
        lightness.ObserveRatio()
            .Subscribe(tup =>
            {
                float current = tup.Item1;
                sights.Collider.radius = current;
            })
            .AddTo(this);

        sights.targets.ObserveAdd()
            .Subscribe(ev =>
            {
                ev.Value.witnessCount.Value++;
            })
            .AddTo(this);

        sights.targets.ObserveRemove()
            .Subscribe(ev =>
            {
                ev.Value.witnessCount.Value--;
            })
            .AddTo(this);

        witnessCount
            .Subscribe(w => ToggleVisibility(w > 0))
            .AddTo(this);
    }

    public IEnumerable<Torch> GetWouldBeUnWitnessedTorches()
    {
        return sights.targets.Where(t => t.witnessCount.Value == 1);
    }

    void ToggleVisibility(bool toggle)
    {
        button.image.enabled = button.interactable = toggle;
    }

    public void LightUp()
    {
        isLit = true;
        witnessCount.Value++;
    }

    public float Douse()
    {
        isLit = false;

        lightness.current.Value = 0;
        witnessCount.Value--;

        return energySpentOnGrowth;
    }
}
