using System;

namespace GameProject02.Models;

public class WorkObject
{
    public int WorkType { get; set; } = -1;        // -1 = no job
    public int JobLevel { get; set; } = -1;        // -1 = no job
    public long JobStartTimeMilli { get; set; } = 0;      // When started current job
    public long JobGotSalaryTimeMilli { get; set; } = 0;   // Last salary collection timestamp
}