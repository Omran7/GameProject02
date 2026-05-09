using System;
using System.Collections.Generic;

namespace GameProject02.Models;

public class CrimeObject
{
    // تقدم الجرائم
    public int CurrentCrimeType { get; set; } = 0;
    public Dictionary<int, int> CurrentTaskIndex { get; set; } = new();
    public Dictionary<int, int> CurrentTaskExecutionCount { get; set; } = new();
    public Dictionary<int, int> TaskProgress { get; set; } = new();

    // ── الشجاعة ──────────────────────────────────────────────────────────────
    // ✅ المصدر الوحيد للشجاعة الآن هو PlayerAccount.Courage
    // هذه الحقول احتياطية للتوافق مع الكود القديم فقط — لا تُستخدم للاستهلاك
    public int MaxCourage { get; set; } = 100;

    // ── نظام السجن والمستشفى ─────────────────────────────────────────────────
    public bool IsInPrison { get; set; } = false;
    public long PrisonReleaseTime { get; set; } = 0;
    public long PrisonBailAmount { get; set; } = 0;
    public string PrisonReason { get; set; } = "جرمت وتم القبض عليك";

    public bool IsInHospital { get; set; } = false;
    public long HospitalReleaseTime { get; set; } = 0;
    public string HospitalReason { get; set; } = "جرحت نفسك أثناء جريمة فاشلة";

    // ── الصحة (للمستشفى) ─────────────────────────────────────────────────────
    public int HealthCurrent { get; set; } = 100;
    public int HealthMax { get; set; } = 100;

    // ── إحصائيات ─────────────────────────────────────────────────────────────
    public int TotalCrimesAttempted { get; set; } = 0;
    public int TotalCrimesSuccessful { get; set; } = 0;
    public int TotalCrimesFailed { get; set; } = 0;
    public int TotalPrisonVisits { get; set; } = 0;
    public int TotalHospitalVisits { get; set; } = 0;

    // ── المهام ───────────────────────────────────────────────────────────────
    public int CurrentMissionId { get; set; } = 1;
    public Dictionary<int, int> MissionProgress { get; set; } = new();

    // ── التوقيت القديم (للتوافق فقط — لا يُستخدم) ───────────────────────────
    public long LastCourageRechargeTime { get; set; } =
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ── RegenerateCourage حُذفت ───────────────────────────────────────────────
    // RegenerationService هو المسؤول الوحيد عن تجديد الشجاعة الآن.
    // لا تستدعِ أي دالة تجديد من هنا.

    /// <summary>
    /// تحقق من انتهاء وقت السجن أو المستشفى وحرّر اللاعب تلقائياً.
    /// </summary>
    public void CheckConfinementStatus()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (IsInPrison && now >= PrisonReleaseTime)
        {
            IsInPrison = false;
            PrisonReleaseTime = 0;
            PrisonBailAmount = 0;
        }

        if (IsInHospital && now >= HospitalReleaseTime)
        {
            IsInHospital = false;
            HospitalReleaseTime = 0;
            HealthCurrent = HealthMax;
        }
    }

    public static int GetGlobalTaskId(int crimeTypeId, int crimeItemId)
        => crimeTypeId * 100 + crimeItemId;
}