using System;

namespace Wombat.Web.Infrastructure
{
    public class JWTPayload
    {
        public string UserId { get; set; }
        public DateTime Expire { get; set; }
    }
}
