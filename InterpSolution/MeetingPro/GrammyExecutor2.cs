using Executor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {

    public class GrammyExecutor2 {
        public string datapath = "data.xml";
        public string saveFPath = @"saves\";
        object locker = new object();
        object locker2 = new object();
        public int done = 0;
        public int inqueue = 0;
        public string GetFilePath4Save(OneWay ow) {
            return saveFPath + $@"{ow.Id}_{ow.Vec0.Thetta}_{ow.Vec0.Alpha}_{ow.Vec0.Betta}_{ow.Vec0.V}_{ow.Vec0.Temperature}_{ow.Vec0.T}.csv";
        }
        public ThreadExecutor<OneWay, int> exc;


        public GrammyExecutor2(List<OneWay> plan) {
            var worker = new GranneWorker2(this);
            exc = new ThreadExecutor<OneWay, int>(worker) {
                WorkerCountMax = 9
            };
            exc.saveToDoneQueue = false;
            exc.AddToQueue(plan);
            exc.ExecutDoneNew += Exc_ExecutDoneNew;
            inqueue = plan.Count;
        }

        private void Exc_ExecutDoneNew(object sender, Res<OneWay, int> e) {
            lock (locker) {
                done++;
            }
        }

        public Action callback;

        public void Run() {
            exc.Run();
        }

        public void Stop() {
            exc.Stop();
        }
    }

    class GranneWorker2: IComputeTask<OneWay, int> {
        public GrammyExecutor2 owner;
        public GranneWorker2(GrammyExecutor2 owner) {
            this.owner = owner;
        }
        public int MapAction(OneWay OneWayP) {
            var glrv = new GramofonLarva(OneWayP.Vec0, OneWayP.Pos0);
            var sols = glrv
                .GetSols_short();

            var fp = owner.GetFilePath4Save(OneWayP);
            sols.SaveToFile(fp);
            return 1;
        }
    }
}
