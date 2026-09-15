using System;

namespace Hafiz.Application.Interfaces
{
    public interface IUserTimeProvider
    {
        DateTime GetUserNow();
        DateTime GetUserToday();
    }
}
