using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.Collections.Generic;

namespace Clicker.GameSystem
{ 
    class ArchivController
    {
        private Dictionary<Archivments, int> archivProg;
        private Dictionary<Archivments, XXLNum> archivMax;

        private List<Archivka> doneArchiv;
        private int progressSum;
        public bool IsDoneArchiv() => doneArchiv.Count != 0;
        public Archivka GetDonArchiv() => doneArchiv[0];
        public ArchivController(int[] progress)
        {
            archivProg = new Dictionary<Archivments, int>();
            archivMax = new Dictionary<Archivments, XXLNum>();
            doneArchiv = new List<Archivka>();
            progressSum = 0;
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
            {
                progressSum += progress[(int)archiv];
                archivProg.Add(archiv, progress[(int)archiv]);
                archivMax[archiv] = JsonParser.getArchivValue(ArchivmentSystem.GetCategoryArchivName(archiv), progress[(int)archiv]);
            }
        }
        //complete archiv and return previous progress
        public void archivmentDone(Archivka arch)
        {
            progressSum++;
            archivProg[arch.archType]++;
            doneArchiv.Remove(arch);
        }   
    }
}
