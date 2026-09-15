using System;
using Hafiz.Application.Interfaces;
using Hafiz.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace Hafiz.Web.Services
{
    public class UserTimeProvider : IUserTimeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserTimeProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DateTime GetUserNow()
        {
            var context = _httpContextAccessor.HttpContext;
            return context != null
                ? TimeZoneHelper.GetUserNow(context)
                : DateTime.UtcNow;
        }

        public DateTime GetUserToday()
        {
            var context = _httpContextAccessor.HttpContext;
            return context != null
                ? TimeZoneHelper.GetUserToday(context)
                : DateTime.UtcNow.Date;
        }
    }
}
