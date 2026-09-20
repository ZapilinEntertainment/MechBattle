using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Editor.Tests
{
   // Qwen generated

    public class TriangularIndicesMathTests
    {
        private NativeArray<byte> EmulateFulfilRowIndices(int rowsCount)
        {
            return TrianglesToIndexFlattenedConverter.FulfilRowIndices(Unity.Collections.Allocator.Temp, rowsCount);
        }

        // resulting formula not used atm
        [Test]
        public void IndexToV2_MathFormula_Matches_PrecalculatedArray()
        {
            // note of original byte limitations
            for (int N = 1; N < 256; N++)
            {
                var precalculatedTable = EmulateFulfilRowIndices(N);
                int maxIndex = N * (N + 1) / 2;

                for (int i = 0; i < maxIndex; i++)
                {
                    // 1. Ожидаемые значения из "старого" массива
                    int expectedY = precalculatedTable[i];
                    int expectedX = i - expectedY * (expectedY + 1) / 2;

                    // 2. Фактические значения из "новой" математической формулы
                    int actualY = (int)math.floor((math.sqrt(8f * i + 1f) - 1f) * 0.5f);
                    int actualX = i - actualY * (actualY + 1) / 2;

                    // 3. Сверка
                    Assert.AreEqual(expectedY, actualY,
                        $"Y mismatch at index {i} for N={N}. Expected: {expectedY}, Actual: {actualY}");
                    Assert.AreEqual(expectedX, actualX,
                        $"X mismatch at index {i} for N={N}. Expected: {expectedX}, Actual: {actualX}");
                }
            }
        }

        [Test]
        public void IndexToV2_BoundaryConditions()
        {
            // Проверка границ строк (переход от одной строки к другой)
            // Row 0: index 0
            // Row 1: indices 1, 2
            // Row 2: indices 3, 4, 5
            // Row 3: indices 6, 7, 8, 9

            Assert.AreEqual(new int2(0, 0), GetV2(0));
            Assert.AreEqual(new int2(0, 1), GetV2(1));
            Assert.AreEqual(new int2(1, 1), GetV2(2));
            Assert.AreEqual(new int2(0, 2), GetV2(3));
            Assert.AreEqual(new int2(1, 2), GetV2(4));
            Assert.AreEqual(new int2(2, 2), GetV2(5));
            Assert.AreEqual(new int2(0, 3), GetV2(6));
        }

        // Вспомогательный метод, повторяющий логику из рефакторенного конвертера
        private int2 GetV2(int index)
        {
            int y = (int)math.floor((math.sqrt(8f * index + 1f) - 1f) * 0.5f);
            int x = index - y * (y + 1) / 2;
            return new int2(x, y);
        }
    }
}
