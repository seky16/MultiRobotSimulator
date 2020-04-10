using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Enums;
using QuickGraph;

namespace MultiRobotSimulator.Core.Models
{
    public class Map : IGraph
    {
        public const string HeightStr = "height ";

        public const string MapStr = "map";

        public const string TypeStr = "type octile";

        public const string WidthStr = "width ";

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _wrappedGraph = new UndirectedGraph<Tile, SEdge<Tile>>(false);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tile = new Tile() { X = x, Y = y };
                    _wrappedGraph.AddVertex(tile);
                }
            }

            foreach (var tile in _wrappedGraph.Vertices)
            {
                RecalculateNeighbours(tile);
            }
        }

        public int Height { get; }

        public int Width { get; }

        public bool AddToTile(Tile tile, DrawingMode drawingMode)
        {
            if (!tile.IsEmpty)
            {
                return false;
            }

            switch (drawingMode)
            {
                case DrawingMode.Obstacle:
                    tile.Passable = false;
                    _wrappedGraph.ClearAdjacentEdges(tile);
                    break;

                case DrawingMode.Start:
                    tile.IsStart = true;
                    break;

                case DrawingMode.Target:
                    tile.IsTarget = true;
                    break;

                default:
                    break;
            }

            return true;
        }

        public bool ClearAll()
        {
            var result = false;

            foreach (var tile in _wrappedGraph.Vertices)
            {
                result |= tile.SetToDefault();
            }

            foreach (var tile in _wrappedGraph.Vertices)
            {
                RecalculateNeighbours(tile);
            }

            return result;
        }

        public IEnumerable<Tile> GetCornerNeighbours(Tile tile) => _wrappedGraph.AdjacentVertices(tile).Where(t => !(t.X == tile.X || t.Y == tile.Y));

        public IEnumerable<Tile> GetPerpendicularNeighbours(Tile tile) => _wrappedGraph.AdjacentVertices(tile).Where(t => t.X == tile.X || t.Y == tile.Y);

        public string GetString()
        {
            var sb = new StringBuilder();

            // header
            sb.AppendLine(TypeStr);
            sb.Append(HeightStr).AppendLine(Height.ToString(CultureInfo.InvariantCulture));
            sb.Append(WidthStr).AppendLine(Width.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine(MapStr);

            // map
            AbstractTile? t;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    t = GetTileAtPos(x, y);

                    if (!(t is Tile tile))
                    {
                        throw new InvalidOperationException($"Couldn't find tile at [{x};{y}]");
                    }

                    sb.Append(tile.Passable ? "." : "@");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public AbstractTile? GetTileAtPos(int x, int y) => _wrappedGraph.Vertices.SingleOrDefault(t => t.X == x && t.Y == y);

        public bool RemoveFromTile(Tile tile)
        {
            var result = tile.SetToDefault();

            RecalculateNeighbours(tile);

            return result;
        }

        private void RecalculateNeighbours(Tile tile)
        {
            var foo = _wrappedGraph.AdjacentVertices(tile);

            _wrappedGraph.ClearAdjacentEdges(tile);

            var corners = new List<Tile>(4);

            // TODO dont allow to cut corners
            for (var yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (var xOffset = -1; xOffset <= 1; xOffset++)
                {
                    if (GetTileAtPos(tile.X + xOffset, tile.Y + yOffset) is Tile neighbour && neighbour != tile && neighbour.Passable)
                    {
                        if (xOffset != 0 && yOffset != 0)
                        {
                            corners.Add(neighbour);
                        }

                        _wrappedGraph.AddEdge(new SEdge<Tile>(tile, neighbour));
                    }
                }
            }
        }

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

        public (AbstractTile, AbstractTile) AdjacentEdge(AbstractTile v, int index)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            var edge = _wrappedGraph.AdjacentEdge(tile, index);
            return (edge.Source, edge.Target);
        }

        public IEnumerable<(AbstractTile, AbstractTile)> AdjacentEdges(AbstractTile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            var edges = _wrappedGraph.AdjacentEdges(tile);
            return edges.Select(e => ((AbstractTile)e.Source, (AbstractTile)e.Target));
        }

        public IEnumerable<AbstractTile> AdjacentVertices(AbstractTile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.AdjacentVertices(tile);
        }

        public bool ContainsEdge(AbstractTile source, AbstractTile target)
        {
            if (!(source is Tile tSource))
                throw GetException(source);
            if (!(target is Tile tTarget))
                throw GetException(target);

            return _wrappedGraph.ContainsEdge(tSource, tTarget);
        }

        public bool ContainsEdge((AbstractTile, AbstractTile) edge)
        {
            if (!(edge.Item1 is Tile source))
                throw GetException(edge.Item1);

            if (!(edge.Item2 is Tile target))
                throw GetException(edge.Item2);

            var tEdge = new SEdge<Tile>(source, target);
            return _wrappedGraph.ContainsEdge(tEdge);
        }

        public bool ContainsVertex(AbstractTile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.ContainsVertex(tile);
        }

        public object GetWrappedGraph()
        {
            return _wrappedGraph.Clone();
        }

        public bool IsAdjacentEdgesEmpty(AbstractTile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.IsAdjacentEdgesEmpty(tile);
        }

        public bool TryGetEdge(AbstractTile source, AbstractTile target, out (AbstractTile, AbstractTile) edge)
        {
            if (!(source is Tile sourceTile))
                throw GetException(source);

            if (!(target is Tile targetTile))
                throw GetException(target);

            edge = default((AbstractTile, AbstractTile));
            var result = _wrappedGraph.TryGetEdge(sourceTile, targetTile, out var sEdge);
            if (result)
            {
                edge = (sEdge.Source, sEdge.Target);
            }

            return result;
        }

        private Exception GetException(AbstractTile v) => new KeyNotFoundException($"Could not find tile '{v}'");

        #endregion Wrapped graph
    }
}
