namespace GameProject02.Models;

public class WorkCategory
{
    public int Id { get; set; }           // 0=Restaurant, 1=Bank, 2=Cinema, 3=ScienceLab, 4=Army, 5=Hospital, 6=CoalMining, 7=Freelancer, 8=School
    public string Name { get; set; } = string.Empty;
    public string ImageResource { get; set; } = string.Empty;
    public List<JobDefinition> Jobs { get; set; } = new();
}