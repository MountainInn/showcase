using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerceptionMinigameView : MonoBehaviour
{
    public Image mapSymbol, energyBar;

    private Material material;
    private int torchesProperty;
    private Vector4[] torchesVectors = new Vector4[32];

    private void Awake()
    {
        material = mapSymbol.material;
        torchesProperty = Shader.PropertyToID("_Torches");
    }

    public void StartBlink(Torch torch)
    {
        StartCoroutine(Blink(torch));
    }

    private IEnumerator Blink(Torch torch)
    {
        Image image = torch.button.image;
        Color baseColor = image.color;
        float duration = 0.25f;

        image.CrossFadeColor(Color.red, duration, true, false);

        yield return new WaitForSeconds(duration);

        image.CrossFadeColor(baseColor, duration, true, false);
    }

    public void UpdateTorchShader(List<Torch> torches)
    {
        var size = mapSymbol.rectTransform.sizeDelta;

        for (int i = 0; i < torchesVectors.Length; i++)
        {
            if (i < torches.Count)
            {
                torchesVectors[i].x = torches[i].transform.localPosition.x / size.x + 0.5f;
                torchesVectors[i].y = torches[i].transform.localPosition.y / size.y + 0.5f;
                torchesVectors[i].z = torches[i].lightness.current.Value / size.x;
            }
            else
            {
                torchesVectors[i] = default;
            }
        }

        material.SetVectorArray(torchesProperty, torchesVectors);
    }
}
