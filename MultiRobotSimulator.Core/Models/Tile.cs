using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Enums;

namespace MultiRobotSimulator.Core.Models
{
    public class Tile : ITile
    {
        private const bool IsStartDefaultValue = false;
        private const bool IsTargetDefaultValue = false;
        private const bool PassableDefaultValue = true;

        public bool IsStart { get; set; } = IsStartDefaultValue;
        public bool IsTarget { get; set; } = IsTargetDefaultValue;
        public bool Passable { get; set; } = PassableDefaultValue;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;

        private bool IsEmpty => IsTarget == IsTargetDefaultValue && IsStart == IsStartDefaultValue && Passable == PassableDefaultValue;

        public bool AddToTile(DrawingMode drawingMode)
        {
            if (!IsEmpty)
            {
                return false;
            }

            switch (drawingMode)
            {
                case DrawingMode.Obstacle:
                    Passable = false;
                    break;

                case DrawingMode.Start:
                    IsStart = true;
                    break;

                case DrawingMode.Target:
                    IsTarget = true;
                    break;

                default:
                    break;
            }

            return true;
        }

        public bool SetToDefault()
        {
            if (IsEmpty)
            {
                return false;
            }

            IsTarget = IsTargetDefaultValue;
            IsStart = IsStartDefaultValue;
            Passable = PassableDefaultValue;
            return true;
        }
    }
}
