namespace GameProject02.Models;

public class JobDefinition
{
    public int Level { get; set; }            // 0,1,2,3,4,5 (progression)
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = string.Empty;
    public long SalaryGold { get; set; }      // Base gold reward per work day
    public int ExperienceReward { get; set; } = 0;

    // Requirements for promotion to this job (from previous level)
    public int RequiredStrength { get; set; }
    public int RequiredDefense { get; set; }
    public int RequiredSpeed { get; set; }
    public int RequiredDexterity { get; set; }
    public double RequiredDaysWorked { get; set; }   // Minimum days in previous job

    // Certificate item that might be required (optional)
    public string RequiredCertificateItemId { get; set; } = string.Empty;
}