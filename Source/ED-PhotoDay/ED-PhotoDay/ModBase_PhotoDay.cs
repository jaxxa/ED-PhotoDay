using HugsLib;
using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EnhancedDevelopment.Example.ED_PhotoDay
{
    class ModBase_PhotoDay : ModBase
    {

        private int m_NextRunTicks;

        List<int> m_ScreenshotDays;
        List<int> m_ScreenshotHours;
        //float m_Longitude;

        public override string ModIdentifier
        {
            get { return "ED-PhotoDay"; }
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if(!this.SettingModRunning) { return; }

            int _TicksNowAbs = Find.TickManager.TicksAbs;

            //Check SettingCheckIntervalTicks
            //if (Find.TickManager.TicksAbs % this.SettingCheckIntervalTicks != 0)
            //{
            //    return;
            //}
            //Log.Message("Current Tick: " + currentTick.ToString() + "Abs: " + Find.TickManager.TicksAbs.ToString());

            this.InitiliseLongitudeIfRequired();
            
            //On First Run Calculate the Time but dont Execute Operations.
            if (this.m_NextRunTicks == 0)
            {
                this.CalculateNextRunTick(_TicksNowAbs);
                return;
            }

            if (this.m_NextRunTicks < _TicksNowAbs)
            {
                this.ExecureOperation(_TicksNowAbs);
                this.CalculateNextRunTick(_TicksNowAbs);
            }

        }

        private void CalculateNextRunTick(int ticksNowAbs)
        {
            //If Settings have not been parsed do that now.
            if (this.m_ScreenshotHours == null || this.m_ScreenshotDays == null)
            {
                this.CalculateDaysAndHoursToRunOn();
            }

            int _CurrentHour = GenDate.HourOfDay(ticksNowAbs, this.SettingTimeZoneLongitude);
            int _CurrentDay = GenDate.DayOfSeason(ticksNowAbs, this.SettingTimeZoneLongitude) + 1; //Convert from 0-14 to 1-15 as seen by user.

            Log.Message("CalculateNextRunTick Day:" + _CurrentDay.ToString() + " Hour " + _CurrentHour.ToString());

            int _NextHour = this.m_ScreenshotHours.Where(item => item > _CurrentHour).FirstOrDefault();
            int _NextDay = this.m_ScreenshotDays.Where(item => item > _CurrentDay).FirstOrDefault();
            Log.Message("Next Hour is: " + _NextHour.ToString() + " Next Day: " + _NextDay.ToString());

            int _TicksThroughDay = (int)((ticksNowAbs + this.LocalTicksOffsetFromLongitude((int)this.SettingTimeZoneLongitude)) % 60000L);
            int _TicksAtStartOfDay = ticksNowAbs - _TicksThroughDay;        
            Log.Message("_TicksAtStartOfDay: " + _TicksAtStartOfDay.ToString() + " _TicksThroughDay: " + _TicksThroughDay.ToString());

            int _DayOffset = 0;
            if (int.Equals(0,_NextHour))
            {
                _NextHour = this.m_ScreenshotHours.FirstOrDefault();
                _DayOffset = _NextDay - _CurrentDay;
                Log.Message("Day offset: " + _DayOffset.ToString());
            }

            this.m_NextRunTicks = _TicksAtStartOfDay + _NextHour * GenDate.TicksPerHour + _DayOffset * GenDate.TicksPerDay;
            Log.Message("Running at: " + this.m_NextRunTicks);

        }

        private long LocalTicksOffsetFromLongitude(float longitude)
        {
            return (long)GenDate.TimeZoneAt(longitude) * 2500L;
        }
        
        private void CalculateDaysAndHoursToRunOn()
        {
            Log.Message("Parsing Days: " + SettingScreenshotDays + " Hours: " + SettingScreenshotHours);
            this.m_ScreenshotDays = SettingScreenshotDays.ToString().Split(',').Select(int.Parse).ToList();
            this.m_ScreenshotHours = SettingScreenshotHours.ToString().Split(',').Select(int.Parse).ToList();
            Log.Message("Days" + m_ScreenshotDays.Count.ToString() + " Hours: " + m_ScreenshotHours.Count.ToString());
        }

        private void InitiliseLongitudeIfRequired()
        {
            if (this.SettingRecalculateLongitudeNext)
            {
                this.SettingTimeZoneLongitude.Value = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile).x;

                this.SettingRecalculateLongitudeNext.Value = false;
                this.m_NextRunTicks = 0; //Reset to 0 so it will be recalculated.
                HugsLibController.SettingsManager.SaveChanges();

            }
        }
        
        private void ExecureOperation(int ticksNowAbs)
        {
            if (this.SettingAutoPause)
            {
                if (!Find.TickManager.Paused)
                {
                    Find.TickManager.TogglePaused();
                }
            }

            if (this.SettingDisplayMessage)
            {
                Letter _Letter = new Letter(this.SettingMessageLabel, this.SettingMessageContent, LetterType.Good);
                Find.LetterStack.ReceiveLetter(_Letter, "PhotoDay Letter");
            }

            if (this.SettingAutoScreenshot)
            {
                string _ScreenshotFolderPath = GenFilePaths.ScreenshotFolderPath;

                string _FullFilePath = _ScreenshotFolderPath;
                // _FullFilePath = _FullFilePath + (object)Path.DirectorySeparatorChar + GenDate.DaysPassedFloat.ToString() + ".jpg";
                _FullFilePath = _FullFilePath + (object)Path.DirectorySeparatorChar +
                                GenDate.Year(ticksNowAbs, this.SettingTimeZoneLongitude) + "-" +
                                GenDate.Season(ticksNowAbs, this.SettingTimeZoneLongitude) + "-" +
                                (GenDate.DayOfSeason(ticksNowAbs, this.SettingTimeZoneLongitude)+1 ).ToString().PadLeft(2,'0') + "-" +
                                GenDate.HourOfDay(ticksNowAbs, this.SettingTimeZoneLongitude).ToString().PadLeft(2,'0') + ".jpg";

                Log.Message(_FullFilePath);
                Application.CaptureScreenshot(_FullFilePath);
            }

        }
        
        #region Settings

        private SettingHandle<bool> SettingModRunning;
        private SettingHandle<bool> SettingAutoScreenshot;
        private SettingHandle<bool> SettingAutoPause;
        private SettingHandle<bool> SettingDisplayMessage;

        private SettingHandle<bool> SettingRecalculateLongitudeNext;
        private SettingHandle<float> SettingTimeZoneLongitude;
        //private SettingHandle<float> SettingCheckIntervalTicks;

        private SettingHandle<string> SettingScreenshotHours;
        private SettingHandle<string> SettingScreenshotDays;

        private SettingHandle<string> SettingMessageLabel;
        private SettingHandle<string> SettingMessageContent;

        public override void DefsLoaded()
        {
            this.SettingModRunning = Settings.GetHandle<bool>("ModRunning", "Mod Running", "True for the mod to do anything.", false);
            this.SettingAutoScreenshot = Settings.GetHandle<bool>("AutoScreenshot", "Auto Screenshot", "Take a Screenshot and dont pause or show a message.", false);
            this.SettingAutoPause = Settings.GetHandle<bool>("AutoPause", "Auto Pause", "Pause the game.", true);
            this.SettingDisplayMessage = Settings.GetHandle<bool>("DisplayMessage", "Display Message", "Show a message.", true);


            this.SettingRecalculateLongitudeNext = Settings.GetHandle<bool>("RecalculateLongitude", "Recalculate Longitude", "Recalculate Longitude on next run.", true);
            this.SettingTimeZoneLongitude = Settings.GetHandle<float>("TimeZoneLongitude", "Time Zone Longitude", "The Longitude of the location to use as the refrence for Times.", 0.0f);
            //this.SettingCheckIntervalTicks = Settings.GetHandle<float>("CheckIntervalTicks", "CheckIntervalTicks", "How oftern to run the check to take a photo or not.", 1000);

            this.SettingScreenshotHours = Settings.GetHandle<string>("ScreenshotHours", "Screenshot Hours", "Take a Screenshot on these Hours, comma seperated. Defaults to Every 3 Hours.", "0,3,6,9,12,15,18,21");
            this.SettingScreenshotDays = Settings.GetHandle<string>("ScreenshotDays", "Screenshot Days", "Take a Screenshot on these Days, comma seperated. Defaults to Every Day.", "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15");

            this.SettingMessageLabel = Settings.GetHandle<string>("MessageLabel", "Message Label", "The Label to use for Messages", "Photo Day");
            this.SettingMessageContent = Settings.GetHandle<string>("MessageContent", "Message Content", "The Message content.", "Look your best, its Photo Day");
        }

        #endregion

    }
}
