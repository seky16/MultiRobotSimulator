using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using QuickGraph;

namespace MultiRobotSimulator.Core.Models
{
    public class Graph : IGraph
    {
        private readonly int _height;
        private readonly int _width;

        public Graph(int width, int height)
        {
            _width = width;
            _height = height;
            _wrappedGraph = new UndirectedGraph<Tile, SEdge<Tile>>(false);
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var tile = new Tile() { X = x, Y = y };
                    _wrappedGraph.AddVertex(tile);
                }
            }

            foreach (var tile in _wrappedGraph.Vertices)
            {
                for (var yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (var xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        if (GetTileAtPos(tile.X + xOffset, tile.Y + yOffset) is Tile neighbour && neighbour != tile)
                        {
                            _wrappedGraph.AddEdge(new SEdge<Tile>(tile, neighbour));
                        }
                    }
                }
            }
        }

        public ITile? GetTileAtPos(int x, int y) => _wrappedGraph.Vertices.SingleOrDefault(t => t.X == x && t.Y == y);

        #region Wrapped graph

        private readonly UndirectedGraph<Tile, SEdge<Tile>> _wrappedGraph;
        public IEnumerable<(ITile, ITile)> Edges => _wrappedGraph.Edges.Select(e => ((ITile)e.Source, (ITile)e.Target));
        public IEnumerable<ITile> Vertices => _wrappedGraph.Vertices;

        public int AdjacentDegree(ITile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.AdjacentDegree(tile);
        }

        public (ITile, ITile) AdjacentEdge(ITile v, int index)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            var edge = _wrappedGraph.AdjacentEdge(tile, index);
            return (edge.Source, edge.Target);
        }

        public IEnumerable<(ITile, ITile)> AdjacentEdges(ITile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            var edges = _wrappedGraph.AdjacentEdges(tile);
            return edges.Select(e => ((ITile)e.Source, (ITile)e.Target));
        }

        public IEnumerable<ITile> AdjacentVertices(ITile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.AdjacentVertices(tile);
        }

        public bool ContainsEdge(ITile source, ITile target)
        {
            if (!(source is Tile tSource))
                throw GetException(source);
            if (!(target is Tile tTarget))
                throw GetException(target);

            return _wrappedGraph.ContainsEdge(tSource, tTarget);
        }

        public bool ContainsEdge((ITile, ITile) edge)
        {
            if (!(edge.Item1 is Tile source))
                throw GetException(edge.Item1);

            if (!(edge.Item2 is Tile target))
                throw GetException(edge.Item2);

            var tEdge = new SEdge<Tile>(source, target);
            return _wrappedGraph.ContainsEdge(tEdge);
        }

        public bool ContainsVertex(ITile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.ContainsVertex(tile);
        }

        public object GetWrappedGraph()
        {
            return _wrappedGraph.Clone();
        }

        public bool IsAdjacentEdgesEmpty(ITile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.IsAdjacentEdgesEmpty(tile);
        }

        public bool TryGetEdge(ITile source, ITile target, out (ITile, ITile) edge)
        {
            if (!(source is Tile sourceTile))
                throw GetException(source);

            if (!(target is Tile targetTile))
                throw GetException(target);

            edge = default((ITile, ITile));
            var result = _wrappedGraph.TryGetEdge(sourceTile, targetTile, out var sEdge);
            if (result)
            {
                edge = (sEdge.Source, sEdge.Target);
            }

            return result;
        }

        private Exception GetException(ITile v) => new KeyNotFoundException($"Could not find tile '{v}'");

        #endregion Wrapped graph
    }
}
