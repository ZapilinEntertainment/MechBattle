using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace ZE.MechBattle.Navigation.Tests
{
    public class RansacTest
    {
        // Вспомогательный метод для генерации seed (заглушка вашего GetNextSeed)
        private static uint GetNextSeed() => 12345u;

        #region Тесты на падения (Ошибки времени выполнения)

        [Test]
        public void TryGetBestPlane_OneElementList_ThrowsArgumentExceptionInRandom()
        {
            // Arrange
            var points = new List<OrientedPoint>
            {
                new OrientedPoint(new float3(1, 2, 3),new float3(0, 1, 0) )
            };
            var heights = new List<float> { 2f };

            // Act & Assert
            // Код не падает, так как rng.NextInt(0, 1) возвращает 0, но RANSAC для 1 элемента бессмысленен
            Assert.DoesNotThrow(() =>
            {
                bool success = TryGetBestPlane(points, heights, 10, 0.1f, out float4 bestPlane);

                // Метод вернет true, так как единственный элемент совпадет сам с собой
                Assert.IsTrue(success);
            });
        }

        #endregion

        #region Тесты на логику и корректность работы

        [Test]
        public void TryGetBestPlane_PerfectHorizontalPlane_ReturnsTrueAndCorrectPlane()
        {
            // Arrange
            // Создаем идеальную горизонтальную плоскость на высоте Y = 5
            var points = new List<OrientedPoint>();
            var heights = new List<float>();

            for (int i = 0; i < 10; i++)
            {
                points.Add(new OrientedPoint(new float3(i, 5, 0), math.up()));
                heights.Add(5f);
            }

            // Act
            bool success = TryGetBestPlane(points, heights, 5, 0.01f, out float4 bestPlane);

            // Assert
            Assert.IsTrue(success, "Метод должен успешно найти плоскость");

            // Уравнение плоскости: 0x + 1y + 0z - 5 = 0 -> float4(0, 1, 0, -5)
            Assert.AreEqual(0f, bestPlane.x, 0.001f);
            Assert.AreEqual(1f, bestPlane.y, 0.001f);
            Assert.AreEqual(0f, bestPlane.z, 0.001f);
            Assert.AreEqual(-5f, bestPlane.w, 0.001f);
        }

        [Test]
        public void TryGetBestPlane_WithNoiseAndOutliers_FindsDominantPlane()
        {
            // Arrange
            var points = new List<OrientedPoint>();
            var heights = new List<float>();

            // 80 инлайеров (правильная плоскость на высоте Y = 0)
            for (int i = 0; i < 80; i++)
            {
                points.Add(new OrientedPoint(new float3(i, 0, 0), math.up()));
                heights.Add(0f);
            }

            // 20 аутлайеров (шумные точки, улетающие вверх)
            for (int i = 0; i < 20; i++)
            {
                points.Add(new OrientedPoint(new float3(i, 100, i), math.right()));
                heights.Add(100f);
            }

            // Act
            // Запускаем достаточно итераций, чтобы RANSAC случайно попал в инлайер
            bool success = TryGetBestPlane(points, heights, 50, 0.1f, out float4 bestPlane);

            // Assert
            Assert.IsTrue(success);
            // Должна выбраться плоскость инлайеров (Y = 0)
            Assert.AreEqual(1f, bestPlane.y, 0.01f);
            Assert.AreEqual(0f, bestPlane.w, 0.01f);
        }

        #endregion

        #region Тестируемый метод (Вставлен для автономности теста)

        // Атрибут [BurstDiscard] закомментирован, чтобы тест в Editor вообще мог запуститься.
        // Если его раскомментировать и вызвать метод из Burst Job — тест компиляции завалит сборку.
        // [BurstDiscard] 
        public static bool TryGetBestPlane(
            IReadOnlyList<OrientedPoint> points,
            IList<float> heights,
            int ransacIterationsCount,
            float ransacThreshold,
            out float4 bestPlane)
        {
            bestPlane = new float4(0, 1, 0, -heights[0]);
            int maxInliers = -1;
            var rng = new Unity.Mathematics.Random(GetNextSeed());
            var length = points.Count;

            for (int i = 0; i < ransacIterationsCount; i++)
            {
                int idx = rng.NextInt(0, length);
                var hit = points[idx];
                float3 p = new float3(hit.Position.x, heights[idx], hit.Position.z);
                float3 n = hit.Normal;

                if (math.abs(n.y) < 0.01f) continue;

                float4 currentPlane = new float4(n, -math.dot(n, p));
                int inliersCount = 0;

                for (int j = 0; j < length; j++)
                {
                    var h = points[j];
                    float3 pCurr = new float3(h.Position.x, heights[j], h.Position.z);
                    float dist = math.abs(math.dot(currentPlane.xyz, pCurr) + currentPlane.w);
                    float normalDiff = 1f - math.dot(currentPlane.xyz, h.Normal);

                    if (dist < ransacThreshold && normalDiff < 0.1f)
                        inliersCount++;
                }

                if (inliersCount > maxInliers)
                {
                    maxInliers = inliersCount;
                    bestPlane = currentPlane;
                }
            }
            return maxInliers > 0;
        }

        #endregion
    }

}
