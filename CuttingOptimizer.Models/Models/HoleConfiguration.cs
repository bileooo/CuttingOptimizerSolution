using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingOptimizer.Models
{
    public class HoleConfiguration
    {
        public double HoleSpacing { get; set; }  // 孔洞间距
        public double LeftMargin { get; set; }   // 左边距
        public double RightMargin { get; set; }  // 右边距
        public double MaterialLength { get; set; } // 原料总长度

        public List<HolePosition> GetHolePositions()
        {
            var holes = new List<HolePosition>();

            if (HoleSpacing <= 0 || MaterialLength <= 0)
                return holes;

            double currentPosition = LeftMargin;
            int holeIndex = 1;

            while (currentPosition + HoleSpacing <= MaterialLength - RightMargin)
            {
                holes.Add(new HolePosition
                {
                    Index = holeIndex++,
                    StartPosition = currentPosition,
                    EndPosition = currentPosition + HoleSpacing
                });

                currentPosition += HoleSpacing;
            }

            return holes;
        }

        public bool WouldCreateIncompleteHole(double startPos, double partLength)
        {
            var holes = GetHolePositions();

            foreach (var hole in holes)
            {
                // 检查起始位置是否在孔洞中间
                if (startPos > hole.StartPosition && startPos < hole.EndPosition)
                {
                    return true;
                }
            }

            return false;
        }

        public double CalculateNextValidStart(double currentPosition)
        {
            var holes = GetHolePositions();

            foreach (var hole in holes)
            {
                if (currentPosition > hole.StartPosition && currentPosition < hole.EndPosition)
                {
                    return hole.EndPosition;
                }
            }

            return currentPosition;
        }
    }

    public class HolePosition
    {
        public int Index { get; set; }
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }
        public double Length => EndPosition - StartPosition;
    }
}