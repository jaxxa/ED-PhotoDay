//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using Verse;

//namespace EnhancedDevelopment.Example.ED_PhotoDay
//{
//    class MapComponent_PhotoDay : Verse.MapComponent
//    {

//        public const int CHECK_INTERVAL_TICKS = 10000;

//        public MapComponent_PhotoDay(Map map) : base(map)
//        {

//        }


//        public override void MapComponentTick()
//        {
//            Log.Message("MapComponent_PhotoDay.MapComponentTick");

//            if (Find.TickManager.TicksGame % CHECK_INTERVAL_TICKS != 0)
//            {
//                return;
//            }

//            Letter _Letter = new Letter("Photo Day","Look your best, its Photo Day", LetterType.Good);

//            Find.LetterStack.ReceiveLetter(_Letter,"PhotoDay Letter");

//            if (!Find.TickManager.Paused)
//            {
//                Find.TickManager.TogglePaused();
//            }
//        }

//    }
//}
