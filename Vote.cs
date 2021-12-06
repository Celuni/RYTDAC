using System;
using Newtonsoft.Json;

namespace RYTDAC
{
    public class Vote
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("likes")]
        public int Likes { get; set; }

        [JsonProperty("dislikes")]
        public int Dislikes { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("viewCount")]
        public int ViewCount { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("fromCache")]
        public bool FromCache { get; set; }
    }
}