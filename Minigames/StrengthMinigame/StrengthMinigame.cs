using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;

public class StrengthMinigame : Minigame
{
    [SerializeField] public Volume impulse = new(0, 100);
    [SerializeField] private float clickImpulsePerStrengthPoint;
    [SerializeField] private float impulseApplicationTime;
    [Space]
    [SerializeField] [Range(0, 1)] private float bonusZoneStart;
    [SerializeField] [Range(0, 1)] private float bonusZoneEnd;
    [SerializeField] [Range(0, 1)] private float liftingZoneStart;
    [SerializeField] [Range(0, 1)] private float fallZone;
    [Space]
    [SerializeField] public Volume height;
    [SerializeField] private float gravityBase;
    [SerializeField] [RangeAttribute(1, 2)] private float gravityPower;

    private float targetHeight;
    private float gravity;
    private float liftingPower;

    private int strengthPoints =>
        PlayerCharacter.instance.characterInfo.GetAttributes(AttributeName.Strength).value;

    private float clickImpulse => strengthPoints * clickImpulsePerStrengthPoint;

    private void OnValidate()
    {
        GetComponent<StrengthMinigameView>()
            .UpdateZones(fallZone, bonusZoneStart, bonusZoneEnd, liftingZoneStart);
    }

    private void FixedUpdate()
    {
        if (!GameIsOn)
            return;

        gravity += Mathf.Pow((1 + height.Ratio) * gravityBase, gravityPower) * Time.fixedDeltaTime;
        float gravDelta = gravity * Time.fixedDeltaTime;

        if (impulse.Ratio >= liftingZoneStart)
        {
            liftingPower += impulse.current.Value * Time.fixedDeltaTime;

            impulse.Subtract(liftingPower * Time.fixedDeltaTime / 2);
            height.Add(liftingPower * Time.fixedDeltaTime / 2);
        }
        else
        {
            if (liftingPower > 0)
                liftingPower -= gravityBase * Time.fixedDeltaTime;
            else
                liftingPower = 0;
        }

       
        impulse.Subtract(gravDelta);


        if (impulse.Ratio < fallZone)
        {
            height.Subtract(gravDelta);
        }

        if (height.current.Value >= targetHeight)
        {
            gameOver.Value = GameOver.Win;
        }
    }

    public void Reset(float targetHeight)
    {
        this.targetHeight = targetHeight;

        impulse.Empty();
        height.Empty();

        gameOver.Value = GameOver.None;
    }

    public void AddImpulse()
    {
        float cachedClickImpulse = clickImpulse;

        if (IsInBonusZone)
        {
            cachedClickImpulse += gravity;
        }

        gravity = 0;

        this.StartCoroutine(
            AddPowerCoroutine(cachedClickImpulse));

        IEnumerator AddPowerCoroutine(float clickImpulse)
        {
            float prevEase = 0;
            for (float t = 0f; t < 1f; t += Time.fixedDeltaTime / impulseApplicationTime)
            {
                float ease = (t == 1f) ? 1f : 1 - Mathf.Pow(3, -10 * t);

                float deltaEase = ease - prevEase;

                impulse.Add(deltaEase * clickImpulse, out float overflow);

                liftingPower += overflow;

                prevEase = ease;

                yield return null;
            }
        }
    }

    private bool IsInBonusZone =>
        (bonusZoneStart <= impulse.Ratio && impulse.Ratio <= bonusZoneEnd);
}
