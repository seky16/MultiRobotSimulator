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
                RecalculateNeighbors(tile);
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
                    RecalculateNeighbors(tile);
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
                RecalculateNeighbors(tile);
            }

            return result;
        }

        public IEnumerable<Tile> GetNeighbors(Tile tile) => _wrappedGraph.Vertices.Where(t => t != tile && (Math.Abs(t.X - tile.X) <= 1 && Math.Abs(t.Y - tile.Y) <= 1));

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

            RecalculateNeighbors(tile);

            return result;
        }

        private void RecalculateNeighbors(Tile tile)
        {
            var n = GetNeighbors(tile).Append(tile);

            _wrappedGraph.ClearAdjacentEdges(tile);

            if (!tile.Passable)
            {
                // obstacle - don't allow to cut corners
                var p = n.Where(t => t.IsPerpendicularNeighbour(tile));
                foreach ((var t1, var t2) in p.Pairs())
                {
                    if (_wrappedGraph.TryGetEdge(t1, t2, out var edge))
                    {
                        _wrappedGraph.RemoveEdge(edge);
                    }
                }
            }
            else
            {
                // insert edges for all neighbors
                foreach ((var t1, var t2) in n.Pairs())
                {
                    if (t1.IsPerpendicularNeighbour(t2) && t1.Passable && t2.Passable)
                    {
                        // perpendicular - simple check
                        _wrappedGraph.AddEdge(new SEdge<Tile>(t1, t2));
                    }
                    else if (t1.IsCornerNeighbour(t2) && t1.Passable && t2.Passable)
                    {
                        // corner - don't allow to cut corners
                        if (n.Where(t => t.IsPerpendicularNeighbour(t1) && t.IsPerpendicularNeighbour(t2)).Count(t => t.Passable) == 2)
                        {
                            _wrappedGraph.AddEdge(new SEdge<Tile>(t1, t2));
                        }
                    }
                }
            }
        }

        #region Wrapped graph

        private readonly UndirectedGraph<Tile, SEdge<Tile>> _wrappedGraph;
        public IEnumerable<(AbstractTile, AbstractTile)> Edges => _wrappedGraph.Edges.Select(e => ((AbstractTile)e.Source, (AbstractTile)e.Target));
        public IEnumerable<AbstractTile> Vertices => _wrappedGraph.Vertices;

        public int AdjacentDegree(AbstractTile v)
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
