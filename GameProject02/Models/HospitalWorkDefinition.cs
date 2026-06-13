using System;
using System.Collections.Generic;

namespace GameProject02.Models;

// ✅ AUTHENTIC OLD GAME HOSPITAL WORK TIERS (FROM DECOMPILED CODE)
public class HospitalWorkDefinition
{
    public int WorkLevel { get; set; }          // 3, 4, 5 (matching old game)
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "work_hospital";

    // Requirements
    public int RequiredCash { get; set; }       // 100000, 300000, 400000
    public int RequiredCourage { get; set; }    // 10, 15, 20

    // Rewards
    public int HealthRestored { get; set; }     // 75, 250, 500
    public int ExperienceReward { get; set; }   // 75, 250, 500

    // Unlock requirement (player level)
    public int RequiredPlayerLevel { get; set; } = 0; // 0, 103, 119 (from decompiled code)
}