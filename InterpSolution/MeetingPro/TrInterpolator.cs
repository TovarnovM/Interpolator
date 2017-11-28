using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class TrInterpolator {
        public Vector2D va, vb, vc;
        public Vector fa, fb, fc;
        public TrInterpolator(Vector2D va, Vector2D vb, Vector2D vc, Vector fa, Vector fb, Vector fc) {
            this.va = va;
            this.vb = vb;
            this.vc = vc;
            this.fa = fa;
            this.fb = fb;
            this.fc = fc;

        }
        public Vector Interp(Vector2D p) {
            double alf =  ((p.X - vb.X) * (vc.Y - vb.Y) - (vc.X - vb.X) * (p.Y - vb.Y)) / ((vc.Y - vb.Y) * (va.X - vb.X) -
(vc.X - vb.X) * (va.Y - vb.Y));
            double bet = ((p.Y - vb.Y) * (va.X - vb.X) - (p.X - vb.X) * (va.Y - vb.Y)) / ((vc.Y - vb.Y) * (va.X - vb.X) -
(vc.X - vb.X) * (va.Y - vb.Y));
            return alf * (fa - fb) + bet * (fc - fb) + fb;
        }
    }
}
