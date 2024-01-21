using UniRx;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Sights : MonoBehaviour
{
    [SerializeField]CircleCollider2D _collider;

    public CircleCollider2D Collider { get => _collider ??= GetComponent<CircleCollider2D>(); }
    public ReactiveCollection<Torch> targets {get; protected set;} = new();

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Torch torch)
            && torch != GetComponentInParent<Torch>())
        {
            targets.Add(torch);
        }
    }

    protected void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Torch torch)
            && torch != GetComponentInParent<Torch>())
        {
            targets.Remove(torch);
        }
    }
}
