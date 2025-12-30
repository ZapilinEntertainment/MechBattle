using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using TriInspector;
using UnityEditor;
#endif

namespace ZE.MechBattle.Navigation.DebugDraw
{
    [ExecuteInEditMode]
    public class ObjectPositionDrawer : MonoBehaviour
    {

        [SerializeField] private NavigationMapDrawer _mainDrawer;
        [SerializeField] private Transform _trackingObject;
        private IntTriangularPos _selectedTrianglePos;
        private int2 _selectedHex; 
        private Vector3[] _trianglePositions = new Vector3[3];
        private Vector3[] _hexPositions = new Vector3[6];
        private HexPointsPreset _hexPointsPreset;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_trackingObject == null || _mainDrawer?.Map == null)
                return;

            var triangleEdgeSize = _mainDrawer.Map.TriangleEdgeSize;
            var worldPos = _trackingObject.position;

            var currentTriangle = TriangularMath.WorldToTrianglePos(worldPos, triangleEdgeSize);
            if (currentTriangle != _selectedTrianglePos) 
                UpdateTriangleData(currentTriangle);

            var currentHex = TriangularMath.WorldToHex(new(worldPos.x, worldPos.z), _mainDrawer.Map.HexEdgeSize);
            if (currentHex.x != _selectedHex.x || currentHex.y != _selectedHex.y)
                UpdateHexData(currentHex);

            var pos = TriangularMath.TriangularToWorld(_selectedTrianglePos, triangleEdgeSize);
            Gizmos.color = Color.hotPink;

            var radius = _mainDrawer.Map.TrianglesPerEdge;
            var x = currentTriangle.DownLeft / (2f * radius) ;
            var y = currentTriangle.Up / (2f * radius);
            var z = currentTriangle.DownRight / (2f * radius);


            Handles.Label(pos, $"{_selectedHex} : {_selectedTrianglePos}");
            //Gizmos.DrawSphere(, 0.1f);

            Gizmos.DrawLine(_trianglePositions[0], _trianglePositions[1]);
            Gizmos.DrawLine(_trianglePositions[1], _trianglePositions[2]);
            Gizmos.DrawLine(_trianglePositions[0], _trianglePositions[2]);

            Gizmos.DrawLine(_hexPositions[0], _hexPositions[1]);
            Gizmos.DrawLine(_hexPositions[1], _hexPositions[2]);
            Gizmos.DrawLine(_hexPositions[2], _hexPositions[3]);
            Gizmos.DrawLine(_hexPositions[3], _hexPositions[4]);
            Gizmos.DrawLine(_hexPositions[4], _hexPositions[5]);
            Gizmos.DrawLine(_hexPositions[0], _hexPositions[5]);
        }

        [Button("Print Hex")]
        public void PlaneToHex()
        {
            Debug.Log(_selectedHex);
        }
