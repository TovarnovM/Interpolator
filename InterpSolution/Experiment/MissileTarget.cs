using Interpolator;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Experiment {
    public class MissileTarget: Position3D {
        public double Radius { get; set; } = 1d;
        public IPosition3D Vel { get; set; }
        public MissileTarget(Vector3D initPos, IPosition3D vel):base(initPos, "Trg") {
            Vel = vel;
            Vel.Name = "Vel";
            AddChild(Vel);
            AddDiffVect(Vel);
        }
        public static Vector3D MethNavCleanChase(double t,MissileTarget trg,IPosition3D missPos,double missVel,object otherStuff) {
            //линия визирования 
            var visLine = trg.Vec3D - missPos.Vec3D;
            visLine.Normalize();
            return visLine * missVel;
        }

        public static Vector3D MethNavParalelSblizj(double t,MissileTarget trg,IPosition3D missPos,double missVel,object otherStuff) {
            //линия визирования
            var visLine = trg.Vec3D - missPos.Vec3D;
            visLine.Normalize();
            //нормаль к плоскости линии визирования и вектором скорости цели
            var norm = visLine & trg.Vel.Vec3D;
            double l = norm.GetLength();
            if(l < 1E-17) {
                // если вектора параллельны, то чистая погоня
                visLine.Normalize();
                return visLine * missVel;
            }

            norm /=l;
            //единичный вектор, лежащий в плоскости линии визирования и вектором скорости цели, направленный 
            // перпендикулярно к линии визирования, и направленный по ходу вектора скорости цели
            var norm2 = (norm & visLine).Norm;
            //тангенциальная составляющая скорости цели, относитально линии визирования    
            var vPoNapravl = trg.Vel.Vec3D * norm2;
            //квадрат скорости ракеты
            var v2 = missVel* missVel;
            //квадрат тангенциальной составляющей скорости цели, относитально линии визирования 
            var a2 = vPoNapravl * vPoNapravl;
            if(v2 - a2 < 0) {
                //если мы не успеваем за поворотао цели, тьо чистая погоня
                visLine.Normalize();
                return visLine * missVel;
            }
            //проекция скорости ракеты на линию визирования
            var vPoVisLine = Sqrt(v2 - a2);


            return norm2* vPoNapravl + visLine.Norm*vPoVisLine;
        }

        public static Vector3D MethNav3p(double t,MissileTarget trg,IPosition3D missPos,double missVel,object otherStuff) {
            //А вот тут хитро: PROP - делит отрезок, который лежит на линии визирования НАБЛ.ПУНКТ-ЦЕЛЬ.
            //Одна точка лежит на цели, вторая точка является точкой пересчечения  линии визирования НАБЛ.ПУНКТ-ЦЕЛЬ с нормалью к ней же(perfectPoint)
            //(нормаль проходит через фактическое местоположение ракеты, которое может быть расположено не на линии визирования НАБЛ.ПУНКТ -> ЦЕЛЬ)
            //Отрезок этот делится в соотношении PROP - (1-PROP) точкой(vel2TrgPoint), в которую целится ракета 
            //(не полностью, просто по вектору РАКЕТА -> vel2TrgPoint направлена одна из составляущей скорости ракеты )
            const double PROP = 0.3d;
            //координаты набл пункта (одна из тех самых трех точек "цель" "ракета" >>"НАБЛ. ПУНКТ"<<)
            var nablPunkt = ((otherStuff==null)?Vector3D.Zero:(Vector3D)otherStuff);
            //Линия визирования НАБЛ.ПУНКТ -> ЦЕЛЬ
            var visLine = trg.Vec3D - nablPunkt;
            //Пропорция в которой делит линию визирования НАБЛ.ПУНКТ -> ЦЕЛЬ точка пересчечения линии визирования НАБЛ.ПУНКТ-ЦЕЛЬ с нормалью к ней же(perfectPoint)
            //(нормаль проходит через фактическое местоположение ракеты, которое может быть расположено не на линии визирования НАБЛ.ПУНКТ -> ЦЕЛЬ)
            var perfectProportion = (visLine.Norm * (missPos.Vec3D-nablPunkt))/ visLine.GetLength();
            //точка пересчечения линии визирования НАБЛ.ПУНКТ-ЦЕЛЬ с нормалью к ней же(perfectPoint)
            //(нормаль проходит через фактическое местоположение ракеты, которое может быть расположено не на линии визирования НАБЛ.ПУНКТ -> ЦЕЛЬ)
            var perfectPoint = perfectProportion * visLine;
            //единичный вектор, лежащий в плоскости линии визирования НАБЛ.ПУНКТ -> ЦЕЛЬ и вектором скорости цели, перпендикулярный линии визирования
            //и направленный по ходу вектора скорости цели, если линия виз. и скорость цели параллельны, то n1=(0,0,0); 
            var n1 = ((visLine & trg.Vel.Vec3D) & visLine).Norm;
            //тангенциальная состаавляющая скорости цели, относитально НАБЛ.ПУНКТ 
            var vTrgPerpend = n1 * (n1 * trg.Vel.Vec3D);
            //модуль тангенциальной составляющей скорости ракеты, относитально НАБЛ.ПУНКТ
            var vMisPerpend = vTrgPerpend * perfectProportion;
            //точка в которую целится ракета 
            //(не полностью, просто по вектору РАКЕТА -> vel2TrgPoint направлена одна из составляущей скорости ракеты )
            //делит отрезок perfectPoint -> ЦЕЛЬ в соотношении PROP - (1-PROP)
            var vel2TrgPoint = Vector3D.Lerp(PROP,0d,perfectPoint,1d,visLine);
            //направление одной из составляущей скорости ракеты 
            var vel2Dir = (vel2TrgPoint - missPos.Vec3D).Norm;


            //далее решается задача нахождения вектора скорости ракеты, при условии, 
            //что известна ее тангенциальная составляющая, известен модуль скорости, и известно направление его второй составляющей...... (РАКЕТА -> vel2TrgPoint)
            var vMisPerp2 = vMisPerpend.GetLengthSquared();
            var vMis2 = missVel * missVel;
            if(vMisPerp2 > vMis2)
                return MethNavCleanChase(t,trg,missPos,missVel,otherStuff);
            var b = vel2Dir * vMisPerpend;
            var b2 = b*b;
            var dscr = 4 * b + 4 * (vMis2 - vMisPerp2);
            var s1 = 0.5 * (-b + Sqrt(dscr));

            //ОТВЕТ
            var res = vMisPerpend + s1 * vel2Dir;
            if(Abs(res.GetLength() - missVel) < 1E-15)
                throw new Exception("Чет не работает алгоритм по корректировке по 3 точкам(");
            return res;
        }
    }

    public delegate Vector3D MethNav(double t,MissileTarget trg, IPosition3D missPos,double missVel, object otherStuff);

}
