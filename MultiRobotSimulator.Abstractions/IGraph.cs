using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public interface IGraph
    {
        IEnumerable<(ITile, ITile)> Edges { get; }
        IEnumerable<ITile> Vertices { get; }

        int AdjacentDegree(ITile v);

        (ITile, ITile) AdjacentEdge(ITile v, int index);

        IEnumerable<(ITile, ITile)> AdjacentEdges(ITile v);

        IEnumerable<ITile> AdjacentVertices(ITile v);

        bool ContainsEdge(ITile source, ITile target);

        bool ContainsEdge((ITile, ITile) edge);

        bool ContainsVertex(ITile vertex);

        ITile? GetTileAtPos(int x, int y);

        object GetWrappedGraph();

        bool IsAdjacentEdgesEmpty(ITile v);

        bool TryGetEdge(ITile source, ITile target, out (ITile, ITile) edge);
    }
}
