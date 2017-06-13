using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotIM.Scene {
    public static class RoomGenerator {
        public static double DiffPerc = 0.15;
        
        static MyRandomGenerator.MyRandom _rnd = new MyRandomGenerator.MyRandom();
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
        static List<double> GetBlurList(int nh, double h) {
            var hh = h / nh;
            var dhmax = DiffPerc * hh;
            var heights = Enumerable
                .Range(0, nh +1)
                .Select(j => {
                    return j * hh;
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

        public static List<LevelLine> GetWalls(double h, double w, int nh, int nw) {
            var cells = new Cell[nw, nh];
            for (int i = 0; i < nw; i++) {
                for (int j = 0; j < nh; j++) {
                    cells[i, j] = new Cell() { ix = i, jy = j, id = i*nh + j };
                    var clus = new CellCluster();
                    clus.CellList.Add(cells[i, j]);
                    cells[i, j].MyCluster = clus;
                }
            }           
            for (int i = 0; i < nw; i++) {
                for (int j = 0; j < nh; j++) {
                    if (i - 1 >= 0) {
                        cells[i, j].neibleft = cells[i - 1, j];
                    }
                    if (j - 1 >= 0) {
                        cells[i, j].neibbottom = cells[i, j-1];
                    }
                    if (i +1 < nw) {
                        cells[i, j].neibright = cells[i + 1, j];
                    }
                    if (j + 1 < nh) {
                        cells[i, j].neibTop = cells[i, j + 1];
                    }
                }
            }
            var yPoints = GetBlurList(nh, h);
            var xPoints = GetBlurList(nw, w);
            for (int i = 0; i < nw; i++) {
                for (int j = 0; j < nh; j++) {
                    if(i+1 < nw) { 
                        var wl = GetWall(i, j, i + 1, j, xPoints, yPoints);
                        cells[i, j].llright = wl;
                        cells[i + 1, j].llleft = wl;
                    }
                    if (j + 1 < nh) {
                        var wl = GetWall(i, j, i, j + 1, xPoints, yPoints);
                        cells[i, j].lltop = wl;
                        cells[i, j + 1].llbottom = wl;
                    }
                }
            }

            int nRoomz = _rnd.GetInt(5, 10);
            var inds = GetUniqueVectors(nw, nh, nRoomz);
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
                        .Except(bigRooms.Except(new[] { br }).SelectMany(bbr => bbr.c.MyCluster.CellList).Distinct())
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
            gl.AddPoint(w, 0);
            gl.AddPoint(w, h);
            gl.AddPoint(0, h);
            gl.AddPoint(0, 0);
            allWalls.Add(gl);
            return allWalls;
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
