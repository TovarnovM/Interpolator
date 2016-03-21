using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Interpolator
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var intD = new InterpDouble(77);
            XmlSerializer serial = new XmlSerializer(typeof(InterpDouble));
            StreamWriter sr = new StreamWriter("InterpDouble.xml");
            serial.Serialize(sr, intD);
            sr.Close(); 
            */

            /*
            var interpol = new InterpXY();
            interpol.Add(-1.2, 1);
            interpol.Add(-4.2, 2);
            interpol.Add(2, 3);
            interpol.Add(4, 4);
            interpol.Add(-10, 5);
            interpol.Add(11, 6);
            interpol.Add(7, 7);

            var lst = interpol.Data;

            XmlSerializer serial = new XmlSerializer(typeof(InterpXY));//SerializableGenerics.SerializableSortedList<double,InterpDouble>));
            StreamWriter sr = new StreamWriter("InterpXY.xml");
            serial.Serialize(sr, interpol);
            sr.Close(); 
            */
            /*
            XmlSerializer serial = new XmlSerializer(typeof(InterpXY));
            var sr = new StreamReader("InterpXY.xml");
            var interpol = (InterpXY)serial.Deserialize(sr);
            */

            var interp2D = new Interp2D();
            for (int i = 1; i < 5; i++)
            {
                var interpol = new InterpXY();
                interpol.Add(-1.2*i, 1);
                interpol.Add(-4.2, 2 * i);
                interpol.Add(2 * i, 3);
                interpol.Add(4, 4 * i);
                interpol.Add(-10 * i, 5);
                interpol.Add(11, 6 * i);
                interpol.Add(7 * i, 7);
                interp2D.AddElement((double)i, interpol);
            }
            XmlSerializer serial = new XmlSerializer(typeof(Interp2D));
            var sw = new StreamWriter("Interp2D.xml");
            serial.Serialize(sw, interp2D);
            sw.Close();

            var sr = new StreamReader("Interp2D.xml");
            var interp2D_des = (Interp2D)serial.Deserialize(sr);

            var interp2D_clone = interp2D_des.CopyMe();

            var ww = new int[7, 4];
            Console.WriteLine(ww.GetLength(0));

            var interp2D2 = new Interp2D();
            interp2D2.ImportDataFromMatrix(new double[4, 5]
                {   { 0, 0, 20, 30, 40},

                    { -1,0, 3, 0, -1},
                    { 0, -1,-2,-3,-3},
                    { 2, 1, 2, 3, 1}});
            var ee = interp2D2.GetV(-1, 10);

            Console.ReadLine();
        }
    }
}
