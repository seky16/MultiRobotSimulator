using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.Core.Models
{
    public class Tile : AbstractTile
    {
        private const bool IsStartDefaultValue = false;
        private const bool IsTargetDefaultValue = false;
        private const bool PassableDefaultValue = true;

        public Tile()
        {
            X = Y = -1;
            SetToDefault();
        }

        public bool IsEmpty => IsTarget == IsTargetDefaultValue && IsStart == IsStartDefaultValue && Passable == PassableDefaultValue;

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
