﻿using Newtonsoft.Json.Linq;
using System;

namespace Prism.App.Models
{
    public class FoursquareCheckin
    {
        public string ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtStr { get { return CreatedAt.ToUnix().ToString(); } } // for maps rendering
        public bool? IsMayor { get; set; }

        public string ClientName { get; set; }
        public string VenueName { get; set; }
        public float LocationLat { get; set; }
        public float LocationLng { get; set; }

        public int MyVenueCheckins { get; set; }
        public int TotalVenueCheckins { get; set; }
        public int LikesCount { get; set; }
        public string LikesSummary { get; set; }

        public int CommentsCount { get; set; }
        public int PhotosCount { get; set; }
     

        public float T1Rate { get; set; }

        public FoursquareCheckin()
        {
        }

        public FoursquareCheckin(JObject jcheckin)
        {
            if (jcheckin["venue"] != null) { 
                LocationLat = (float)jcheckin["venue"]["location"]["lat"];
                LocationLng = (float)jcheckin["venue"]["location"]["lng"];
                MyVenueCheckins = (int)jcheckin["venue"]["beenHere"]["count"];
                TotalVenueCheckins = (int)jcheckin["venue"]["stats"]["checkinsCount"];
                VenueName = (string)jcheckin["venue"]["name"];
            }
          
            ID = (string)jcheckin["id"];
            CreatedAt = ((int)jcheckin["createdAt"]).FromUnix();
            LikesCount = (int)jcheckin["likes"]["count"];
            ClientName = (string)jcheckin["source"]["name"];
            IsMayor = (bool?)jcheckin["isMayor"];
            LikesSummary = LikesCount > 0 ? (string)jcheckin["likes"]["summary"] : String.Empty;
            CommentsCount = (int)jcheckin["comments"]["count"];
            PhotosCount = (int)jcheckin["photos"]["count"];
        }
    }
}
