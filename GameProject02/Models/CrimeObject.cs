using System;
using System.Collections.Generic;
using GameProject02.Services;

namespace GameProject02.Models;

public class CrimeObject
{
    // ✅ AUTHENTIC OLD GAME FIELDS (kept for compatibility)
    public int CurrentCrimeType { get; set; } = 0;
    public Dictionary<int, int> CurrentTaskIndex { get; set; } = new();
    public Dictionary<int, int> CurrentTaskExecutionCount { get; set; } = new();

    // ✅ NEW: Chain progression (global task ID -> current successes)
    public Dictionary<int, int> TaskProgress { get; set; } = new();

    // Courage system
    public int Courage { get; set; } = 100;
    public int MaxCourage { get; set; } = 100;
    public long LastCourageRechargeTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Confinement system
    public bool IsInPrison { get; set; } = false;
    public long PrisonReleaseTime { get; set; } = 0;
    public long PrisonBailAmount { get; set; } = 0; // Amount required to pay bail (stored when sent to prison)
    public string PrisonReason { get; set; } = "جرمت وتم القبض عليك"; // Reason for imprisonment
    public bool IsInHospital { get; set; } = false;
    public long HospitalReleaseTime { get; set; } = 0; // Unix timestamp in milliseconds
    public string HospitalReason { get; set; } = "جرحت نفسك أثناء جريمة فاشلة";


    // Statistics
    public int TotalCrimesAttempted { get; set; } = 0;
    public int TotalCrimesSuccessful { get; set; } = 0;
    public int TotalCrimesFailed { get; set; } = 0;
    public int TotalPrisonVisits { get; set; } = 0;
    public int TotalHospitalVisits { get; set; } = 0;

    // ✅ HEALTH SYSTEM (FOR HOSPITAL WORK)
    public int HealthCurrent { get; set; } = 100;
    public int HealthMax { get; set; } = 100;

    // ✅ Mission system
    public int CurrentMissionId { get; set; } = 1;
    public Dictionary<int, int> MissionProgress { get; set; } = new();

    // Regenerate courage
    public void RegenerateCourage()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elapsedSeconds = (now - LastCourageRechargeTime) / 1000;
        var regenerated = elapsedSeconds / 30;

        if (regenerated > 0)
        {
            Courage = Math.Min(MaxCourage, Courage + (int)regenerated);
            LastCourageRechargeTime = now;
        }
    }

    // Check confinement status
    public void CheckConfinementStatus()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // ✅ CHECK PRISON STATUS (AUTHENTIC OLD GAME)
        if (IsInPrison && now >= PrisonReleaseTime)
        {
            IsInPrison = false;
            PrisonReleaseTime = 0;
            PrisonBailAmount = 0;
        }

        // ✅ CHECK HOSPITAL STATUS (ALREADY EXISTS IN YOUR CODE)
        if (IsInHospital && now >= HospitalReleaseTime)
        {
            IsInHospital = false;
            HospitalReleaseTime = 0;
            HealthCurrent = HealthMax;
        }
    }

    // Helper to compute global task ID
    public static int GetGlobalTaskId(int crimeTypeId, int crimeItemId) => crimeTypeId * 100 + crimeItemId;
}