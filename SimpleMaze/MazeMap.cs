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

        // Recursive backtracker
        public void GenerateWallRb() {
            var currentCell = this[(1, _inPoint)];
            currentCell.Walls[Direction.Left] = false;

            var cells = new Stack<MazeCell>();
            var NotAccessedCell = new List<KeyValuePair<Direction, MazeCell>>();

            do {
                currentCell.IsAccessed = true;
                if (currentCell.Location == (_width, _outPoint)) currentCell.Walls[Direction.Right] = false;

                NotAccessedCell = currentCell.RelativeCells.Where(e => !(e.Value?.IsAccessed ?? true))?.ToList();
                if (NotAccessedCell?.Count() > 0) {
                    var cellInfo = NotAccessedCell[_random.Next(NotAccessedCell.Count())];
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
            } while (NotAccessedCell?.Count() > 0 || cells.Count > 0);
        }

        public void GetRouteFromInToOut() {
            var currentCell = this[(1, _inPoint)];

            var cells = new Stack<MazeCell>();
            var NotAccessedCell = new List<KeyValuePair<Direction, MazeCell>>();

            do {
                if (!OutRoute.Contains(currentCell.Location)) {
                    currentCell.IsAccessed = !currentCell.IsAccessed;
                    OutRoute.Add(currentCell.Location);
                }
                if (OutRoute?.Last() == (_width, _outPoint)) {
                    break;
                }

                NotAccessedCell = currentCell.RelativeCells.Where(e => (e.Value?.IsAccessed ?? currentCell.IsAccessed) == !currentCell.IsAccessed)?.ToList();
                if (NotAccessedCell?.Count() > 0) {
                    var cellInfo = NotAccessedCell[_random.Next(NotAccessedCell.Count())];
                    if (cellInfo.Key > Direction.Top && cellInfo.Value.Walls[cellInfo.Key - 2]) {
                        continue;
                    } else if (currentCell.Walls[cellInfo.Key]) {
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
                    }
                }
            } while (NotAccessedCell?.Count() > 0 || cells.Count > 0);
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
