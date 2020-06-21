#nullable disable

using System;
using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public struct SpaceTimeNode : IEquatable<SpaceTimeNode>
    {
        public SpaceTimeNode(int x, int y, int z)
        {
            X = x;
            Y = y;
            T = z;
        }

        public int T { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public static bool operator !=(SpaceTimeNode left, SpaceTimeNode right)
        {
            return !(left == right);
        }

        public static bool operator ==(SpaceTimeNode left, SpaceTimeNode right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is AbstractTile tile)
            {
                return (tile.X, tile.Y).Equals((X, Y));
            }

            return obj is SpaceTimeNode other && Equals(other);
        }

        public bool Equals(SpaceTimeNode other)
        {
            return (X, Y, T).Equals((other.X, other.Y, other.T));
        }

        public override int GetHashCode()
        {
            return (X, Y, T).GetHashCode();
        }

        public SpaceTimeNode Next()
        {
            return new SpaceTimeNode(X, Y, T + 1);
        }

        public override string ToString()
        {
            return $"({X},{Y},{T})";
        }
    }

    public class SpaceTimeGraph
    {
        public SpaceTimeGraph(IGraph graph)
        {
            Graph = graph;
        }

        public IGraph Graph { get; }

        public IEnumerable<SpaceTimeNode> GetNeighbours(SpaceTimeNode node)
        {
            foreach (var n in Graph.AdjacentVertices(Graph.GetTileAtPos(node.X, node.Y)))
            {
                yield return new SpaceTimeNode(n.X, n.Y, node.T + 1);
            }
            yield return node.Next();
        }

        public AbstractTile GetTile(SpaceTimeNode node) => Graph.GetTileAtPos(node.X, node.Y);
    }
}
