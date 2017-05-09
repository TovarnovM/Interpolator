using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpPathFinding.cs;
using Interpolator;
using MoreLinq;
using static System.Math;
using Sharp3D.Math.Core;

namespace RobotIM.Scene {
    public class Room {
        public List<LevelLine> walls = new List<LevelLine>();
        public  StaticGrid searchGrid;
        JumpPointParam jumpParam;
        public double cellsize = 0.1;
        public (Vector2D p1, Vector2D p2) gabarit;
        public int nw, nh;

        (Vector2D p1, Vector2D p2) GetGabarit() {
            var left =      walls.SelectMany(ll => ll.pointsList).Min(p => p.X);
            var right =     walls.SelectMany(ll => ll.pointsList).Max(p => p.X);
            var top =       walls.SelectMany(ll => ll.pointsList).Max(p => p.Y);
            var bottom =    walls.SelectMany(ll => ll.pointsList).Min(p => p.Y);
            return (new Vector2D(left, bottom), new Vector2D(right, top));
        }

        GridPos GetGridCoordsGP(Vector2D p) {
            var t = GetGridCoords(p);
            return new GridPos(t.ix, t.iy);
        }
        (int ix, int iy) GetGridCoords(Vector2D p) {
            return GetGridCoords(p.X, p.Y);
        }
        public (int ix, int iy) GetGridCoords(double x, double y) {
            var x_otn = (x - gabarit.p1.X);
            int ix = x_otn < 0 ? 0 :
                x_otn > gabarit.p2.X ? nw-1 :
                (int)Floor(x_otn / cellsize);
            var y_otn = (y - gabarit.p1.Y);
            int iy = y_otn < 0 ? 0 :
                y_otn > gabarit.p2.Y ? nh-1 :
                (int)Floor(y_otn / cellsize);
            return (ix, iy);
        }
        public Vector2D GetCellCenter(int ix, int iy) {
            var x = gabarit.p1.X + 0.5 * cellsize + ix * cellsize;
            var y = gabarit.p1.Y + 0.5 * cellsize + iy * cellsize;
            return new Vector2D(x, y);
        }
        public IEnumerable<(Vector2D center, int ix, int iy)> GetAllCellCenters() {
            for (int i = 0; i < nw; i++) {
                for (int j = 0; j < nh; j++) {
                    yield return (GetCellCenter(i, j), i, j);     
                }
            }
        }
        public IEnumerable<(Vector2D p1, Vector2D p2, int ix, int iy)> GetAllCellRects() {
            var half = new Vector2D(cellsize / 2, cellsize / 2);
            for (int i = 0; i < nw; i++) {
                for (int j = 0; j < nh; j++) {
                    var center = GetCellCenter(i, j);
                    yield return (center - half,center+half, i, j);
                }
            }
        }


        public void SynchCellSizeNsWithGab() {
            double w = Abs(gabarit.p2.X - gabarit.p1.X);
            double h = Abs(gabarit.p2.Y - gabarit.p1.Y);
            nw = (int)Ceiling(w / cellsize);
            nh = (int)Ceiling(h / cellsize);
            cellsize = Max(w / nw, h / nh);
        }

        public bool[][] GetBoolGrid() {
            var bgrid = new bool[nw][];
            for (int i = 0; i < nw; i++) {
                bgrid[i] = new bool[nh];
                for (int j = 0; j < nh; j++) {
                    bgrid[i][j] = true;
                }
            }

            foreach (var cell in GetAllCellRects()) {
                var p1 = new System.Windows.Vector(cell.p1.X, cell.p1.Y);
                var p2 = p1 + new System.Windows.Vector(cell.p2.X - cell.p1.X, 0);
                var p3 = new System.Windows.Vector(cell.p2.X, cell.p2.Y);
                var p4 = p1 + new System.Windows.Vector(0,cell.p2.Y - cell.p1.Y);

                foreach (var wall in walls) {
                    bool cross = wall.IsCrossMe(p1, p2) ||
                        wall.IsCrossMe(p2, p3) ||
                        wall.IsCrossMe(p3, p4) ||
                        wall.IsCrossMe(p1, p4);
                    if(cross) {
                        bgrid[cell.ix][cell.iy] = false;
                        break;
                    }
                }
            }
            return bgrid;
        }
        public void CreateScene() {
            gabarit = GetGabarit();
            SynchCellSizeNsWithGab();
            searchGrid = new StaticGrid(nw, nh, GetBoolGrid());
            jumpParam = new JumpPointParam(searchGrid, true, false, false);
        }

        public List<Vector2D> FindPath(Vector2D from_pos, Vector2D to_pos) {
            var fr = GetGridCoordsGP(from_pos);
            var to = GetGridCoordsGP(to_pos);
            jumpParam.Reset(fr, to);
            var resGP = JumpPointFinder.FindPath(jumpParam);
            var res = new List<Vector2D>(resGP.Count+1);
            for (int i = 0; i < resGP.Count; i++) {
                res.Add(GetCellCenter(resGP[i].x, resGP[i].y));
            }
            res.Add(to_pos);
            return res;
        }
    }
}
