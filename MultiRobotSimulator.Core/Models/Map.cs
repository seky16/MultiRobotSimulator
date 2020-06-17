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

        private readonly Dictionary<(int x, int y), AbstractTile> _tileCache = new Dictionary<(int x, int y), AbstractTile>();

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
                    _tileCache.Add((x, y), tile);
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

            foreach (var obs in _wrappedGraph.Vertices.Where(t => !t.Passable))
            {
                RecalculateNeighbors(obs);
            }
        }

        private Map(int width, int height, UndirectedGraph<Tile, SEdge<Tile>> wrappedGraph)
        {
            Width = width;
            Height = height;
            _wrappedGraph = wrappedGraph.Clone();
        }

        public int Height { get; }
        public List<Tile> Starts { get; } = new List<Tile>();
        public List<Tile> Targets { get; } = new List<Tile>();
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
                    Starts.Add(tile);
                    break;

                case DrawingMode.Target:
                    tile.IsTarget = true;
                    Targets.Add(tile);
                    break;

                default:
                    break;
            }

            return true;
        }

        public bool ClearAll()
        {
            var result = false;

            Starts.Clear();
            Targets.Clear();

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

        public void ClearRobots()
        {
            Starts.Clear();
            Targets.Clear();

            foreach (var tile in _wrappedGraph.Vertices)
            {
                tile.IsStart = false;
                tile.IsTarget = false;
            }
        }

        public IEnumerable<Tile> GetNeighbors(Tile tile)
        {
            for (var yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (var xOffset = -1; xOffset <= 1; xOffset++)
                {
                    if (GetTileAtPos(tile.X + xOffset, tile.Y + yOffset) is Tile neighbour && neighbour != tile)
                    {
                        yield return neighbour;
                    }
                }
            }
        }

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

        public AbstractTile? GetTileAtPos(int x, int y) => _tileCache.TryGetValue((x, y), out var tile) ? tile : null;

        public bool RemoveFromTile(Tile tile)
        {
            var result = tile.SetToDefault();

            if (Starts.Contains(tile))
            {
                Starts.Remove(tile);
            }

            if (Targets.Contains(tile))
            {
                Targets.Remove(tile);
            }

            RecalculateNeighbors(tile);

            return result;
        }

        private bool AddEdge(Tile source, Tile target)
        {
            if (_wrappedGraph.ContainsEdge(source, target) || !source.Passable || !target.Passable)
            {
                return false;
            }

            var n = GetNeighbors(source);
            if (!n.Contains(target))
            {
                return false;
            }

            if (source.IsCornerNeighbour(target) && n.Count(t => t.IsPerpendicularNeighbour(target) && t.Passable) != 2)
            {
                return false;
            }

            return _wrappedGraph.AddEdge(new SEdge<Tile>(source, target));
        }

        private Exception GetException(AbstractTile v) => new KeyNotFoundException($"Could not find tile '{v}'");

        private void RecalculateNeighbors(Tile tile)
        {
            var n = GetNeighbors(tile);
            var p = n.Where(t => t.IsPerpendicularNeighbour(tile));

            if (!tile.Passable)
            {
                _wrappedGraph.ClearAdjacentEdges(tile);

                // don't allow to cut corners
                foreach ((var t1, var t2) in p.Pairs())
                {
                    RemoveEdge(t1, t2);
                }
            }
            else
            {
                foreach (var neighbor in n.Where(t => t.Passable))
                {
                    AddEdge(tile, neighbor);
                }

                foreach ((var t1, var t2) in p.Pairs())
                {
                    AddEdge(t1, t2);
                }
            }
        }

        private bool RemoveEdge(Tile source, Tile target)
        {
            if (_wrappedGraph.TryGetEdge(source, target, out var edge))
            {
                return _wrappedGraph.RemoveEdge(edge);
            }
            return false;
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

        public IGraph Clone()
        {
            return new Map(Width, Height, _wrappedGraph);
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

        public bool RemoveEdge((AbstractTile, AbstractTile) edge)
        {
            if (!(edge.Item1 is Tile source))
                throw GetException(edge.Item1);

            if (!(edge.Item2 is Tile target))
                throw GetException(edge.Item2);

            var tEdge = new SEdge<Tile>(source, target);
            return _wrappedGraph.RemoveEdge(tEdge);
        }

        public int RemoveEdges(IEnumerable<(AbstractTile, AbstractTile)> edges)
        {
            var tEdges = new List<SEdge<Tile>>(edges.Count());
            foreach (var edge in edges)
            {
                if (!(edge.Item1 is Tile source))
                    throw GetException(edge.Item1);

                if (!(edge.Item2 is Tile target))
                    throw GetException(edge.Item2);

                tEdges.Add(new SEdge<Tile>(source, target));
            }

            return _wrappedGraph.RemoveEdges(tEdges);
        }

        public bool RemoveVertex(AbstractTile v)
        {
            if (!(v is Tile tile))
                throw GetException(v);

            return _wrappedGraph.RemoveVertex(tile);
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

        #endregion Wrapped graph
    }
}
