﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.App.Models
{
    public class FoursquareProcessing
    {
        public List<Action<FoursquareLiveStats>> InitFunctions; // todo replace with Task<>

        public List<Action<FoursquareCheckin, FoursquareLiveStats, SocialPlayer>> CalculationFunctions; // todo replace with Task<>

        public FoursquareProcessing()
        {
            CalculationFunctions = new List<Action<FoursquareCheckin, FoursquareLiveStats, SocialPlayer>>();
            InitFunctions = new List<Action<FoursquareLiveStats>>();

            InitFunctions.Add(stats =>
            {
                stats.KeyValue.Add("TopSpeed", 0.0f);
                stats.KeyValue.Add("CurrentSpeed", 0.0f);
            });
            /// Common routines
            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {                
                stats.TotalCheckins++;
                stats.LastDistance = CaclulateDistanceBeetweenTwoPoints(stats.PreviousCheckin, currentCheckin);
                stats.TotalDistance+= stats.LastDistance;
                stats.KeyValue["CurrentSpeed"] = (float)stats.LastDistance / (currentCheckin.CreatedAt - stats.PreviousCheckin.CreatedAt).Hours;
                if ((float)stats.KeyValue["CurrentSpeed"] > (float)stats.KeyValue["TopSpeed"])
                    stats.KeyValue["TopSpeed"] = stats.KeyValue["CurrentSpeed"];
            });


            /// MOST LIKED & MOST POPULAR
            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {
                if (stats.MostLikedCheckin == null) stats.MostLikedCheckin = currentCheckin;
                if (stats.MostPopularCheckin == null) stats.MostPopularCheckin = currentCheckin;
                if (stats.MostLikedCheckin.LikesCount < currentCheckin.LikesCount) stats.MostLikedCheckin = currentCheckin;
                if (stats.MostPopularCheckin.TotalVenueCheckins < currentCheckin.TotalVenueCheckins) stats.MostPopularCheckin = currentCheckin;
            });

            /// MY TOP PLACE 
            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {
                // current 
                // rate = my_checkins * total_in_venue
                // TODO: calibrate it
                currentCheckin.T1Rate = currentCheckin.MyVenueCheckins * currentCheckin.TotalVenueCheckins;
                if (stats.MyTopCheckin == null) stats.MyTopCheckin = currentCheckin;
                if (stats.MyTopCheckin.T1Rate < currentCheckin.T1Rate) stats.MyTopCheckin = currentCheckin;
            });

            /// MY TOP CLIENT
            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {
                if (!stats.KeyValue.ContainsKey("TopClient"))
                {
                    stats.KeyValue.Add("TopClients", new Dictionary<string, int>());
                    stats.KeyValue.Add("TopClient", String.Empty);
                }
                var topClients = (Dictionary<string, int>)stats.KeyValue["TopClients"];
                if (topClients.ContainsKey(currentCheckin.ClientName))
                    topClients[currentCheckin.ClientName]++;
                else
                {
                    topClients.Add(currentCheckin.ClientName, 1);
                    socialPlayer.Apply(PlayerSkill.Sociality, SocialExperienceConstants.Foursquare.CHECKIN_WITH_NEW_FOURSQUARE_CLIENT);
                    socialPlayer.Achievements.Add(String.Format("{0} Checkin with new client: {1}", currentCheckin.CreatedAt, currentCheckin.ClientName));
                }

                stats.KeyValue["TopClient"] = topClients.OrderByDescending(c => c.Value).First().Key;              
            });

            TimelineProcessingTasks();
            ExperienceAccumulationTasks();

            CalculationFunctions.Add((checkin, stats, socialPlayer) =>
            {
                if (checkin.LocationLat != 0 || checkin.LocationLng != 0)
                    stats.PreviousCheckin = checkin;
            });

           
        }

        private void TimelineProcessingTasks()
        {
            InitFunctions.Add(stats => {
                if (!stats.Temporary.ContainsKey("CheckinsTimeline"))
                {
                    stats.Temporary.Add("CheckinsTimeline", new Dictionary<string, int>());
                }

                if (!stats.KeyValue.ContainsKey("timeline"))
                    stats.KeyValue.Add("timeline", new List<int>());

                if (!stats.KeyValue.ContainsKey("timelineX"))
                    stats.KeyValue.Add("timelineX", new List<string>());
            });


            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {
                var timeline = (List<int>)stats.KeyValue["timeline"];
                var timelineX = (List<string>)stats.KeyValue["timelineX"];
                var currentDay = stats.Temporary.ContainsKey("CurrentDay") ? (int)stats.Temporary["CurrentDay"] : 0;

                if (currentCheckin.CreatedAt.Day != currentDay)
                {
                    currentDay = currentCheckin.CreatedAt.Day;
                    stats.Temporary["CurrentDay"] = currentDay;
                    timeline.Add(stats.Temporary.ContainsKey("CurrentDayCount") ? (int)stats.Temporary["CurrentDayCount"] : 1);
                    timelineX.Add(GetTimelineKey(currentCheckin.CreatedAt));
                    stats.Temporary["CurrentDayCount"] = 0;
                }

                if (stats.Temporary.ContainsKey("CurrentDayCount"))
                    stats.Temporary["CurrentDayCount"] = (int)stats.Temporary["CurrentDayCount"] + 1;
                else
                    stats.Temporary.Add("CurrentDayCount", 1);

                stats.KeyValue["timeline"] = timeline;

            });
        }

        private void ExperienceAccumulationTasks()
        {
            CalculationFunctions.Add((currentCheckin, stats, socialPlayer) =>
            {
                socialPlayer.Apply(PlayerSkill.Sociality,SocialExperienceConstants.Foursquare.BASE_CHECKIN);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.ONE_KILOMETER_PASSED * (int)stats.LastDistance);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_WITH_MORE_THAN_100_CHECKINS, currentCheckin.TotalVenueCheckins > 100);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_WITH_MORE_THAN_1000_CHECKINS, currentCheckin.TotalVenueCheckins > 1000);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_WITH_MORE_THAN_10000_CHECKINS, currentCheckin.TotalVenueCheckins > 10000);
                socialPlayer.Apply(PlayerSkill.Sociality, SocialExperienceConstants.Foursquare.ONE_LIKE_TO_CHECKIN * currentCheckin.LikesCount);
                socialPlayer.Apply(PlayerSkill.Sociality, SocialExperienceConstants.Foursquare.MAYORSHIP_CHECKIN, currentCheckin.IsMayor.HasValue && currentCheckin.IsMayor.Value);
                socialPlayer.Apply(PlayerSkill.Sociality, SocialExperienceConstants.Foursquare.ONE_COMMENT_TO_CHECKIN * currentCheckin.CommentsCount);
                socialPlayer.Apply(PlayerSkill.Sociality, SocialExperienceConstants.Foursquare.CHECKIN_WITH_PHOTO * currentCheckin.PhotosCount);

                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_REMOTE_FROM_LAST_AT_1000KM, stats.LastDistance > 1000);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_REMOTE_FROM_LAST_AT_5000KM, stats.LastDistance > 5000);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_PLACE_REMOTE_FROM_LAST_AT_10000KM, stats.LastDistance > 10000);
                socialPlayer.Apply(PlayerSkill.Curiosity, SocialExperienceConstants.Foursquare.CHECKIN_AT_JUST_CREATED_PLACE, currentCheckin.TotalVenueCheckins == 0);
                

                
            });
        }

        
        public void Finalize(FoursquareLiveStats liveStats)
        {        
            
            
        }

        private string GetTimelineKey(DateTime forDateTime)
        {
            return forDateTime.ToShortDateString();//.Year.ToString() + forDateTime.Month.ToString() + forDateTime.Day.ToString();
        }

        private double CaclulateDistanceBeetweenTwoPoints(FoursquareCheckin previous, FoursquareCheckin current)
        {
            if (previous == null) return 0;
            if (current.LocationLat == 0 && current.LocationLng == 0) return 0;

            var R = 6371; // km
            var dLat = toRad(current.LocationLat - previous.LocationLat);
            var dLon = toRad(current.LocationLng - previous.LocationLng);
            var lat1 = toRad(current.LocationLat);
            var lat2 = toRad(previous.LocationLat);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d;
        }

        private double toRad(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
