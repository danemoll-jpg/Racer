using System;
using System.Linq;

namespace Racer
{
    // Read-only presentation: existing record writers, tie ordering and timing stay authoritative.
    public sealed class FinishPresentation
    {
        public sealed class Result
        {
            public string headline,comparison,id;
            public double time;
            public int rank;
            public string Text=>headline+"\n"+(rank>0?"#"+rank+"  ":"")+RaceHud.FormatTime(time)+"\n"+comparison;
        }
        double priorLapLeader,priorRaceLeader,priorLapPb,priorRacePb;
        public Result Lap {get;private set;}
        public Result Race {get;private set;}
        public void Begin(RecordBoards boards,string category,RacerSave.Records best)
        {
            Lap=Race=null;priorLapLeader=boards.Board(category,false).FirstOrDefault()?.seconds??0;priorRaceLeader=boards.Board(category,true).FirstOrDefault()?.seconds??0;priorLapPb=best.lap;priorRacePb=best.race;
        }
        public void Finish(RecordBoards boards,string category,string attempt,RaceProgress progress,double clock)
        {
            if(!progress.Finished||progress.LapTimes.Count==0)return;
            int best=Enumerable.Range(0,progress.LapTimes.Count).OrderBy(i=>progress.LapTimes[i]).First();string lapId=attempt+"/lap/"+(best+1),raceId=attempt+"/race";
            Lap=Evaluate(progress.LapTimes[best],Array.FindIndex(boards.Board(category,false).ToArray(),e=>e.id==lapId)+1,priorLapLeader,priorLapPb,lapId);
            Race=Evaluate(progress.AdjustedTime(clock),Array.FindIndex(boards.Board(category,true).ToArray(),e=>e.id==raceId)+1,priorRaceLeader,priorRacePb,raceId);
        }
        public static Result Evaluate(double time,int rank,double leader,double pb,string id="")
        {
            bool record=leader>0&&time<leader;
            string headline=record?"NEW COURSE RECORD!":leader<=0&&rank>0?"FIRST COURSE RECORD!":leader>0&&time==leader?"Matched Best":rank>=2&&rank<=3?"PODIUM TIME!":rank>=1&&rank<=10?"TOP 10 TIME!":pb>0&&time<pb?"NEW PERSONAL BEST!":"Finished";
            double prior=record?leader:pb;string comparison=prior<=0?"First recorded time.":(record?"Previous Course Best ":"Your Previous Best ")+RaceHud.FormatTime(prior);
            if(prior>time){double delta=prior-time;comparison+="\n"+(delta<.001?"Less than 0.001s faster.":delta.ToString("0.000")+"s faster.");}
            return new Result{headline=headline,comparison=comparison,time=time,rank=rank,id=id};
        }
        public string Summary=>Lap==null?"":"LAP: "+Lap.headline+" · "+RaceHud.FormatTime(Lap.time)+"\nRACE: "+Race.headline+" · "+RaceHud.FormatTime(Race.time);
    }
}
