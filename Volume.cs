using UnityEngine;
using System.Linq;
using UniRx;
using System;

[System.SerializableAttribute]
public class Volume
{
    public FloatReactiveProperty
        maximum,
        current;

    public Volume(float current) : this(current, current)
    {

    }

    public Volume(float current, float maximum)
    {
        this.current = new FloatReactiveProperty(current);
        this.maximum = new FloatReactiveProperty(maximum);
    }

    public IObservable<(float, float, float)> ObserveRatio()
    {
        return
            Observable.CombineLatest(
                current, maximum,
                (cur , max) => (cur, max, Ratio));
    }

    public IObservable<bool> ObserveFull()
    {
        return
            Observable.CombineLatest(
                current, maximum,
                (cur , max) => !IsFullZeroes && IsFull);
    }

    public IObservable<bool> ObserveEmpty()
    {
        return
            current
            .Select(v => !IsFullZeroes && IsEmpty);
    }

    public IObservable<bool> ObserveRefill()
    {
        return
            ObserveFull()
            .Pairwise()
            .Select(isFullPair =>
                    !IsFullZeroes
                    &&
                    isFullPair.Previous == false && isFullPair.Current == true);
    }

    public bool IsFull => (current.Value == maximum.Value);
    public bool IsEmpty => (current.Value == 0);
    public bool IsFullZeroes => (current.Value == 0 && maximum.Value == 0);

    public float Ratio => (current.Value / maximum.Value);
    public float Unfilled => (maximum.Value - current.Value);

    public void Empty()
    {
        current.Value = 0;
    }

    public void Add(float amount, out float overflow)
    {
        overflow = 0;

        if (amount > Unfilled)
            overflow = amount - Unfilled;

        Add(amount);
    }

    public void Add(float amount)
    {
        amount = Mathf.Min(amount, Unfilled);
        current.Value = Clamp(current.Value + amount);
    }

    public void Subtract(float amount)
    {
        current.Value -= Mathf.Min(amount, current.Value);
    }

    public void ResizeAndRefill(float newMaximum)
    {
        maximum.Value = newMaximum;
        current.Value = newMaximum;
    }

    public void Resize(float newMaximum)
    {
        maximum.Value = newMaximum;
        current.Value = Clamp(current.Value);
    }

    public void Refill()
    {
        current.Value = maximum.Value;
    }

    private float Clamp(float amount)
    {
        return Mathf.Clamp(amount, 0, maximum.Value);
    }
}
