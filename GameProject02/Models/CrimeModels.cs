using System.Collections.Generic;

namespace GameProject02.Models
{
    public class CrimeTypeDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageResource { get; set; } = "crime_type_one";
        public List<CrimeItemDefinition> Crimes { get; set; } = new();
        public CrimeCompletionReward TypeCompletionReward { get; set; } = new();
    }

    public class CrimeItemDefinition
    {
        public int CrimeTypeId { get; set; }
        public int CrimeItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageResource { get; set; } = "crime_type_one_number_one";
        public int CourageCost { get; set; } = 10;
        public int BaseSuccessChance { get; set; } = 60;
        public int RequiredSuccesses { get; set; } = 10; // number of successful executions needed
        public List<CrimeToolRequirement> ToolRequirements { get; set; } = new();
        public CrimeReward Reward { get; set; } = new();
        public CrimeCompletionReward CompletionReward { get; set; } = new(); // reward for finishing this specific task (if any)
    }

    public class CrimeToolRequirement
    {
        public string ToolItemId { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public int RequiredCount { get; set; } = 1;
    }

    public class CrimeReward
    {
        public int CashRewardMin { get; set; } = 100;
        public int CashRewardMax { get; set; } = 500;
        public int ExperienceReward { get; set; } = 10;
        public List<CrimeItemReward> ItemRewards { get; set; } = new();
    }

    public class CrimeItemReward
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ImageResource { get; set; } = "item_herb";
        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = 1;
        public int DropChance { get; set; } = 100;
    }

    public class CrimeCompletionReward
    {
        public int CashReward { get; set; } = 0;
        public int ExperienceReward { get; set; } = 0;
        public List<CrimeItemReward> ItemRewards { get; set; } = new();
    }
}