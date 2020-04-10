using System;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractTile : IComparable, IComparable<AbstractTile>, IEquatable<AbstractTile>
    {
        public bool IsStart { get; set; }
        public bool IsTarget { get; set; }
        public bool Passable { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsCornerNeighbour(AbstractTile tile) => Math.Abs(X - tile.X) == 1 && Math.Abs(Y - tile.Y) == 1;

        public bool IsPerpendicularNeighbour(AbstractTile tile) => (Math.Abs(X - tile.X) == 1 && Math.Abs(Y - tile.Y) == 0) || (Math.Abs(X - tile.X) == 0 && Math.Abs(Y - tile.Y) == 1);

        #region IComparable, IEquatable overrides

        public static bool operator !=(AbstractTile? left, AbstractTile? right)
        {
            return !(left == right);
        }

        public static bool operator <(AbstractTile? left, AbstractTile? right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(AbstractTile? left, AbstractTile? right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator ==(AbstractTile? left, AbstractTile? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator >(AbstractTile? left, AbstractTile? right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(AbstractTile? left, AbstractTile? right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        public int CompareTo(object? obj)
        {
            if (!(obj is AbstractTile tile))
            {
                throw new ArgumentException($"{obj} is not a type of {nameof(AbstractTile)}");
            }

            return CompareTo(tile);
        }

        public int CompareTo(AbstractTile? other)
        {
            if (other is null)
            {
                return 1;
            }

            if (Y < other.Y)
            {
                return -1;
            }
            else if (Y == other.Y)
            {
                if (X < other.X)
                {
                    return -1;
                }
                else if (X == other.X)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }

        public bool Equals(AbstractTile? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is AbstractTile tile)
            {
                return Equals(tile);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        #endregion IComparable, IEquatable overrides
    }
}
