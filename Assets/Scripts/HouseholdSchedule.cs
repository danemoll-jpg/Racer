using System;
namespace Racer
{
    // Saved independently for each course. Only a new race consumes a visit.
    [Serializable]
    public sealed class HouseholdSchedule
    {
        public int[] dan = Array.Empty<int>(), friend = Array.Empty<int>();
        public int lastDan = -1, lastFriend = -1;
        public void Next(Random random, out int scene, out bool smokers)
        {
            scene = Take(ref dan, 3, lastDan, random);
            int visit = Take(ref friend, 2, lastFriend, random);
            lastDan = scene; lastFriend = visit; smokers = visit == 1;
        }
        static int Take(ref int[] bag, int count, int previous, Random random)
        {
            if (bag == null || bag.Length == 0 || Array.Exists(bag, x => x < 0 || x >= count))
            {
                bag = new int[count];
                for (int i=0;i<count;i++) bag[i]=i;
                for (int i=count-1;i>0;i--) { int j=random.Next(i+1); (bag[i],bag[j])=(bag[j],bag[i]); }
                if (bag[0]==previous) { int j=random.Next(1,count); (bag[0],bag[j])=(bag[j],bag[0]); }
            }
            int result=bag[0]; var rest=new int[bag.Length-1]; Array.Copy(bag,1,rest,0,rest.Length); bag=rest;
            return result;
        }
    }
}
