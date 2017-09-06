using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public static class DataFactory {
        public static Interp2D GetA3() {
            var res = new Interp2D();
            res.Title = "А.3";
            res.ImportDataFromMatrix(new double[,]
                {   {   0,      0,      4.06,   8.15,   12.24,  16.05,  18.72,  19.17   },
                    {   0,      1102.9, 1091.9, 1084.4, 1080.7, 1080.9, 1083.8, 1084.5  },
                    {   2.66,   1089.7, 1077.9, 1069.5, 1065.0, 1064.6, 1067.0, 1067.7  },
                    {   4.71,   1079.4, 1066.8, 1057.8, 1052.6, 1051.6, 1053.6, 1054.2  },
                    {   6.69,   1068.9, 1055.7, 1045.9, 1040.0, 1038.4, 1040.0, 1040.6  },
                    {   8.35,   1059.8, 1045.9, 1035.5, 1029.0, 1026.8, 1028.1, 1028.6  },
                    {   8.75,   1057.6, 1043.5, 1033.0, 1026.3, 1023.9, 1025.2, 1025.6  }                    
                });
            res.SynchArrays();
            return res;
        }
        public static Interp2D GetA4() {
            var res = new Interp2D();
            res.Title = "А.4";
            res.ImportDataFromMatrix(new double[,]
                {   {   0,      0,      4.06,   8.15,   12.24,  16.05,  18.72,  19.17   },
                    {   0,      104.03, 99.928, 95.838, 91.748, 87.938, 85.268, 84.818  },
                    {   2.66,   1089.7, 1077.9, 1069.5, 1065.0, 1064.6, 1067.0, 1067.7  },
                    {   4.71,   1079.4, 1066.8, 1057.8, 1052.6, 1051.6, 1053.6, 1054.2  },
                    {   6.69,   1068.9, 1055.7, 1045.9, 1040.0, 1038.4, 1040.0, 1040.6  },
                    {   8.35,   1059.8, 1045.9, 1035.5, 1029.0, 1026.8, 1028.1, 1028.6  },
                    {   8.75,   1057.6, 1043.5, 1033.0, 1026.3, 1023.9, 1025.2, 1025.6  }
                });
            res.SynchArrays();
            return res;
        }

    }
}
