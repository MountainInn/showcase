using UnityEngine;
using UniRx;

abstract public class Minigame : MonoBehaviour
{
    [HideInInspector] public ReactiveProperty<GameOver> gameOver = new(GameOver.None);

    public bool GameIsOn => (gameOver.Value == GameOver.None);
}
