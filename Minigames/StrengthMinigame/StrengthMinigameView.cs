using UnityEngine;
using UnityEngine.UI;

public class StrengthMinigameView : MonoBehaviour
{
    [SerializeField] public Image shaft, lift, powerMeter, targetHeight,
    bonusZone, fallZone, liftZone;

    public void UpdateZones(float fallZoneEnd, float bonusStart, float bonusEnd, float liftZoneStart)
    {
        var size = fallZone.rectTransform.sizeDelta;
        var pos = fallZone.rectTransform.anchoredPosition;

        fallZone.rectTransform.anchoredPosition = new Vector2(pos.x, 0);
        fallZone.rectTransform.sizeDelta = new Vector2(size.x, fallZoneEnd * lift.rectTransform.sizeDelta.y);

        size = liftZone.rectTransform.sizeDelta;
        pos = liftZone.rectTransform.anchoredPosition;

        liftZone.rectTransform.anchoredPosition = new Vector2(pos.x, liftZoneStart * lift.rectTransform.sizeDelta.y);
        liftZone.rectTransform.sizeDelta = new Vector2(size.x, (1f - liftZoneStart) * lift.rectTransform.sizeDelta.y);

        size = bonusZone.rectTransform.sizeDelta;
        pos = bonusZone.rectTransform.anchoredPosition;

        bonusZone.rectTransform.anchoredPosition = new Vector2(pos.x, bonusStart * lift.rectTransform.sizeDelta.y);
        bonusZone.rectTransform.sizeDelta = new Vector2(size.x, (bonusEnd - bonusStart) * lift.rectTransform.sizeDelta.y);
    }

    public void UpdateShaftHeight(float height)
    {
        var size = shaft.rectTransform.sizeDelta;

        shaft.rectTransform.sizeDelta = new Vector2(size.x, height);
    }

    public void UpdateTargetHeight(float height)
    {
        var position = new Vector2(0, height);

        targetHeight.rectTransform.anchoredPosition = position;
    }

    public void UpdateLiftHeight(float ratio)
    {
        var position = new Vector2(0, shaft.rectTransform.sizeDelta.y * ratio);

        lift.rectTransform.anchoredPosition = position;
    }

    public void UpdatePowerMeter(float ratio)
    {
        var position = new Vector2(0, lift.rectTransform.sizeDelta.y * ratio);

        powerMeter.rectTransform.anchoredPosition = position;
    }
}
