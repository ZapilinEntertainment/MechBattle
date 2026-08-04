using R3;
using System;
using System.Collections.Generic;
using TriInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ZE.MechBattle.Develop
{
    public class PointGraph : MonoBehaviour
    {

        [Header("Settings")]
        public Color lineColor = Color.green;
        public Color selectedColor = Color.yellow;
        public float pointSize = 0.2f;

        [Header("Data")]
        [SerializeField] private List<Vector3> _points = new List<Vector3>();
        [SerializeField] private List<string> _notes = new();
        [SerializeField] private int selectedIndex = -1;

        public int SelectedIndex => selectedIndex;
        private IDisposable _subscription;

        public void AddPoint(Vector3 position, string note = "")
        {
            _points.Add(position);
            _notes.Add(note);
        }

        [Button]
        public void Next() => Shift(1);
        [Button]
        public void Next10() => Shift(10);

        [Button]
        public void Previous() => Shift(-1);
        [Button]
        public void Previous10() => Shift(-10);

        private void Shift(int delta) => selectedIndex = math.clamp(selectedIndex + delta, 0, _points.Count - 1);

        private void Start()
        {
            _subscription = MessageBroker.Receive<DrawPointMessage>().Subscribe(msg => AddPoint(msg.Pos, msg.Text));
        }

        private void OnDestroy()
        {
            _subscription.Dispose();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enabled || _points.Count == 0)
                return;

            Gizmos.color = lineColor;
            Gizmos.DrawSphere(_points[0], pointSize);
            if (_points.Count == 1)
                return;

            for (var i = 1; i < _points.Count; i++)
            {
                var pos = _points[i];
                Gizmos.DrawSphere(pos, pointSize);
                Gizmos.DrawLine(_points[i - 1], pos);
            }

            if (selectedIndex != -1)
            {
                Gizmos.color = selectedColor;
                var selectedPos = _points[selectedIndex];
                Gizmos.DrawSphere(selectedPos, pointSize);
                Handles.Label(selectedPos + new Vector3(0,-1f,0), _notes[selectedIndex]);
            }
        }
#endif
    }

}
