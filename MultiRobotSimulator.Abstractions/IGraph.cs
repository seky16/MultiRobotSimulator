using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public interface IGraph
    {
        IEnumerable<(AbstractTile, AbstractTile)> Edges { get; }
        IEnumerable<AbstractTile> Vertices { get; }

        int AdjacentDegree(AbstractTile v);

        (AbstractTile, AbstractTile) AdjacentEdge(AbstractTile v, int index);

        IEnumerable<(AbstractTile, AbstractTile)> AdjacentEdges(AbstractTile v);

        IEnumerable<AbstractTile> AdjacentVertices(AbstractTile v);

        IGraph Clone();

        bool ContainsEdge(AbstractTile source, AbstractTile target);

        bool ContainsEdge((AbstractTile, AbstractTile) edge);

        bool ContainsVertex(AbstractTile vertex);

        AbstractTile? GetTileAtPos(int x, int y);

        object GetWrappedGraph();

        bool IsAdjacentEdgesEmpty(AbstractTile v);

        bool RemoveEdge((AbstractTile, AbstractTile) edge);

        int RemoveEdges(IEnumerable<(AbstractTile, AbstractTile)> edges);

        bool RemoveVertex(AbstractTile v);

        bool TryGetEdge(AbstractTile source, AbstractTile target, out (AbstractTile, AbstractTile) edge);
    }
}
