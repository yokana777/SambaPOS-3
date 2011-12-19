﻿namespace Samba.Services
{
    public interface IWorkPeriodService : IService
    {
        void StartWorkPeriod(string description, decimal cashAmount, decimal creditCardAmount, decimal ticketAmount);
        void StopWorkPeriod(string description);
    }
}