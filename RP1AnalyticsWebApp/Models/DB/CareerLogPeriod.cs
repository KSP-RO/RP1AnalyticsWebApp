﻿using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class CareerLogPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumEngineers { get; set; }
        public int NumResearchers { get; set; }
        public double EfficiencyEngineers { get; set; }
        public double CurrentFunds { get; set; }
        public double CurrentSci { get; set; }
        public int RnDQueueLength { get; set; }
        public double ScienceEarned { get; set; }
        public double SalaryEngineers { get; set; }
        public double SalaryResearchers { get; set; }
        public double SalaryCrew { get; set; }
        public double ProgramFunds { get; set; }
        public double OtherFundsEarned { get; set; }
        public double LaunchFees { get; set; }
        public double VesselPurchase { get; set; }
        public double VesselRecovery { get; set; }
        public double LCMaintenance { get; set; }
        public double FacilityMaintenance { get; set; }
        public double MaintenanceFees { get; set; }
        public double TrainingFees { get; set; }
        public double ToolingFees { get; set; }
        public double EntryCosts { get; set; }
        public double SpentUnlockCredit { get; set; }
        public double ConstructionFees { get; set; }
        public double HiringResearchers { get; set; }
        public double HiringEngineers { get; set; }
        public double OtherFees { get; set; }
        public double SubsidySize { get; set; }
        public double SubsidyPaidOut { get; set; }
        public double RepFromPrograms { get; set; }
        public double FundsGainMult { get; set; }
        public int NumNautsKilled { get; set; }
        public double Confidence { get; set; }
        public double Reputation { get; set; }

        public CareerLogPeriod()
        {
        }

        public CareerLogPeriod(CareerLogPeriodDto c)
        {
            StartDate = c.StartDate;
            EndDate = c.EndDate;
            NumEngineers = c.NumEngineers;
            NumResearchers = c.NumResearchers;
            EfficiencyEngineers = c.EfficiencyEngineers;
            CurrentFunds = c.CurrentFunds;
            CurrentSci = c.CurrentSci;
            RnDQueueLength = c.RnDQueueLength;
            ScienceEarned = c.ScienceEarned;
            SalaryEngineers = c.SalaryEngineers;
            SalaryResearchers = c.SalaryResearchers;
            SalaryCrew = c.SalaryCrew;
            ProgramFunds = c.ProgramFunds;
            OtherFundsEarned = c.OtherFundsEarned;
            LaunchFees = c.LaunchFees;
            VesselPurchase = c.VesselPurchase;
            VesselRecovery = c.VesselRecovery;
            LCMaintenance = c.LCMaintenance;
            FacilityMaintenance = c.FacilityMaintenance;
            MaintenanceFees = c.MaintenanceFees;
            TrainingFees = c.TrainingFees;
            ToolingFees = c.ToolingFees;
            EntryCosts = c.EntryCosts;
            SpentUnlockCredit = c.SpentUnlockCredit;
            ConstructionFees = c.ConstructionFees;
            HiringResearchers = c.HiringResearchers;
            HiringEngineers = c.HiringEngineers;
            OtherFees = c.OtherFees;
            SubsidySize = c.SubsidySize;
            SubsidyPaidOut = c.SubsidyPaidOut;
            RepFromPrograms = c.RepFromPrograms;
            FundsGainMult = c.FundsGainMult;
            NumNautsKilled = c.NumNautsKilled;
            Confidence = c.Confidence;
            Reputation = c.Reputation;
        }
    }
}
