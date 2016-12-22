using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using System.Runtime.InteropServices;
using GeneticSharp.Domain.Randomizations;
using static System.Math;
using System.Runtime.CompilerServices;
using DoubleEnumGenetic;

namespace GeneticNik {
    public class FitnessNik  : IFitness {
        public IList<GeneDoubleRange> GInfo { get; set; }
        public IList<CritInfo> CrInfo { get; set; }

        public FitnessNik() {
             prep();
            //var tst = new GeneDoubleRange("Lcone",0.3,0.7);
            GInfo = new List<GeneDoubleRange>(5);
            GInfo.Add(new GeneDoubleRange("Lcone",0.3,0.7));
            GInfo.Add(new GeneDoubleRange("dout",0.07,0.125));
            GInfo.Add(new GeneDoubleRange("Lpiston",0.3,0.7));
            GInfo.Add(new GeneDoubleRange("m1",1,10));
            GInfo.Add(new GeneDoubleRange("m2",1,10));

            CrInfo = new List<CritInfo>(2);
            CrInfo.Add(new CritInfo("Vd",CritExtremum.maximize));
            CrInfo.Add(new CritInfo("pmax",CritExtremum.minimize));
        }

        public ChromosomeD GetNewChromosome() {
            return new ChromosomeD(GInfo, CrInfo);
        }

        public double Evaluate(IChromosome chromosome) {
            var c = chromosome as ChromosomeD;
            if(c == null)
                throw new Exception("хромосома не того типа");
            //if(c.GInfo != GInfo)
            //    throw new Exception("хромосома содериит другие гены");
            int n = GetThreadN();
            float l = (float)c["Lcone"],
                d = (float)c["dout"],
                lp = (float)c["Lpiston"],
                m1 = (float)c["m1"],
                m2 = (float)c["m2"],
                Vd = 0f,
                pmax = 0f;
            //string p = System.Reflection.Assembly.GetEntryAssembly().Location;
            //UniBallS0(ref l,ref d,ref lp,ref m1,ref m2,ref Vd,ref pmax);
            lock(_locks[n]) {
                delegs[n](ref l,ref d,ref lp,ref m1,ref m2,ref Vd,ref pmax);
            }
            //c["Vd"] = Vd;
            //c["pmax"] = pmax;
            if(float.IsNaN(Vd)) {
                return -1000d;
            } else if(Abs(Vd) > 300000) {
                return -1000d;
            }
            float penalty = 0.01f;
            float pmm = 10000E5f;
            return 0.5 * m2 * Vd * Vd - (pmax > pmm ? penalty * (pmax - pmm) : 0);
        }

        #region NikDlls
        delegate void UniBallSDeleg(
            ref float Lcone,
            ref float dout,
            ref float Lpiston,
            ref float m1,
            ref float m2,
            ref float Vd,
            ref float pmax);

        [DllImport("Nik0.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        public static extern void UniBallS0(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik1.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS1(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik2.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS2(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik3.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS3(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik4.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS4(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik5.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS5(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik6.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS6(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        [DllImport("Nik7.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        static extern void UniBallS7(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        const int Ncores = 8;
        static object[] _locks;
        static UniBallSDeleg[] delegs;
        static int GetThreadN() {
            return RandomizationProvider.Current.GetInt(0,Ncores);
        }




        static void prep() {
            _locks = new object[Ncores];
            for(int i = 0; i < Ncores; i++) {
                _locks[i] = new object();
            }
            delegs = new UniBallSDeleg[Ncores] {
                UniBallS0,
                UniBallS1,
                UniBallS2,
                UniBallS3,
                UniBallS4,
                UniBallS5,
                UniBallS6,
                UniBallS7
            };
        }
        #endregion
    }
}
