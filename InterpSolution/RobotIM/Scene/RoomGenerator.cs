using Interpolator;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotIM.Scene {
    public class RoomGeneratorSettings {
        public double h { get; set; }
        public double w { get; set; }
        public int nh { get; set; }
        public int nw { get; set; }
        public double diff { get; set; } = 0.15;
        public int nmin { get; set; } = 5;
        public int nmax { get; set; } = 10;
        public double FurRo { get; set; } = 0.3;
        public double FHmin { get; set; } = 1;
        public double FHmax{ get; set; } = 3;
    }

    public static class RoomGenerator {       
        public static MyRandomGenerator.MyRandom _rnd = new MyRandomGenerator.MyRandom();
        static List<(int x, int y)> GetUniqueVectors(int w, int h, int n) {
            var answ = new List<(int x, int y)>(n);
            for (int i = 0; i < n; i++) {
                bool flag = true;
                var vec = (x:0, y: 0);
                while (flag) {                  
                    flag = false;
                    vec = (x: _rnd.GetInt(0, w), y: _rnd.GetInt(0, h));
                    foreach (var v in answ) {
                        if(v.x == vec.x && v.y == vec.y) {
                            flag = true;
                            break;
                        }
                    }
                }
                answ.Add(vec);
            }
            return answ;
        }
        public static List<double> GetBlurList(int nh, double h, double diff, double d0 = 0d) {
            var hh = h / nh;
            var dhmax = diff * hh;
            var heights = Enumerable
                .Range(0, nh +1)
                .Select(j => {
                    return d0 + j * hh;
                })
                .ToList();
            for (int j = 1; j < heights.Count-1; j++) {
                var dh = _rnd.GetDouble(-dhmax, dhmax);
                heights[j] += dh;
            }
            return heights;
        }
        static LevelLine GetWall(int i1, int j1, int i2, int j2, List<double> widths, List<double> heights) {
            var ll = new LevelLine();
            void MinAMaxB(ref int a, ref int b)
            {
                if (a > b) {
                    int c = a;
                    a = b;
                    b = c;
                }
            }
            MinAMaxB(ref i1, ref i2);
            MinAMaxB(ref j1, ref j2);

            ll.AddPoint(widths[i2], heights[j2]);
            ll.AddPoint(widths[i2 + (j2 - j1)], heights[j2 + (i2 - i1)]);

            return ll;
        }

        public static (List<LevelLine> walls, List<Furniture> furs) GetWallsAndFurs(RoomGeneratorSettings settings) {
            var cells = new Cell[settings.nw, settings.nh];
            for (int i = 0; i < settings.nw; i++) {
                for (int j = 0; j < settings.nh; j++) {
                    cells[i, j] = new Cell() { ix = i, jy = j, id = i* settings.nh + j };
                    var clus = new CellCluster();
                    clus.CellList.Add(cells[i, j]);
                    cells[i, j].MyCluster = clus;
                }
            }           
            for (int i = 0; i < settings.nw; i++) {
                for (int j = 0; j < settings.nh; j++) {
                    if (i - 1 >= 0) {
                        cells[i, j].neibleft = cells[i - 1, j];
                    }
                    if (j - 1 >= 0) {
                        cells[i, j].neibbottom = cells[i, j-1];
                    }
                    if (i +1 < settings.nw) {
                        cells[i, j].neibright = cells[i + 1, j];
                    }
                    if (j + 1 < settings.nh) {
                        cells[i, j].neibTop = cells[i, j + 1];
                    }
                }
            }
            var yPoints = GetBlurList(settings.nh, settings.h, settings.diff);
            var xPoints = GetBlurList(settings.nw, settings.w, settings.diff);
            for (int i = 0; i < settings.nw; i++) {
                for (int j = 0; j < settings.nh; j++) {
                    if(i+1 < settings.nw) { 
                        var wl = GetWall(i, j, i + 1, j, xPoints, yPoints);
                        cells[i, j].llright = wl;
                        cells[i + 1, j].llleft = wl;
                    }
                    if (j + 1 < settings.nh) {
                        var wl = GetWall(i, j, i, j + 1, xPoints, yPoints);
                        cells[i, j].lltop = wl;
                        cells[i, j + 1].llbottom = wl;
                    }
                }
            }
            List<Furniture> furs = new List<Furniture>();
            foreach (var c in cells) {
                if(_rnd.GetDouble() < settings.FurRo) {
                    int n = _rnd.GetInt(3, 7);
                    double fheight = _rnd.GetDouble(settings.FHmin, settings.FHmax);
                    furs.Add(c.GenerateFurInMe(n, fheight,lilbit:0.4,s_otn:0.3));
                }
            }
            int nRoomz = _rnd.GetInt(settings.nmin, settings.nmax);
            var inds = GetUniqueVectors(settings.nw, settings.nh, nRoomz);
            var bigRooms = Enumerable
                .Range(0, nRoomz)
                .Select(i => new BigRoom() {
                    cellMax = _rnd.GetInt(10, 20),
                    c = cells[inds[i].x, inds[i].y]
                })
                .ToList();

            int cellMax = bigRooms.Select(br => br.cellMax).Max();
            bool flag = true;
            while (flag) {
                flag = false;
                foreach (var br in bigRooms) {
                    //if (br.cellMax < i)
                    //    continue;
                    var possNeibs = br.c.MyCluster.Neibs
                        .Except(bigRooms.Except(new[] { br })
                        .SelectMany(bbr => bbr.c.MyCluster.CellList).Distinct())
                        .ToList();
                    if (possNeibs.Count == 0)
                        continue;
                    int nextCellind = _rnd.GetInt(0, possNeibs.Count);
                    br.c.AddToClusters(possNeibs[nextCellind]);
                    flag = true;
                }
            }
            foreach (var br in bigRooms) {
                foreach (var c1 in br.c.MyCluster.CellList) {
                    foreach (var c2 in br.c.MyCluster.CellList) {
                        c1.DellWall(c2);
                    }
                }
            }

            var cl = cells[0, 0];
            var nbs = cl.MyCluster.Neibs;
            while (nbs.Count > 0) {
                int ind = _rnd.GetInt(0, nbs.Count);
                cl.AddToClusters(nbs[ind]);
                nbs = cl.MyCluster.Neibs;
            }

            //cells[1, 1].DellWall(cells[1, 0]);

            var allWalls = cells.Cast<Cell>()
                .SelectMany(c => c.Walls)
                .Where(levLine => levLine != null)
                .Distinct()
                .ToList();

            var gl = new LevelLine();
            gl.AddPoint(0, 0);
            gl.AddPoint(settings.w, 0);
            gl.AddPoint(settings.w, settings.h);
            gl.AddPoint(0, settings.h);
            gl.AddPoint(0, 0);
            allWalls.Add(gl);
            return (allWalls, furs);
        }

        class BigRoom {
            public int cellMax;
            public Cell c;
        }

        class Cell {
            public int ix,jy,id;
            public LevelLine lltop, llbottom, llleft, llright;
            public IEnumerable<LevelLine> Walls {
                get {
                    yield return lltop;
                    yield return llbottom;
                    yield return llleft;
                    yield return llright;
                }
            }
            public Cell neibTop, neibleft, neibbottom, neibright;
            public IEnumerable<Cell> Neibs {
                get {
                    return NeibsRouth.Where(n => n != null);
                }
            }
            IEnumerable<Cell> NeibsRouth {
                get {
                    yield return neibTop;
                    yield return neibleft;
                    yield return neibbottom;
                    yield return neibright;
                }
            }
            public bool IsNeib(Cell possNeib) {
                return Neibs.Any(n => ReferenceEquals(n, possNeib));
            }
            public CellCluster MyCluster;

            public List<Cell> AddToClusters(Cell c, bool deleteWall = true) {
                var megaCluster = MyCluster.CellList
                    .Concat(c.MyCluster.CellList)
                    .Distinct()
                    .ToList();
                
                var oldNeibs = c.MyCluster.Neibs
                    .Except(MyCluster.CellList)
                    .ToList();
                if (deleteWall)
                    DellWall(c);
                MyCluster.CellList = megaCluster;
                c.MyCluster = MyCluster;

                return oldNeibs;
            }

            public void DellWall(Cell neib1) {
                Cell neib = this;
                if (!neib.IsNeib(neib1)) {
                    foreach (var c in MyCluster.CellList) {
                        if (c.IsNeib(neib1)) {
                            neib = c;
                            break;
                        }
                    }
                }


                if(LevelLine.ReferenceEquals(neib.lltop, neib1.llbottom)) {
                    neib.lltop = null;
                    neib1.llbottom = null;
                }
                if (LevelLine.ReferenceEquals(neib.llbottom, neib1.lltop)) {
                    neib.llbottom = null;
                    neib1.lltop = null;
                }
                if (LevelLine.ReferenceEquals(neib.llleft, neib1.llright)) {
                    neib.llleft = null;
                    neib1.llright = null;
                }
                if (LevelLine.ReferenceEquals(neib.llright, neib1.llleft)) {
                    neib.llright = null;
                    neib1.llleft = null;
                }
            }

            public Furniture GenerateFurInMe(int n, double height, double irregular_rate = 0.1, double s_otn = 0.5, double lilbit = 0.9) {
                #region local functions
                double get_t(Vector2D r0, Vector2D rn0, Vector2D p1, Vector2D p2) {
                    var v1 = r0 - p1;
                    var v2 = p2 - p1;
                    var v3 = new Vector2D(-rn0.Y, rn0.X);


                    var dot = v2 * v3;
                    if (Math.Abs(dot) < 0.000001)
                        return -99;

                    double CrossProduct(Vector2D a, Vector2D b) {
                        return a.X * b.Y - b.X * a.Y;
                    }

                    var t1 = CrossProduct(v2, v1) / dot;
                    var t2 = (v1 * v3) / dot;

                    if (t1 >= 0.0 && (t2 >= 0.0 && t2 <= 1.0)) {
                        return t1;
                    }
                    return -98;
                }
                double getSquare(List<Vector2D> pList) {
                    double a = 0d;
                    for (int i = 0; i < pList.Count - 1; i++) {
                        a += pList[i].X * pList[i + 1].Y - pList[i + 1].X * pList[i].Y;
                    }
                    a += pList[pList.Count - 1].X * pList[0].Y - pList[0].X * pList[pList.Count - 1].Y;
                    a /= 2;
                    return Math.Abs(a);
                }
                #endregion
                var allps = Walls
                    .Where(ll => ll != null)
                    .SelectMany(ll => ll.pointsList);
                var gp1 = new Vector2D(allps.Min(wv => wv.X), allps.Min(wv => wv.Y));
                var gp2 = new Vector2D(allps.Max(wv => wv.X), allps.Max(wv => wv.Y));
                var center = 0.5 * (gp1 + gp2);
                double poly_sq = 0d;
                double cell_sq = (gp2 - gp1).X * (gp2 - gp1).Y;
                List<Vector2D> ps_cros, n0_list;
                int inc = 0;
                do {
                    var angles = RoomGenerator.GetBlurList(n, 360, irregular_rate, RoomGenerator._rnd.GetDouble(0, 90))
                        .Take(n)
                        .ToList();
                    var rad = PI / 180d;

                    n0_list = angles
                        .Select(a => new Vector2D(Sin(a * rad), Cos(a * rad)).Norm)
                        .ToList();

                    ps_cros = n0_list
                        .Select(n0 => {
                            var wp1 = gp1;
                            var wp2 = new Vector2D(gp1.X, gp2.Y);
                            var wp3 = gp2;
                            var wp4 = new Vector2D(gp2.X, gp1.Y);
                            return new[]
                                {
                                    new { p1 = wp1, p2 = wp2 },
                                    new { p1 = wp2, p2 = wp3 },
                                    new { p1 = wp3, p2 = wp4 },
                                    new { p1 = wp4, p2 = wp1 }
                                }
                                .Select(ao => get_t(center, n0, ao.p1, ao.p2))
                                .Where(t => t > 0)
                                .Select(t => center + n0 * t)
                                .First();
                        })
                        .ToList();
                    poly_sq = getSquare(ps_cros);
                    inc++;
                } while (poly_sq < s_otn*cell_sq && inc < 33);

                var res_p = ps_cros
                    .Select(p => {
                        return center + lilbit * (p - center);
                    });
                var res = new Furniture(height);
                foreach (var p in res_p) {
                    res.AddPoint(VecConv.Vec2DToWinVec(p));
                }
                res.Close();
                return res;
            }
        }

        class CellCluster {
            public List<Cell> CellList = new List<Cell>();
            public List<Cell> Neibs {
                get {
                    return CellList
                         .SelectMany(cell => cell.Neibs)         
                         .Distinct()
                         .Except(CellList)
                         .ToList();                   
                }
            }
        }
    }
}