#endif

        private void UpdateTriangleData(in IntTriangularPos pos)
        {
            var vertices = NavigationMapHelper.GetTriangleVertices(pos, _mainDrawer.Map.TriangleEdgeSize);
            _trianglePositions[0] = vertices.A;
            _trianglePositions[1] = vertices.B;
            _trianglePositions[2] = vertices.C;
            _selectedTrianglePos = pos;
        }

        private void UpdateHexData(in int2 hexPos)
        {
            var hexEdge = _mainDrawer.Map.HexEdgeSize;
            _hexPointsPreset = new(hexEdge);
            var center = TriangularMath.HexToWorld(hexPos, hexEdge);

            float3 ToVector3(float2 pos) => new (pos.x, 0f, pos.y);

            _hexPositions[0] = ToVector3(center + _hexPointsPreset.TopRight);
            _hexPositions[1] =  ToVector3(center + _hexPointsPreset.Right);
            _hexPositions[2] =  ToVector3(center + _hexPointsPreset.BottomRight);
            _hexPositions[3] =  ToVector3(center + _hexPointsPreset.BottomLeft);
            _hexPositions[4] =  ToVector3(center + _hexPointsPreset.Left);
            _hexPositions[5] =  ToVector3(center + _hexPointsPreset.TopLeft);

            _selectedHex = hexPos;
        }

        public static (int hx, int hy, int hz) GetHexFromTriangle(int tx, int ty, int tz, int N)
        {
            // Проверяем, что треугольник находится в плоскости x+y+z=0
            if (tx + ty + tz != 0)
            {
                // Если сумма не ноль, корректируем координаты
                // Приводим к ближайшей точке на плоскости
                int diff = (tx + ty + tz) / 3;
                tx -= diff;
                ty -= diff;
                tz -= diff;
            }

            // Размер гекса в единицах треугольников
            int hexSize = 2 * N;  // Диаметр гекса

            // 1. Находим "грубые" координаты гекса делением на размер
            // Центры гексов расположены в точках, кратных N (или 2N)
            // Для простоты предположим, что центры гексов имеют координаты,
            // кратные N во всех измерениях

            // Округляем координаты треугольника до ближайшего кратного N
            int hx = (int)Mathf.Round((float)tx / N) * N;
            int hy = (int)Mathf.Round((float)ty / N) * N;
            int hz = (int)Mathf.Round((float)tz / N) * N;

            // 2. Корректируем, чтобы сумма координат была 0 (центр гекса в плоскости)
            int sum = hx + hy + hz;
            if (sum != 0)
            {
                // Распределяем разницу поровну
                int correction = sum / 3;
                hx -= correction;
                hy -= correction;
                hz -= correction;
            }

            // 3. Проверяем, что треугольник действительно находится в этом гексе
            // Расстояние в кубических координатах
            int dx = Mathf.Abs(tx - hx);
            int dy = Mathf.Abs(ty - hy);
            int dz = Mathf.Abs(tz - hz);

            // В кубических координатах расстояние = max(dx, dy, dz)
            int distance = Mathf.Max(Mathf.Max(dx, dy), dz);

            // Если расстояние <= N, треугольник в этом гексе
            if (distance <= N)
            {
                return (hx, hy, hz);
            }

            (int hx, int hy, int hz)[] candidates = new[]
            {
            (hx, hy, hz),
            (hx + N, hy, hz - N),
            (hx - N, hy, hz + N),
            (hx, hy + N, hz - N),
            (hx, hy - N, hz + N),
            (hx + N, hy - N, hz),
            (hx - N, hy + N, hz)
        };

            foreach (var candidate in candidates)
            {
                dx = Mathf.Abs(tx - candidate.hx);
                dy = Mathf.Abs(ty - candidate.hy);
                dz = Mathf.Abs(tz - candidate.hz);
                distance = Mathf.Max(Mathf.Max(dx, dy), dz);

                if (distance <= N && candidate.hx + candidate.hy + candidate.hz == 0)
                {
                    return candidate;
                }
            }

            // 5. Если всё ещё не нашли, используем грубый метод
            // Иногда нужно проверить больше кандидатов
            for (int dx2 = -2; dx2 <= 2; dx2++)
            {
                for (int dy2 = -2; dy2 <= 2; dy2++)
                {
                    int dz2 = -dx2 - dy2;
                    int candHx = hx + dx2 * N;
                    int candHy = hy + dy2 * N;
                    int candHz = hz + dz2 * N;

                    dx = Mathf.Abs(tx - candHx);
                    dy = Mathf.Abs(ty - candHy);
                    dz = Mathf.Abs(tz - candHz);
                    distance = Mathf.Max(Mathf.Max(dx, dy), dz);

                    if (distance <= N && candHx + candHy + candHz == 0)
                    {
                        return (candHx, candHy, candHz);
                    }
                }
            }

            // Если ничего не нашли (крайне маловероятно), возвращаем исходный
            return (hx, hy, hz);
        }       
    }
}
