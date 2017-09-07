using Interpolator;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class Mis: ScnObjDummy {
        public MaterialObjectNewton body;
        Force Pforce;
        public MisOptions opts;
        public Mis(MisOptions opts) :base(opts.name) {
            this.opts = opts;
        }
        public void Init() {
            body = new MaterialObjectNewton("body");
            Pforce = Force.GetForceCentered(0, new Vector3D(-1, 0, 0), body);
            body.AddForce(Pforce);
            
        }

        public void SyncAct(double t) {
            var deltam_rd = opts.graphs["deltam_rd"].GetV(opts.Temperature, t);
            var deltam_md = opts.graphs["deltam_md"].GetV(opts.Temperature, t);
            var x_m = opts.graphs["x_m"].GetV(deltam_rd, deltam_md);
            var m = opts.graphs["m"].GetV(deltam_rd, deltam_md);
            var i_x = opts.graphs["i_x"].GetV(deltam_rd, deltam_md);
            var i_yz = opts.graphs["i_yz"].GetV(deltam_rd, deltam_md);

            var r_rd = 9.80665 * opts.graphs["r_rd"].GetV(opts.Temperature, t);
            var r_md = 9.80665 * opts.graphs["r_md"].GetV(opts.Temperature, t);
            var alpha = GetAlpha();
            var mach = GetMach();
            var c_x = opts.graphs["c_x"].GetV(alpha, mach);
            var c_r_x
        }

        public double GetAlpha() {
            throw new NotImplementedException();
        }

        public double GetMach() {
            throw new NotImplementedException();
        }
    }

    public class MisOptions {
        public Graphs graphs;
        public string name;
        public int id;
        public double Temperature;
    }

    
}
