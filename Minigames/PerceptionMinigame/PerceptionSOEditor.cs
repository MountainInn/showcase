using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerceptionSO))]
public class PerceptionSOEditor : Editor
{
    private const string previewName = "[PerceptionSOEditor] Map Preview";
    private PerceptionSO so;

    private SpriteRenderer imagePreview;

    private Vector2 positionBeforeEditing;
    private float sizeBeforeEditing;

    private GUIStyle labelStyle;

    private void OnEnable()
    {
        so = (PerceptionSO)target;

        labelStyle = new GUIStyle
        {
            fontSize = 18,
            normal = new GUIStyleState { textColor = Color.cyan }
        };


        try
        {
            SceneView.duringSceneGui += OnSceneGUI;

            if (SceneView.lastActiveSceneView != null)
            {
                positionBeforeEditing = SceneView.lastActiveSceneView.position.center;
                sizeBeforeEditing = SceneView.lastActiveSceneView.size;

                SceneView.lastActiveSceneView.LookAtDirect(Vector3.zero,
                                                           Quaternion.identity,
                                                           3f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PerceptionSOEditor] OnEnable catch: {e}");
        }


        imagePreview =
            GameObject.Find(previewName)?.GetComponent<SpriteRenderer>()
            ??
            new GameObject(previewName).AddComponent<SpriteRenderer>();

        SceneVisibilityManager.instance.Isolate(imagePreview.gameObject, true);
    }

    private void OnDisable()
    {
        if (Selection.Contains(target))
            return;

        try
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.lastActiveSceneView.LookAtDirect(positionBeforeEditing,
                                                       Quaternion.identity,
                                                       sizeBeforeEditing);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PerceptionSOEditor] OnDisable catch: {e}");
        }

        if (imagePreview?.gameObject != null)
            DestroyImmediate(imagePreview.gameObject);
        imagePreview = null;

        SceneVisibilityManager.instance.ExitIsolation();
    }

    private void OnSceneGUI(SceneView sv)
    {
        Handles.color = Color.cyan;

        Sprite sprite = so.mapImage;
        if (sprite != null && imagePreview != null)
        {
            imagePreview.sprite = sprite;
            so.editorPreviewSize = imagePreview.size;
        }

        float capRadius = 0.15f;

        so.segments.Map(seg =>
        {
            for (int i = 0; i < seg.points.Count; i++)
            {
                Handles.color = Color.white;
                seg.points[i].radius = Handles.RadiusHandle(Quaternion.identity,
                                                            seg.points[i].position,
                                                            Mathf.Max(seg.points[i].radius, 0.1f));

                Handles.color = Color.green;
                seg.points[i].position = Handles.FreeMoveHandle(seg.points[i].position,
                                                                Quaternion.identity,
                                                                capRadius, Vector2.zero,
                                                                Handles.CubeHandleCap);

                Handles.color = Color.red;
                if (Handles.Button(seg.points[i].position + Vector2.one * .2f,
                                   Quaternion.identity,
                                   capRadius, capRadius,
                                   Handles.ConeHandleCap))
                {
                    seg.points.RemoveAt(i);
                }


                if (i < seg.points.Count-1)
                {
                    Vector2 va = seg.points[i].position;
                    Vector2 vb = seg.points[i+1].position;
                    Vector2 middlePoint = (va + vb) / 2;

                    Handles.color = Color.yellow;

                    if (Handles.Button(middlePoint,
                                       Quaternion.identity,
                                       0.1f, 0.1f,
                                       Handles.CircleHandleCap))
                    {
                        seg.points.Insert(i+1,
                                          new PerceptionSO.Point{
                                              position = middlePoint,
                                                  radius = Mathf.Lerp(seg.points[i].radius,
                                                                      seg.points[i+1].radius, 0.5f) });
                    }

                    Handles.color = Color.white;

                    float
                        ra = seg.points[i].radius,
                        rb = seg.points[i+1].radius;

                    Vector2 ortho =
                        Quaternion.LookRotation(vb - va)
                        * Quaternion.Euler(0, 0, 90)
                        * Vector2.one;

                    Handles.DrawLine(va + ortho * ra, vb + ortho * rb, 1f);
                    Handles.DrawLine(va - ortho * ra, vb - ortho * rb, 1f);
                }
            }
        });


        so.segments = so.segments.Where(seg => seg.points.Count > 0).ToList();

        var firstPoint =
            so
            .segments.FirstOrDefault()
            ?.points.FirstOrDefault();

        if (firstPoint != null)
            Handles.Label(firstPoint.position - Vector2.one * capRadius, "Start", labelStyle);


        Handles.BeginGUI();

        if (GUI.Button(new Rect(10, 200, 100, 100), "Add Line"))
        {
            so.segments.Add(new PerceptionSO.Segment(){
                    points = new()
                    {
                        new PerceptionSO.Point { radius = 0.2f, position = new Vector2(-1, 1) },
                        new PerceptionSO.Point { radius = 0.2f, position = new Vector2(-1, -1) },
                    }
                });
        }

        Handles.EndGUI();

        if (GUI.changed)
        {
            Undo.RecordObject(target, "Move Point");

            EditorUtility.SetDirty(so);
        }
    }

}
