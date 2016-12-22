using DoubleEnumGenetic;
using GeneticSharp.Domain.Populations;
using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveODE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticNik {
    public class VMgenetic {
        ScatterSeries ChromosParams;
        LineSeries FitnessAverSer;
        AreaSeries FitnessMinMaxSer;

        public VMPropRx<PlotModel,Generation> PM_params_Rx { get; private set; }
        public VMPropRx<PlotModel,Population> PM_fitness_Rx { get; private set; }
        public string paramX { get; set; }
        public string paramY { get; set; }
        Generation _curr4Draw;
        private LinearColorAxis colorAxis;

        public VMgenetic() {
            _curr4Draw = null;

            PM_params_Rx = new VMPropRx<PlotModel,Generation>(
                () => {
                    var modelP = GetNewModel("params");

                    colorAxis = new LinearColorAxis {
                        Position = AxisPosition.Right,
                        Palette = OxyPalettes.Jet(200),
                        Minimum = 0,
                        Maximum = 1
                    };

                    ChromosParams = new ScatterSeries() {
                        Title = "Chromosomes",
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 2,
                        ColorAxisKey = colorAxis.Key
                    };
                    modelP.Series.Add(ChromosParams);

                    modelP.Axes.Add(colorAxis);

                    return modelP;
                },
                (g,pm) => {
                    DrawParams(pm,g);
                    return pm;
                });



            PM_fitness_Rx = new VMPropRx<PlotModel,Population>(
                () => {
                    var Model1 = GetNewModel("fitness","generations","p,Ro,V");
                    FitnessAverSer = new LineSeries() {
                        Title = "Fitness Aver",
                        Color = OxyColors.Blue,
                        StrokeThickness = 2
                        

                    };
                    Model1.Series.Add(FitnessAverSer);

                    FitnessMinMaxSer = new AreaSeries();
                    Model1.Series.Add(FitnessMinMaxSer);
                    return Model1;
                },
                (pop,pm) => {
                    DrawFitness(pop,pm);

                    return pm;
                });
        }

        private void DrawParams(PlotModel pm,Generation g) {
            ChromosParams.Points.Clear();
            foreach(var c in g.Chromosomes.Cast<ChromosomeD>()) {

                var dp = new ScatterPoint(c["Lcone"],c["Lpiston"],value: c.Fitness ?? 0);
                ChromosParams.Points.Add(dp);
            }
            colorAxis.Maximum = g.Chromosomes.Cast<ChromosomeD>().Max(c => c.Fitness ?? 0);
            pm.InvalidatePlot(true);
        }

        private void DrawFitness(Population pop,PlotModel pm) {
            throw new NotImplementedException();
        }

        public void Draw(double t,PlotModel pm) {
            //pm.Axes.Remove(colorAxis);
            //Ro.Points.Clear();
            //Ro.MarkerFill = OxyColors.DarkOrange;
            //V.Points.Clear();
            //V.MarkerFill = OxyColors.Red;
            //P.Points.Clear();
            //P.MarkerFill = OxyColors.Green;
            //E.Points.Clear();
            //E.MarkerFill = OxyColors.Blue;
            //colorSer2D.Points.Clear();
            //foreach(var p in _curr4Draw.AllParticles.Cast<IGasParticleVer3>()) {
            //    Ro.Points.Add(new ScatterPoint(p.X,p.Ro / Ro0,value: p.Ro / Ro0));
            //    V.Points.Add(new ScatterPoint(p.X,p.VelVec2D.GetLength() / p.C,value: p.VelVec2D.GetLength() / p.C));
            //    P.Points.Add(new ScatterPoint(p.X,p.P / P0,value: p.P / P0));
            //    E.Points.Add(new ScatterPoint(p.X,p.E / E0,value: p.E / E0));
            //}
            //pm.Title = $"{t:0.##########} s,  RoMax = {_curr4Draw.Particles.Cast<IGasParticleVer3>().Max(p => p.Ro):0.###},  Pmax = {_curr4Draw.Particles.Cast<IGasParticleVer3>().Max(p => p.P):0.###}";
            //pm.InvalidatePlot(true);
        }


        public PlotModel GetNewModel(string title = "",string xname = "",string yname = "") {
            var m = new PlotModel { Title = title };
            var linearAxis1 = new LinearAxis();
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.Title = xname;
            m.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.Title = yname;
            m.Axes.Add(linearAxis2);

            return m;
        }
    }
}
