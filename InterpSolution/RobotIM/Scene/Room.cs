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
using MyRandomGenerator;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace RobotIM.Scene {
    [Serializable]
    public class Room {
        public void SaveToFile(string fileName) {
            using (var jsw = new JsonTextWriter(new StreamWriter(fileName))) {
                var ser = JsonSerializer.Create();
                var rd = new RoomData();
                rd.walls = walls;
                rd.furnitures = furnitures;
                rd.cellsize = cellsize;
                rd.nh = nh;
                rd.nw = nw;
                rd.staticNoiseMap = staticNoiseMap;
                rd.noisePoints = staticNoisesList;
                ser.Serialize(jsw, rd);
            } 

        }
        public void LoadFromFile(string fileName) {
            var jstr = new JsonTextReader(new StreamReader(fileName));
            var ser = JsonSerializer.Create();
            var rd = ser.Deserialize<RoomData>(jstr);
            walls = rd.walls;
            cellsize = rd.cellsize;
            furnitures = rd.furnitures;
            nh = rd.nh;
            nw = rd.nw;
            staticNoiseMap = rd.staticNoiseMap;
            staticNoisesList = rd.noisePoints;
            CreateScene();

        }
        public Room Clone() {
            var res = new Room();
            res.walls = new List<LevelLine>(walls);
            res.searchGrid = (StaticGrid)searchGrid.Clone();
            res.jumpParam = new JumpPointParam(res.searchGrid, true, false, false);
            res.cellsize = cellsize;
            res.gabarit = gabarit;
            res.nw = nw;
            res.nh = nh;
            res.staticNoisesList = new List<StaticNoisePoint>(staticNoisesList);
            res.staticNoiseMap = staticNoiseMap;
            return res;
        }
        class RoomData {
            public List<LevelLine> walls = new List<LevelLine>();
            public List<Furniture> furnitures = new List<Furniture>();
            public double cellsize = 0.1;
            public int nw, nh;
            public double[,] staticNoiseMap;
            public List<StaticNoisePoint> noisePoints;
        }
        public List<LevelLine> walls = new List<LevelLine>();
        public List<Furniture> furnitures = new List<Furniture>();
        MyRandom _rnd = new MyRandom();
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
            var (ix, iy) = GetGridCoords(p);
            return new GridPos(ix, iy);
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

                foreach (var fur in furnitures) {
                    bool cross = fur.IsCrossMe(p1, p2) ||
                        fur.IsCrossMe(p2, p3) ||
                        fur.IsCrossMe(p3, p4) ||
                        fur.IsCrossMe(p1, p4) ||
                        fur.IsCrossMe(p1, p3) ||
                        fur.IsCrossMe(p2, p4);
                    if (cross) {
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

        public List<Vector2D> FindPath(Vector2D from_pos, Vector2D to_pos, bool trimPath = true) {
            var fr = GetGridCoordsGP(from_pos);
            var to = GetGridCoordsGP(to_pos);
            jumpParam.Reset(fr, to);
            var resGP = JumpPointFinder.FindPath(jumpParam);
            var res = new List<Vector2D>(resGP.Count+1);
            for (int i = 0; i < resGP.Count; i++) {
                res.Add(GetCellCenter(resGP[i].x, resGP[i].y));
            }
            res.Add(to_pos);
            if(trimPath)
                res = TrimPath(res);
            return res;
        }

        public List<Vector2D> TrimPath(List<Vector2D> path) {
            var answ = new List<Vector2D>(path);
            int i = 0;
            while( i < answ.Count-3 ) {
                answ[i + 1] = GetTrimed(answ[i], answ[i + 1], answ[i + 2]);
                if (Vector2D.ApproxEqual(answ[i + 1], answ[i + 2])) {
                    answ.RemoveAt(i + 1);
                } else
                    i++;
            }
            return answ;
        }

        Vector2D GetTrimed(Vector2D p0, Vector2D p1, Vector2D p2, int nShag = 7) {
            if (!IsCrossWalls(p0, p2))
                return p2;
            for (int i = 0; i < nShag; i++) {
                var p15 = 0.5 * (p1 + p2);
                if (IsCrossWalls(p0, p15))
                    p2 = p15;
                else
                    p1 = p15;
            }
            return p1;


        }

        public bool IsCrossWalls(Vector2D a1, Vector2D a2) {
            System.Windows.Vector b1 = new System.Windows.Vector(a1.X, a1.Y);
            System.Windows.Vector b2 = new System.Windows.Vector(a2.X, a2.Y);
            foreach (var wall in walls) {
                if (wall.IsCrossMe(b1, b2))
                    return true;
            }
            foreach (var fur in furnitures) {
                if (fur.IsCrossMe(b1, b2))
                    return true;
            }
            return false;
        }

        public bool WalkableCoord(Vector2D p) {
            if (p.X < gabarit.p1.X ||
                p.X > gabarit.p2.X ||
                p.Y < gabarit.p1.Y ||
                p.Y > gabarit.p2.Y)
                return false;
            var inds = GetGridCoords(p);
            return searchGrid.IsWalkableAt(inds.ix, inds.iy);
        }

        public Vector2D GetWalkableCoord() {
            return GetWalkableCoord(gabarit.p1, gabarit.p2);
        }

        public Vector2D GetWalkableCoord(Vector2D p1, Vector2D p2) {
            for (int i = 0; i < 100; i++) {
                var p = new Vector2D(_rnd.GetDouble(p1.X, p2.X), _rnd.GetDouble(p1.Y, p2.Y));
                if (WalkableCoord(p))
                    return p;
            }
            throw new ArgumentException("Baad Cords Rectangle");
        }

        public double GetDistanceBetween(Vector2D f, Vector2D t, bool precise = true) {
            var l = FindPath(f, t);
            if (precise)
                l = TrimPath(l);
            double dist = 0d;
            for (int i = 1; i < l.Count; i++) {
                dist += (l[i - 1] - l[i]).GetLength();
            }
            return dist;
        }

        public List<StaticNoisePoint> staticNoisesList = new List<StaticNoisePoint>();
        public double[,] staticNoiseMap;
        public void InitNoiseMap() {
            staticNoiseMap = new double[searchGrid.width, searchGrid.height];
            foreach (var np in staticNoisesList) {
                for (int i = 0; i < searchGrid.width; i++) {
                    for (int j = 0; j < searchGrid.height; j++) {
                        var p = GetCellCenter(i, j);
                        var noise = 0d;
                    
                        var n = np.GetDBTo(p, this, true);
                        if (Abs(n - np.noiseDB) < 0.01 && (np.GetPos() - p).GetLength() > 1+cellsize)
                            n = 0d;
                        noise += Pow(10,0.1*n);
                        
                        staticNoiseMap[i, j] += noise;
                    }
                }
            }
            for (int i = 0; i < searchGrid.width; i++) {
                for (int j = 0; j < searchGrid.height; j++) {
                    staticNoiseMap[i, j] = 10 * Log10(staticNoiseMap[i, j]);
                }
            }

        }
        public double GetStaticNoiseAt(Vector2D hearP) {
            var c = GetGridCoords(hearP);
            return staticNoiseMap[c.ix, c.iy];
        }

        #region Factory
        public static Room Factory(string fileName = "current") {
            if(fileName != "current" && fileName != _currFileName) {             
                _currRoom.Value.LoadFromFile(fileName);
                _currFileName = fileName;
            }
            return _currRoom.Value.Clone();
        }       
        static string _currFileName = "current";
        static readonly Lazy<Room> _currRoom = new Lazy<Room>(() => new Room());
        #endregion

    }

    public class Furniture: Polygon {
        public Furniture(double height): base(height) {

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RayIntersect3D(Vector3D p1,Vector3D p2) {
            return RayIntersect3D(new Vector2D(p1.X, p1.Y), p1.Z, new Vector2D(p2.X, p2.Y), p2.Z);
        }
        public bool RayIntersect3D(Vector2D p1, double h1, Vector2D p2, double h2) {
            if (Min(h1, h2) > Value) {
                return false;
            }
            if ( !IsCrossMe(VecConv.Vec2DToWinVec(p1), VecConv.Vec2DToWinVec(p2))) {
                return false;
            }
            if(Max(h1,h2) < Value) {
                return true;
            }

            var p3_1 = new Vector3D(p1.X, p1.Y, h1);
            var p3_2 = new Vector3D(p2.X, p2.Y, h2);
            var p0 = new Vector3D(0, 0, Value);
            var u = (p3_1 - p3_2).Norm;
            var n0 = new Vector3D(0, 0, 1);
            var dot = n0 * u;
            if (Math.Abs(dot) > 1E-8) {
                var w = p3_1 - p0;
                var fac = -(n0 * w) / dot;
                var intersect_p3 =  p3_1 + (u * fac);
                var intersect_p2 = new Vector2D(intersect_p3.X, intersect_p3.Y);
                return !IsInside(VecConv.Vec2DToWinVec(intersect_p2));
            }
            return true;

        }
    }

    public static class VecConv {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2D WinVecToVec2D(System.Windows.Vector winVec) {
            return new Vector2D(winVec.X, winVec.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Windows.Vector Vec2DToWinVec(Vector2D vec) {
            return new System.Windows.Vector(vec.X, vec.Y);
        }
    }
}
