using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "PerceptionSO", menuName = "SO/MiniGames/PerceptionSO")]
public class PerceptionSO : ScriptableObject
{
    [SerializeField] public Sprite mapImage;
    [SerializeField] public List<Segment> segments = new();
    [SerializeField] public Vector2 editorPreviewSize;

    public IEnumerable<Segment> GetZoomedSegments(Vector2 mapSymbolSize)
    {
        Vector2 scale = mapSymbolSize / editorPreviewSize;

        return
            segments
            .Select(seg => seg.Scaled(scale));
    }

    [Serializable]
    public class Segment
    {
        [SerializeField] public List<Point> points;

        public Segment Scaled(Vector2 scale)
        {
            return new Segment{ points = points.Select(p => p.Scaled(scale)).ToList() };
        }
    }

    [Serializable]
    public class Point
    {
        [SerializeField] public Vector2 position;
        [SerializeField] public float radius;

        public Point Scaled(Vector2 scale)
        {
            return new Point { radius = radius * scale.x,
                               position = position * scale};
        }
    }
}
