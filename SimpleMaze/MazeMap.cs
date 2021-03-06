﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleMaze {
    enum Direction {
        Left,
        Top,
        Right,
        Bottom
    }

    class MazeMap : IEnumerable<MazeCell> {
        private readonly Random _random;
        private readonly int _width, _height, _inPoint, _outPoint;
        private Dictionary<(int X, int Y), MazeCell> Cells { get; set; }
        public List<(int X, int Y)> OutRoute { get; private set; }

        IEnumerator<MazeCell> IEnumerable<MazeCell>.GetEnumerator() {
            return Cells.Select(e => e.Value).AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Cells.GetEnumerator();
        }

        private MazeMap() {
            _random = new Random();
            Cells = new Dictionary<(int X, int Y), MazeCell>();
            OutRoute = new List<(int X, int Y)>();
        }

        /// <param name="width">Must grate than 1</param>
        /// <param name="height">Must grate than 1</param>
        /// <param name="inPoint">Must grate than 1 and less than height</param>
        /// <param name="outPoint">Must grate than 1 and less than height</param>
        public MazeMap(int width, int height, int inPoint, int outPoint) : this(width, height, inPoint, outPoint, null) {
            for (int row = 1; row <= _height; row++) {
                for (int col = 1; col <= _width; col++) {
                    var cell = new MazeCell((col, row), this);
                    if (col < _width) cell.Walls[Direction.Right] = false;
                    if (row < _height) cell.Walls[Direction.Bottom] = false;
                    this.Cells.Add((col, row), cell);
                }
            }
            this[(1, _inPoint)].Walls[Direction.Left] = false;
            this[(_width, _outPoint)].Walls[Direction.Right] = false;
        }

        public MazeMap(int width, int height, int inPoint, int outPoint, List<((int X, int Y) Location, (bool L, bool T, bool R, bool B) Walls)> mapInfo) : this() {
            if (width < 1 || height < 1 || inPoint < 1 || inPoint > height || outPoint < 1 || outPoint > height) {
                throw new Exception("Invalid parameter for create map.");
            }
            (_width, _height, _inPoint, _outPoint) = (width, height, inPoint, outPoint);
            if (mapInfo != null) {
                foreach (var item in mapInfo) {
                    var cell = new MazeCell((item.Location.X, item.Location.Y), this);
                    cell.Walls[Direction.Left] = item.Walls.L;
                    cell.Walls[Direction.Top] = item.Walls.T;
                    cell.Walls[Direction.Right] = item.Walls.R;
                    cell.Walls[Direction.Bottom] = item.Walls.B;
                    this.Cells.Add((item.Location.X, item.Location.Y), cell);
                }
            }
        }

        public MazeCell this[(int X, int Y) location] {
            get => (location.X < 1 || location.X > _width || location.Y < 1 || location.Y > _height) ? null : Cells[location];
        }

        /// <summary>
        /// Recursive backtracker
        /// </summary>
        public void GenerateWallRb() {
            var currentCell = this[(1, _inPoint)];

            var cells = new Stack<MazeCell>();
            var notAccessedCell = new List<KeyValuePair<Direction, MazeCell>>();

            do {
                currentCell.IsAccessed = true;

                notAccessedCell = currentCell.RelativeCells.Where(e => !(e.Value?.IsAccessed ?? true))?.ToList();
                if (notAccessedCell?.Count() > 0) {
                    var cellInfo = notAccessedCell[_random.Next(notAccessedCell.Count())];
                    if (cellInfo.Key > Direction.Top) {
                        cellInfo.Value.Walls[cellInfo.Key - 2] = false;
                    } else {
                        currentCell.Walls[cellInfo.Key] = false;
                    }
                    cells.Push(currentCell);
                    currentCell = cellInfo.Value;
                } else if (cells.Count > 0) {
                    currentCell = cells.Pop();
                }
            } while (notAccessedCell?.Count() > 0 || cells.Count > 0);
        }

        /// <summary>
        /// Randomized Prim
        /// </summary>
        public void GenerateWallRp() {
            var currentCell = this[(1, _inPoint)];
            var notAccessedCell = new List<MazeCell>();

            do {
                currentCell.IsAccessed = true;
                notAccessedCell = Cells.Values.Where(e => !(e.IsAccessed) && e.RelativeCells.Any(e1 => e1.Value?.IsAccessed ?? false))?.ToList();
                if (notAccessedCell?.Count() > 0) {
                    currentCell = notAccessedCell[_random.Next(notAccessedCell.Count())];
                    var accessedCells = currentCell.RelativeCells.Where(e => e.Value?.IsAccessed ?? false).ToList();
                    var cellInfo = accessedCells[_random.Next(accessedCells.Count())];
                    if (cellInfo.Key > Direction.Top) {
                        cellInfo.Value.Walls[cellInfo.Key - 2] = false;
                    } else {
                        currentCell.Walls[cellInfo.Key] = false;
                    }
                }
            } while (notAccessedCell?.Count() > 0);
        }

        /// <summary>
        /// Recursive division
        /// </summary>
        public void GenerateWallRd() {
            foreach (var item in Cells.Values) {
                if (item.Location.X > 1) item.Walls[Direction.Left] = false;
                if (item.Location.Y > 1) item.Walls[Direction.Top] = false;
            }

            var divs = new Stack<(int minX, int maxX, int minY, int maxY)>();
            divs.Push((1, _width + 1, 1, _height + 1));
            do {
                var div = divs.Pop();
                if ((div.maxX - div.minX < 2)) {
                    Cells.Values.Where(e => e.Location.X == div.maxX && e.Location.X < div.maxX && e.Location.X >= div.minX && e.Location.Y < div.maxY && e.Location.Y >= div.minY).ToList().ForEach(e => e.Walls[Direction.Top] = false);
                    continue;
                }
                if ((div.maxY - div.minY < 2)) {
                    Cells.Values.Where(e => e.Location.Y == div.maxY && e.Location.X < div.maxX && e.Location.X >= div.minX && e.Location.Y < div.maxY && e.Location.Y >= div.minY).ToList().ForEach(e => e.Walls[Direction.Left] = false);
                    continue;
                }

                var col = _random.Next(div.minX + 1, div.maxX);
                var row = _random.Next(div.minY + 1, div.maxY);
                if ((div.maxX - div.minX > 2) && (col % 2 > 0)) col++;
                if ((div.maxY - div.minY > 2) && (row % 2 > 0)) row++;
                if (col > div.maxX) col -= 2;
                if (row > div.maxY) row -= 2;

                divs.Push((col, div.maxX, row, div.maxY));
                divs.Push((div.minX, col, row, div.maxY));
                divs.Push((col, div.maxX, div.minY, row));
                divs.Push((div.minX, col, div.minY, row));

                Cells.Values.Where(e => e.Location.X == col && e.Location.X < div.maxX && e.Location.X >= div.minX && e.Location.Y < div.maxY && e.Location.Y >= div.minY).ToList().ForEach(e => e.Walls[Direction.Left] = true);
                Cells.Values.Where(e => e.Location.Y == row && e.Location.X < div.maxX && e.Location.X >= div.minX && e.Location.Y < div.maxY && e.Location.Y >= div.minY).ToList().ForEach(e => e.Walls[Direction.Top] = true);
                var walls = new List<((int x, int y) location, Direction direction)>() {
                    ((col, _random.Next(div.minY + 1, row)), Direction.Left),
                    ((col, _random.Next(row, div.maxY)), Direction.Left),
                    ((_random.Next(div.minX + 1, col), row), Direction.Top),
                    ((_random.Next(col, div.maxX), row), Direction.Top)
                };
                foreach (var wall in walls) {
                    Cells[wall.location].Walls[wall.direction] = false;
                }

            } while (divs.Count > 0);
        }

        public void GetRouteFromInToOut() {
            OutRoute.Clear();
            var currentCell = this[(1, _inPoint)];

            var cells = new Stack<MazeCell>();
            var NotAccessedCell = new List<KeyValuePair<Direction, MazeCell>>();
            var removedRoute = new List<(int x, int y, Direction)>();

            do {
                if (!OutRoute.Contains(currentCell.Location)) {
                    OutRoute.Add(currentCell.Location);
                }
                if (OutRoute?.Last() == (_width, _outPoint)) {
                    break;
                }

                NotAccessedCell = currentCell.RelativeCells.Where(e => e.Value != null && !OutRoute.Contains(e.Value.Location) && !removedRoute.Contains((currentCell.Location.X, currentCell.Location.Y, e.Key)))?.ToList();

                if (NotAccessedCell?.Count() > 0) {
                    var cellInfo = NotAccessedCell[0];
                    removedRoute.Add((currentCell.Location.X, currentCell.Location.Y, cellInfo.Key));
                    if ((cellInfo.Key > Direction.Top && cellInfo.Value.Walls[cellInfo.Key - 2]) || (currentCell.Walls[cellInfo.Key])) {
                        continue;
                    }
                    cells.Push(currentCell);
                    currentCell = cellInfo.Value;
                } else {
                    if (OutRoute?.Last() == currentCell.Location && currentCell.Location != (_width, _outPoint)) {
                        OutRoute.Remove(currentCell.Location);
                    }
                    if (cells.Count > 0) {
                        currentCell = cells.Pop();
                    } else {
                        currentCell = null;
                    }
                }
            } while (currentCell != null);
        }
    }

    class MazeCell {
        private Dictionary<Direction, MazeCell> _cells;

        public bool IsAccessed { get; set; }
        public (int X, int Y) Location { get; set; }
        public Dictionary<Direction, MazeCell> RelativeCells {
            get {
                return _cells ??= new Dictionary<Direction, MazeCell>() {
                    { Direction.Left, Map[(Location.X - 1, Location.Y)] },
                    { Direction.Top, Map[(Location.X, Location.Y - 1)] },
                    { Direction.Right, Map[(Location.X + 1, Location.Y)] },
                    { Direction.Bottom, Map[(Location.X, Location.Y + 1)] },
                };
            }
        }
        public Dictionary<Direction, bool> Walls { get; set; }
        public MazeMap Map { get; }

        private MazeCell() {
            Walls = new Dictionary<Direction, bool> {
                { Direction.Left, true },
                { Direction.Top, true },
                { Direction.Right, true },
                { Direction.Bottom, true }
            };
        }
        public MazeCell((int x, int y) location, MazeMap map) : this() {
            Location = location;
            Map = map;
        }
    }
}
