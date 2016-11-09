using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPHmain {
    public interface IParticle2D {
        double X { get; set; }
        double Y { get; set; }

    }

    public class Particle2D: Position2D, IParticle2D {

    }
}
