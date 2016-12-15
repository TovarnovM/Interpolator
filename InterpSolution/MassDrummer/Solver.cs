using Microsoft.Research.Oslo;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace MassDrummer {
    public class Solver {
        public static double GetCylIntegrX(Func<double, double> f_ot_x, double a, double b, int n = 300) {
            var f2 = new Func<double,Vector,Vector>((x, v) => new Vector(Pow(f_ot_x(x),2)));
            var v0 = new Vector(0);
            var sol = Ode.Adams4(a,v0,f2,(b - a) / n).SolveTo(b);
            return PI*sol.Last().X[0];
        }


    }

    public abstract class ShapeBase {
        public abstract double X0 { get; }
        public abstract double X1 { get; }
        public abstract double F_ot_x(double x);
        public abstract IList<DataPoint> GetPoints();

        public virtual IList<DataPoint> GetPoints2() {
            return GetPoints().Select(p => new DataPoint(p.X,-p.Y)).ToList();
        }
        protected Dictionary<string,double> Params = new Dictionary<string,double>(10);
        public double this[string key] {
            get { return Params[key];  }
            set { Params[key] = value; }
        }
        
        public double M {
            get { return Params["M"]; }
            set { Params["M"] = value; }
        }
        public double Ro {
            get { return Params["Ro"]; }
            set { Params["Ro"] = value; }
        }

        public double eps = 0.0001;
        public int n_points = 1000;

        public ShapeBase() {
            Params.Add("M",0.1);
            Params.Add("Ro",0.1);
        }

        protected virtual double GetVolume() {
            return Solver.GetCylIntegrX(F_ot_x,X0,X1,n_points);
        }
        double GetVolume(double v_ideal) {
            return GetVolume() - v_ideal;
        }

        public double FindParam(string key, bool updateParam = true) {
            if(key == "M") {
                return GetVolume() * Ro;
            }
            if(key == "Ro") {
                var v = GetVolume();
                return v < eps / 1000 ? 0d : M / v;
            }

            var v_ideal = M / Ro;
            var f_find = new Func<double,double>(t => {
                Params[key] = t;
                return GetVolume(v_ideal);
            });
            var a = Params[key];

            double answ = Find0Newton(f_find,a);
            if(!updateParam) {
                Params[key] = a;
            }
            
            return answ;

        }

        double Find0Newton(Func<double,double> f, double a) {
            var fa = f(a);
            if(Abs(fa) <= eps)
                return a;
            int i_count = 0;
            while(Abs(fa) > eps) {
                var f_shtr_a = GetF_shtr(f,a,fa);

                a -= Abs(f_shtr_a) > eps/100000 ? fa / f_shtr_a : Sign(f_shtr_a)*eps*10000;

                fa = f(a);
                if(++i_count > 10000) {
                    throw new Exception("Чет долго считаем");
                }
            }
            return a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double GetF_shtr(Func<double,double> f, double a, double fa) {
            var b = a + eps / 50;
            var fb = f(b);
            return (fb - fa) / (b - a);
        }

    }

    public class Cylynder : ShapeBase {
        public override double X0 {
            get {
                return 0d;
            }
        }

        public override double X1 {
            get {
                return H;
            }
        }

        protected override double GetVolume() {
            return PI * D * D * 0.25 * H;
        }

        public override double F_ot_x(double x) {
            return D / 2;
        }

        public override IList<DataPoint> GetPoints() {
            var res = new List<DataPoint>(2);
            res.Add(new DataPoint(0,D / 2));
            res.Add(new DataPoint(H,D / 2));
            return res;
        }



        public double H {
            get { return Params["H"]; }
            set { Params["H"] = value; }
        }
        public double D {
            get { return Params["D"]; }
            set { Params["D"] = value; }
        }

        public Cylynder() {
            Params.Add("H",0.1);
            Params.Add("D",0.1);
        }
    }

    public class Ellipse : ShapeBase {
        public override double X0 {
            get {
                return 0d;
            }
        }

        public override double X1 {
            get {
                return D1;
            }
        }

        protected override double GetVolume() {
            var a = D1 * 0.5;
            var b = D2 * 0.5;
            return 4 / 3 * PI * a * b * b;
        }

        public override double F_ot_x(double x) {
            if(x < X0 || x > X1)
                return 0;
            var a = D1 * 0.5;
            var b = D2 * 0.5;
            x -= a;
            return Sqrt((1 - x * x / (a * a)) * b * b);
        }

        public override IList<DataPoint> GetPoints() {
            
            int n = 200;
            var dx = (X1 - X0) / n;
            var x = 0d;
            var res = new List<DataPoint>(n+3);
            for(int i = 0; i <= n; i++) {
                res.Add(new DataPoint(x,F_ot_x(x)));
                x += dx;
            }
            return res;


        }


        public double D1 {
            get { return Params["D1"]; }
            set { Params["D1"] = value; }
        }
        public double D2 {
            get { return Params["D2"]; }
            set { Params["D2"] = value; }
        }

        public Ellipse() {
            Params.Add("D1",0.1);
            Params.Add("D2",0.1);
        }
    }

    public class Ellipse_Obrez : Ellipse {
        public override double X1 {
            get {
                return Min(D1,H);
            }
        }

        protected override double GetVolume() {
            return Solver.GetCylIntegrX(F_ot_x,X0,X1,n_points);
        }


        public double H {
            get { return Params["H"]; }
            set { Params["H"] = value; }
        }


        public Ellipse_Obrez() {
            Params.Add("H",0.1);
        }
    }

    public class ConeCone : ShapeBase {
        public override double X0 {
            get {
                return 0d;
            }
        }

        public override double X1 {
            get {
                return H1 + H2;
            }
        }

        protected override double GetVolume() {
            var d2 = D/2;
            return 1d / 3d * PI * d2 * d2 * (H1 + H2);
        }

        public override double F_ot_x(double x) {
            if(x <= 0)
                return 0d;
            var h1 = H1;
            var d = D;
            if(x <= h1)
                return 0.5 * D / H1 * x;
            var h2 = H2;
            if(x <= h1 + h2) {
                return 0.5 * d - 0.5 * d / h2 * (x - h1);
            }
            return 0;
        }

        public override IList<DataPoint> GetPoints() {
            var res = new List<DataPoint>(2);
            res.Add(new DataPoint(0,0));
            res.Add(new DataPoint(H1,D / 2));
            res.Add(new DataPoint(H1+H2,0));
            return res;
        }


        public double H1 {
            get { return Params["H1"]; }
            set { Params["H1"] = value; }
        }
        public double H2 {
            get { return Params["H2"]; }
            set { Params["H2"] = value; }
        }
        public double D {
            get { return Params["D"]; }
            set { Params["D"] = value; }
        }

        public ConeCone() {
            Params.Add("H1",0.1);
            Params.Add("H2",0.1);
            Params.Add("D",0.1);
        }
    }

    public class ShereShell : ShapeBase {
        public override double X0 {
            get {
                return 0d;
            }
        }

        public override double X1 {
            get {
                return H;
            }
        }

        protected override double GetVolume() {
            var Hl = H;
            var r = R;
            var hl = h;
            return PI * Hl * Hl * (r - 1 / 3 * h) - PI * (Hl - hl) * (Hl - hl) * (r - hl - 1 / 3 * (Hl - hl));
        }

        public override double F_ot_x(double x) {
            if(x < 0)
                return 0;
            var Hl = H;
            if(x > Hl)
                return 0;
            var r = R;
            var a = -(r - Hl);
            return Sqrt(r * r - (x - a) * (x - a));
            
        }

        public override IList<DataPoint> GetPoints() {
            int n = 200;
            var dx = (X1 - X0) / n;
            var x = 0d;
            var res = new List<DataPoint>(n + 3);
            for(int i = 0; i <= n; i++) {
                res.Add(new DataPoint(x,F_ot_x(x)));
                x += dx;
            }
            return res;
        }

        public override IList<DataPoint> GetPoints2() {
            var a = -(R - H);
            var r = R - h;
            

            int n = 200;
            var dx = (X1 - X0-h) / n;
            var x = 0d;
            var res = new List<DataPoint>(n + 3);
            for(int i = 0; i <= n; i++) {
                var y = Sqrt(r * r - (x - a) * (x - a));
                res.Add(new DataPoint(x,y));
                x += dx;
            }
            return res;

        }

        public double H {
            get { return Params["H"]; }
            set { Params["H"] = value; }
        }
        public double R {
            get { return Params["R"]; }
            set { Params["R"] = value; }
        }
        public double h {
            get { return Params["h"]; }
            set { Params["h"] = value; }
        }

        public ShereShell() {
            Params.Add("h",0.1);
            Params.Add("H",0.1);
            Params.Add("R",0.1);
        }
    }
}
