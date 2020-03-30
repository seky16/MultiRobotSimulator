using MultiRobotSimulator.Core.Enums;

namespace MultiRobotSimulator.Core.Models
{
    public class Tile
    {
        public const bool IsFinishDefaultValue = false;
        public const bool IsStartDefaultValue = false;
        public const bool PassableDefaultValue = true;

        public bool IsFinish { get; set; } = IsFinishDefaultValue;
        public bool IsStart { get; set; } = IsStartDefaultValue;
        public bool Passable { get; set; } = PassableDefaultValue;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;

        private bool IsEmpty => IsFinish == IsFinishDefaultValue && IsStart == IsStartDefaultValue && Passable == PassableDefaultValue;

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

                case DrawingMode.Finish:
                    IsFinish = true;
                    break;

                default:
                    break;
            }

            return true;
        }

        public bool Empty()
        {
            if (IsEmpty)
            {
                return false;
            }

            IsFinish = IsFinishDefaultValue;
            IsStart = IsStartDefaultValue;
            Passable = PassableDefaultValue;
            return true;
        }
    }
}
