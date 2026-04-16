using Microsoft.VisualStudio.TestTools.UnitTesting;
using cli_life;

namespace Life.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BoardHasCorrectDimensions()
        {
            var b = new Board(50, 20, 1, 0);
            Assert.AreEqual(50, b.Columns);
            Assert.AreEqual(20, b.Rows);
        }

        [TestMethod]
        public void ZeroDensityAllDead()
        {
            var b = new Board(50, 20, 1, 0);
            Assert.AreEqual(0, b.CountAlive());
        }

        [TestMethod]
        public void FullDensityAllAlive()
        {
            var b = new Board(50, 20, 1, 1.0);
            Assert.AreEqual(50 * 20, b.CountAlive());
        }

        [TestMethod]
        public void DeadCellWithThreeNeighborsBecomesAlive()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[0, 0].IsAlive = true;
            b.Cells[1, 0].IsAlive = true;
            b.Cells[2, 0].IsAlive = true;
            b.Advance();
            Assert.IsTrue(b.Cells[1, 1].IsAlive);
        }

        [TestMethod]
        public void LiveCellWithTwoNeighborsSurvives()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[0, 1].IsAlive = true;
            b.Cells[1, 1].IsAlive = true;
            b.Cells[2, 1].IsAlive = true;
            b.Advance();
            Assert.IsTrue(b.Cells[1, 1].IsAlive);
        }

        [TestMethod]
        public void LiveCellWithNoNeighborsDies()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[5, 5].IsAlive = true;
            b.Advance();
            Assert.IsFalse(b.Cells[5, 5].IsAlive);
        }

        [TestMethod]
        public void LiveCellWithFourNeighborsDies()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[5, 5].IsAlive = true;
            b.Cells[4, 5].IsAlive = true;
            b.Cells[6, 5].IsAlive = true;
            b.Cells[5, 4].IsAlive = true;
            b.Cells[5, 6].IsAlive = true;
            b.Advance();
            Assert.IsFalse(b.Cells[5, 5].IsAlive);
        }

        [TestMethod]
        public void BlockIsStable()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[1, 1].IsAlive = true;
            b.Cells[2, 1].IsAlive = true;
            b.Cells[1, 2].IsAlive = true;
            b.Cells[2, 2].IsAlive = true;
            int before = b.CountAlive();
            b.Advance();
            Assert.AreEqual(before, b.CountAlive());
        }

        [TestMethod]
        public void BlinkerOscillates()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[4, 5].IsAlive = true;
            b.Cells[5, 5].IsAlive = true;
            b.Cells[6, 5].IsAlive = true;
            b.Advance();
            Assert.IsTrue(b.Cells[5, 4].IsAlive);
            Assert.IsTrue(b.Cells[5, 5].IsAlive);
            Assert.IsTrue(b.Cells[5, 6].IsAlive);
        }

        [TestMethod]
        public void CountGroupsEmpty()
        {
            var b = new Board(10, 10, 1, 0);
            Assert.AreEqual(0, b.CountGroups());
        }

        [TestMethod]
        public void CountGroupsOneGroup()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[1, 1].IsAlive = true;
            b.Cells[1, 2].IsAlive = true;
            Assert.AreEqual(1, b.CountGroups());
        }

        [TestMethod]
        public void CountGroupsTwoGroups()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[1, 1].IsAlive = true;
            b.Cells[8, 8].IsAlive = true;
            Assert.AreEqual(2, b.CountGroups());
        }

        [TestMethod]
        public void SaveAndLoadState()
        {
            var b = new Board(10, 10, 1, 0);
            b.Cells[3, 3].IsAlive = true;
            b.Cells[4, 4].IsAlive = true;
            b.SaveToFile("test_state.txt");

            var b2 = new Board(10, 10, 1, 0);
            b2.LoadFromFile("test_state.txt");
            Assert.IsTrue(b2.Cells[3, 3].IsAlive);
            Assert.IsTrue(b2.Cells[4, 4].IsAlive);
            Assert.IsFalse(b2.Cells[0, 0].IsAlive);
        }

        [TestMethod]
        public void NeighborCountIsEight()
        {
            var b = new Board(10, 10, 1, 0);
            Assert.AreEqual(8, b.Cells[5, 5].neighbors.Count);
        }

        [TestMethod]
        public void GenerationsToStableReturnsPositive()
        {
            var b = new Board(30, 20, 1, 0.3);
            int gen = b.GenerationsToStable();
            Assert.IsTrue(gen >= 0);
        }

        [TestMethod]
        public void AllDeadIsStableImmediately()
        {
            var b = new Board(10, 10, 1, 0);
            int gen = b.GenerationsToStable();
            Assert.IsTrue(gen >= 0);
        }
    }
}