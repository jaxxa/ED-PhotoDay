using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EnhancedDevelopment.Example.ED_PhotoDay
{
    class ModBase_PhotoDay : ModBase
    {
        public override string ModIdentifier {
            get { return "ED-PhotoDay"; }
        }

        
        public const int CHECK_INTERVAL_TICKS = 10000;

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if (Find.TickManager.TicksGame % CHECK_INTERVAL_TICKS != 0)
            {
                return;
            }

            Letter _Letter = new Letter("Photo Day - H", "Look your best, its Photo Day - H", LetterType.Good);

            Find.LetterStack.ReceiveLetter(_Letter, "PhotoDay Letter");

            if (!Find.TickManager.Paused)
            {
                Find.TickManager.TogglePaused();
            }

        }


    }
}
 