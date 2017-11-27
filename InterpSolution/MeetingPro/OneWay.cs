using System.Linq;

namespace MeetingPro {
    public class OneWay {
        public NDemVec Vec0 { get; set; }
        public MT_pos Pos0 { get; set; }

        public NDemVec Vec1 { get; set; }
        public MT_pos Pos1 { get; set; }

        public double Del1 { get; set; }
        public double Del2 { get; set; }
        public double Del_el { get; set; }

        public double Flaggy { get; set; } = 0; //0 -default, 1 - 4 train, 2 - normir

        public double XPos { get; set; } = 0; //-1.. +1
        public double YPos { get; set; } = 0;//-1.. +1

        public double[] ToArray() {
            var v0 = Vec0.ToVec();
            var vp0 = Pos0.ToVec();
            var vd0 = Vec1.ToVec();
            var vdp0 = Pos1.ToVec();
            int n = v0.Length * 2 + vp0.Length * 2 + 6;
            var res = new double[n];
            int i = 0;
            foreach (var d in v0.ToArray()) {
                res[i] = d;
                i++;
            }
            foreach (var d in vp0.ToArray()) {
                res[i] = d;
                i++;
            }
            foreach (var d in vd0.ToArray()) {
                res[i] = d;
                i++;
            }
            foreach (var d in vdp0.ToArray()) {
                res[i] = d;
                i++;
            }
            res[i] = Del1;
            i++;
            res[i] = Del2;
            i++;
            res[i] = Del_el;
            i++;
            res[i] = Flaggy;
            i++;
            res[i] = XPos;
            i++;
            res[i] = YPos;
            return res;
        }
        public void FromArray(double[] arr) {
            Vec0 = new NDemVec();
            Pos0 = new MT_pos();
            Vec1 = new NDemVec();
            Pos1 = new MT_pos();
            var v0 = Vec0.ToVec();
            var vp0 = Pos0.ToVec();
            var vd0 = Vec1.ToVec();
            var vdp0 = Pos1.ToVec();
            int i = 0;
            for (int j = 0; j < v0.Length; j++) {
                v0[j] = arr[i];
                i++;
            }
            for (int j = 0; j < vp0.Length; j++) {
                vp0[j] = arr[i];
                i++;
            }
            for (int j = 0; j < vd0.Length; j++) {
                vd0[j] = arr[i];
                i++;
            }
            for (int j = 0; j < vdp0.Length; j++) {
                vdp0[j] = arr[i];
                i++;
            }
            Vec0.FromVec(v0);
            Pos0.FromVec(vp0);
            Vec1.FromVec(vd0);
            Pos1.FromVec(vdp0);
            Del1 = arr[i];
            i++;
            Del2 = arr[i];
            i++;
            Del_el = arr[i];
            i++;
            Flaggy = arr[i];
            i++;
            XPos = arr[i];
            i++;
            YPos = arr[i];
        }
        public string[] GetHeaders() {
            return Vec0.GetHeader(nameof(Vec0)+"-")
                .Concat(Pos0.GetHeader(nameof(Pos0) +"-"))
                .Concat(Vec1.GetHeader(nameof(Vec1) + "-"))
                .Concat(Pos1.GetHeader(nameof(Pos1) + "-"))
                .Concat(new string[] { "Del1", "Del2", "Del_el", "Flaggy", "XPos", "YPos" })
                .ToArray();
        }
    }
}
