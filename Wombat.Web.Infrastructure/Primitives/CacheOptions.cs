﻿
namespace Wombat.Web.Infrastructure
{
    public class CacheOptions
    {
        public CacheType CacheType { get; set; }
        public string RedisEndpoint { get; set; }
    }
}
