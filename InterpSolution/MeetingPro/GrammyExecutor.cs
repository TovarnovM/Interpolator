using Executor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class GrammyExecutor {
        public string datapath = "data.xml";
        public string saveFPath = @"saves\";
        public int id { get; }
        object locker = new object();
        object locker2 = new object();
        public int id_loc_gener = 0;
        public int maxGeneration = 6;
        public int done = 0;
        public int inqueue = 1;
        public string GetFilePath4Save(GramofonLarva gram) {
            int id_loc = 0;
            lock (locker) {
                id_loc = id_loc_gener;
                id_loc_gener++;
            }
            return saveFPath + $@"{id}_{gram.generation}_{gram.nDemVec0.XPos}_{gram.nDemVec0.YPos}_{id_loc}.csv";
        }
        public ThreadExecutor<GramofonLarva, List<GramofonLarva>> exc;

        public GramofonLarva Init_Gl { get; }

        public GrammyExecutor(GramofonLarva init_gl, int id) {
            var worker = new GranneWorker(this);
            exc = new ThreadExecutor<GramofonLarva, List<GramofonLarva>>(worker) {
                WorkerCountMax = 9
            };
            exc.saveToDoneQueue = false;
            exc.ExecutDoneNew += Exc_ExecutDoneNew;
            exc.AddToQueue(init_gl);
            Init_Gl = init_gl;
            this.id = id;
        }

        private void Exc_ExecutDoneNew(object sender, Res<GramofonLarva, List<GramofonLarva>> e) {
            lock (locker2) {
                if (e.Params.generation <= maxGeneration)
                    foreach (var gl in e.Result) {
                        exc.AddToQueue(gl);
                        inqueue++;
                    }
                done++;
                inqueue--;
                
            }
            callback?.Invoke();
        }

        public Action callback;

        public void Run() {
            exc.Run();
        }

        public void Stop() {
            exc.Stop();
        }
    }

    class GranneWorker: IComputeTask<GramofonLarva, List<GramofonLarva>> {
        public GrammyExecutor owner;
        public GranneWorker(GrammyExecutor owner) {
            this.owner = owner;
        }
        public List<GramofonLarva> MapAction(GramofonLarva taskData) {
            var sols = taskData
                .GetSols();
            var res = sols
                .AddCoord()
                .Uniquest()
                .Select(tp => {
                    var gl = new GramofonLarva(tp.ow) {
                        generation = taskData.generation + 1,                     
                    };
                    gl.saveName = owner.GetFilePath4Save(gl);
                    return gl;
                })
                .ToList();
            if (taskData.saveName == "")
                taskData.saveName = owner.GetFilePath4Save(taskData);
            sols.SaveToFile(taskData.saveName);
            return res;
        }
    }
}
