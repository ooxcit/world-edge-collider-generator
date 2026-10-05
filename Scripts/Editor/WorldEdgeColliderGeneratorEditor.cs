#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    [CustomEditor(typeof(WorldEdgeCollidersGenerator))]
    public class WorldEdgeCollidersGeneratorEditor : UnityEditor.Editor
    {
        private const float PointHandleSize = 0.08f;
        private const float InsertHandleSize = 0.05f;
        private const float DragThresholdPixels = 6f;

        private static readonly Color LineColor = new(0.2f, 0.9f, 1f, 1f);
        private static readonly Color ClosingLineColor = new(0.2f, 0.9f, 1f, 0.45f);
        private static readonly Color PointColor = new(1f, 1f, 1f, 1f);
        private static readonly Color SelectedPointColor = new(1f, 0.8f, 0.1f, 1f);
        private static readonly Color InsertColor = new(0.4f, 1f, 0.4f, 0.8f);
        private static readonly Color PreviewColor = new(1f, 0.8f, 0.1f, 0.9f);

        private bool _isPlacingPoints;
        private int _selectedIndex = -1;
        private bool _hasPreview;
        private Vector3 _previewWorldPoint;
        private Vector2 _rightMouseDownPosition;
        private Rect _overlayRect;
        private bool _suppressContextClick;

        private WorldEdgeCollidersGenerator Generator => (WorldEdgeCollidersGenerator)target;

        private void OnDisable()
        {
            SetPlacing(false);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = Generator;

            GUILayout.Space(10);

            var previousColor = GUI.backgroundColor;
            if (_isPlacingPoints)
            {
                GUI.backgroundColor = SelectedPointColor;
            }
            var label = _isPlacingPoints ? "Stop Placing Points (Esc)" : "Place Points";
            if (GUILayout.Button(label, GUILayout.Height(28)))
            {
                SetPlacing(!_isPlacingPoints);
            }
            GUI.backgroundColor = previousColor;

            EditorGUILayout.HelpBox(
                _isPlacingPoints
                    ? "Click in the Scene view to add points. Backspace removes the last point. " +
                      "Esc, Enter or right-click finishes."
                    : "Drag points to move them (snaps to surfaces, hold Shift to stay level). " +
                      "Click a point to select it, then press Delete to remove it. " +
                      "Click a green dot on a segment to insert a point.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(generator.Points.Count == 0))
            {
                if (GUILayout.Button("Clear Points"))
                {
                    Undo.RecordObject(generator, "Clear Points");
                    generator.ClearPoints();
                    _selectedIndex = -1;
                    EditorUtility.SetDirty(generator);
                    SceneView.RepaintAll();
                }
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Generate Box Colliders"))
            {
                generator.GenerateBoxColliders();
            }
            if (GUILayout.Button("Clear Box Colliders"))
            {
                generator.ClearBoxColliders();
            }
        }

        private void OnSceneGUI()
        {
            var generator = Generator;
            var points = generator.Points;
            if (_selectedIndex >= points.Count)
            {
                _selectedIndex = -1;
            }

            // A right-click that finished placing must not also open the Scene view context menu.
            if (Event.current.type == EventType.MouseDown)
            {
                _suppressContextClick = false;
            }
            else if (Event.current.type == EventType.ContextClick && _suppressContextClick)
            {
                _suppressContextClick = false;
                Event.current.Use();
            }

            // The overlay goes first so its buttons get mouse events before the Scene view handles.
            DrawSceneOverlay(generator);
            DrawPath(generator);

            if (_isPlacingPoints)
            {
                HandlePlacing(generator);
            }
            else
            {
                HandleEditing(generator);
            }
        }

        #region Placing

        private void HandlePlacing(WorldEdgeCollidersGenerator generator)
        {
            var currentEvent = Event.current;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            if (currentEvent.isMouse && _overlayRect.Contains(currentEvent.mousePosition))
            {
                _hasPreview = false;
                return;
            }

            switch (currentEvent.type)
            {
                case EventType.Layout:
                    break;

                case EventType.MouseMove:
                case EventType.MouseDrag:
                    _hasPreview = TryGetPlacementPoint(currentEvent.mousePosition, LastPointHeight(generator), out _previewWorldPoint);
                    SceneView.RepaintAll();
                    break;

                case EventType.MouseDown when currentEvent.button == 0 && !currentEvent.alt:
                    if (TryGetPlacementPoint(currentEvent.mousePosition, LastPointHeight(generator), out var worldPoint))
                    {
                        Undo.RecordObject(generator, "Add Point");
                        generator.AddPoint(generator.transform.InverseTransformPoint(worldPoint));
                        EditorUtility.SetDirty(generator);
                    }
                    currentEvent.Use();
                    break;

                case EventType.MouseDown when currentEvent.button == 1:
                    _rightMouseDownPosition = currentEvent.mousePosition;
                    break;

                // Right-click without dragging (dragging right-click is used to look around) finishes placing.
                case EventType.MouseUp when currentEvent.button == 1:
                    if ((currentEvent.mousePosition - _rightMouseDownPosition).magnitude < DragThresholdPixels)
                    {
                        SetPlacing(false);
                        _suppressContextClick = true;
                        currentEvent.Use();
                    }
                    break;

                case EventType.KeyDown:
                    switch (currentEvent.keyCode)
                    {
                        case KeyCode.Escape:
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            SetPlacing(false);
                            currentEvent.Use();
                            break;
                        case KeyCode.Backspace:
                        case KeyCode.Delete:
                            RemoveLastPoint(generator);
                            currentEvent.Use();
                            break;
                    }
                    break;

                case EventType.Repaint:
                    DrawPlacementPreview(generator);
                    break;
            }
        }

        private void DrawPlacementPreview(WorldEdgeCollidersGenerator generator)
        {
            if (!_hasPreview)
            {
                return;
            }

            var points = generator.Points;
            var generatorTransform = generator.transform;
            var size = HandleUtility.GetHandleSize(_previewWorldPoint) * PointHandleSize;

            using (new Handles.DrawingScope(PreviewColor))
            {
                Handles.SphereHandleCap(0, _previewWorldPoint, Quaternion.identity, size * 2f, EventType.Repaint);
                if (points.Count > 0)
                {
                    var last = generatorTransform.TransformPoint(points[^1]);
                    Handles.DrawAAPolyLine(3f, last, _previewWorldPoint);
                }
                if (points.Count > 1)
                {
                    var first = generatorTransform.TransformPoint(points[0]);
                    Handles.DrawDottedLine(_previewWorldPoint, first, 4f);
                }
            }
        }

        private static void RemoveLastPoint(WorldEdgeCollidersGenerator generator)
        {
            if (generator.Points.Count == 0)
            {
                return;
            }
            Undo.RecordObject(generator, "Remove Point");
            generator.RemovePointAt(generator.Points.Count - 1);
            EditorUtility.SetDirty(generator);
        }

        private void SetPlacing(bool isPlacing)
        {
            if (_isPlacingPoints == isPlacing)
            {
                return;
            }

            _isPlacingPoints = isPlacing;
            _hasPreview = false;
            _selectedIndex = -1;
            Tools.hidden = isPlacing;

            if (isPlacing)
            {
                // Make sure the Scene view receives key presses (Esc, Enter, Backspace) right away.
                SceneView.lastActiveSceneView?.Focus();
            }

            Repaint();
            SceneView.RepaintAll();
        }

        #endregion Placing

        #region Editing

        private void HandleEditing(WorldEdgeCollidersGenerator generator)
        {
            var points = generator.Points;
            var generatorTransform = generator.transform;
            var currentEvent = Event.current;

            for (var i = 0; i < points.Count; i++)
            {
                var worldPoint = generatorTransform.TransformPoint(points[i]);
                var size = HandleUtility.GetHandleSize(worldPoint) * PointHandleSize;
                var controlId = GUIUtility.GetControlID(FocusType.Passive);

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 &&
                    HandleUtility.nearestControl == controlId)
                {
                    _selectedIndex = i;
                    Repaint();
                }

                using (new Handles.DrawingScope(i == _selectedIndex ? SelectedPointColor : PointColor))
                {
                    EditorGUI.BeginChangeCheck();
                    Handles.FreeMoveHandle(controlId, worldPoint, size * 2f, Vector3.zero, Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck() &&
                        TryGetPlacementPoint(currentEvent.mousePosition, worldPoint.y, out var newWorldPoint))
                    {
                        Undo.RecordObject(generator, "Move Point");
                        generator.SetPoint(i, generatorTransform.InverseTransformPoint(newWorldPoint));
                        EditorUtility.SetDirty(generator);
                    }
                }
            }

            // Full position handle on the selected point for precise (including vertical) adjustments.
            if (_selectedIndex >= 0)
            {
                var worldPoint = generatorTransform.TransformPoint(points[_selectedIndex]);
                EditorGUI.BeginChangeCheck();
                var newWorldPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(generator, "Move Point");
                    generator.SetPoint(_selectedIndex, generatorTransform.InverseTransformPoint(newWorldPoint));
                    EditorUtility.SetDirty(generator);
                }
            }

            DrawInsertHandles(generator);

            if (currentEvent.type == EventType.KeyDown && _selectedIndex >= 0 &&
                (currentEvent.keyCode == KeyCode.Delete || currentEvent.keyCode == KeyCode.Backspace))
            {
                Undo.RecordObject(generator, "Remove Point");
                generator.RemovePointAt(_selectedIndex);
                EditorUtility.SetDirty(generator);
                _selectedIndex = -1;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape && _selectedIndex >= 0)
            {
                _selectedIndex = -1;
                currentEvent.Use();
            }
        }

        private void DrawInsertHandles(WorldEdgeCollidersGenerator generator)
        {
            var points = generator.Points;
            if (points.Count < 2)
            {
                return;
            }

            var generatorTransform = generator.transform;
            var segmentCount = points.Count == 2 ? 1 : points.Count;
            using (new Handles.DrawingScope(InsertColor))
            {
                for (var i = 0; i < segmentCount; i++)
                {
                    var next = (i + 1) % points.Count;
                    var midpoint = (points[i] + points[next]) * 0.5f;
                    var worldMidpoint = generatorTransform.TransformPoint(midpoint);
                    var size = HandleUtility.GetHandleSize(worldMidpoint) * InsertHandleSize;
                    if (Handles.Button(worldMidpoint, Quaternion.identity, size, size * 1.5f, Handles.DotHandleCap))
                    {
                        Undo.RecordObject(generator, "Insert Point");
                        generator.InsertPoint(i + 1, midpoint);
                        EditorUtility.SetDirty(generator);
                        _selectedIndex = i + 1;
                        Repaint();
                    }
                }
            }
        }

        #endregion Editing

        #region Drawing

        private void DrawPath(WorldEdgeCollidersGenerator generator)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var points = generator.Points;
            var generatorTransform = generator.transform;

            for (var i = 0; i < points.Count; i++)
            {
                var worldPoint = generatorTransform.TransformPoint(points[i]);

                if (_isPlacingPoints)
                {
                    var size = HandleUtility.GetHandleSize(worldPoint) * PointHandleSize;
                    using (new Handles.DrawingScope(PointColor))
                    {
                        Handles.SphereHandleCap(0, worldPoint, Quaternion.identity, size * 2f, EventType.Repaint);
                    }
                }

                Handles.Label(worldPoint + Vector3.up * HandleUtility.GetHandleSize(worldPoint) * 0.3f, i.ToString(), EditorStyles.whiteBoldLabel);

                if (points.Count < 2)
                {
                    continue;
                }

                var nextWorldPoint = generatorTransform.TransformPoint(points[(i + 1) % points.Count]);
                var isClosingSegment = i == points.Count - 1;
                if (!isClosingSegment)
                {
                    using (new Handles.DrawingScope(LineColor))
                    {
                        Handles.DrawAAPolyLine(4f, worldPoint, nextWorldPoint);
                    }
                }
                // While placing, the preview draws the loop through the cursor instead.
                else if (!_isPlacingPoints || !_hasPreview)
                {
                    using (new Handles.DrawingScope(ClosingLineColor))
                    {
                        Handles.DrawDottedLine(worldPoint, nextWorldPoint, 4f);
                    }
                }
            }
        }

        private void DrawSceneOverlay(WorldEdgeCollidersGenerator generator)
        {
            Handles.BeginGUI();

            var sceneView = SceneView.currentDrawingSceneView;
            var viewport = sceneView != null ? sceneView.cameraViewport : new Rect(0f, 0f, Screen.width, Screen.height);
            var viewWidth = viewport.width;
            var viewHeight = viewport.height;

            const float width = 470f;
            var height = _isPlacingPoints ? 74f : 34f;
            _overlayRect = new Rect((viewWidth - width) * 0.5f, viewHeight - height - 12f, width, height);

            // Claim the mouse over the panel so clicks on it don't select or add anything in the scene behind it.
            var blockerId = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout && _overlayRect.Contains(Event.current.mousePosition))
            {
                HandleUtility.AddControl(blockerId, 0f);
            }

            GUILayout.BeginArea(_overlayRect, EditorStyles.helpBox);
            if (_isPlacingPoints)
            {
                GUILayout.Label($"Placing points ({generator.Points.Count})", EditorStyles.boldLabel);
                GUILayout.Label("Click: add point  •  Backspace: remove last  •  Esc / Enter / Right-click: finish", EditorStyles.miniLabel);
                var previousColor = GUI.backgroundColor;
                GUI.backgroundColor = SelectedPointColor;
                if (GUILayout.Button("Done"))
                {
                    SetPlacing(false);
                }
                GUI.backgroundColor = previousColor;
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"World Edge: {generator.Points.Count} points", EditorStyles.boldLabel);
                if (GUILayout.Button(generator.Points.Count == 0 ? "Place Points" : "Add Points", GUILayout.Width(110)))
                {
                    SetPlacing(true);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        #endregion Drawing

        /// <summary>
        /// Finds the world position under the mouse: snaps to scene geometry when there is any (unless Shift is held),
        /// otherwise falls back to a horizontal plane at <paramref name="fallbackHeight"/>.
        /// </summary>
        private static bool TryGetPlacementPoint(Vector2 mousePosition, float fallbackHeight, out Vector3 worldPoint)
        {
            if (!Event.current.shift && HandleUtility.PlaceObject(mousePosition, out worldPoint, out _))
            {
                return true;
            }

            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, fallbackHeight, 0f));
            if (plane.Raycast(ray, out var distance))
            {
                worldPoint = ray.GetPoint(distance);
                return true;
            }

            worldPoint = default;
            return false;
        }

        private static float LastPointHeight(WorldEdgeCollidersGenerator generator)
        {
            var points = generator.Points;
            return points.Count > 0 ? generator.transform.TransformPoint(points[^1]).y : generator.transform.position.y;
        }
    }
}
#endif
