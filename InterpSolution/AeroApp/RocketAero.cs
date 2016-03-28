using System;
using System.Collections.Generic;
using System.Text;
using Interpolator;
using AeroApp.Properties;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Globalization;
using System.Collections;

using System.Xml.Serialization;

namespace RocketAero
{
    public class AeroGraphs
    {

    }
    class RocketAero1
    {
        public void foo()
        {
            var interp3D = Interp3D.LoadFromXmlString(Resources._3_5_3D);
            var resourses = Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            var my3D = from DictionaryEntry res in resourses
                       orderby res.Key.ToString()
                       where res.Key.ToString().EndsWith("3D")
                       select res.Key.ToString();
            foreach (var item in my3D)
            {

            }
        }
    }
}
