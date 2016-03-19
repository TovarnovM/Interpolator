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
            XmlSerializer serial = new XmlSerializer(typeof(InterpXY));
            var sr = new StreamReader("InterpXY.xml");
            var interpol = (InterpXY)serial.Deserialize(sr);


            Console.ReadLine();
        }
    }
}
