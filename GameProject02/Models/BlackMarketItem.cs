using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject02.Models
{
    public class BlackMarketItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageResource { get; set; } = "item_unknown";
        public string Description { get; set; } = string.Empty;

        // For preview in list
        public string RequirementPreview => RequiredAmountPerUnit > 0
            ? $"{RequiredAmountPerUnit} {(RequiredItemType == "Gold" ? "ذهب" : RequiredItemType == "Checks" ? "شيكات" : RequiredItemType)}"
            : string.Empty;

        // Conversion Logic
        public string RequiredItemType { get; set; } = "Gold";
        public int RequiredAmountPerUnit { get; set; } = 1;
        public string RewardItemType { get; set; } = "Gold";
        public int RewardAmountPerUnit { get; set; } = 1;

        // Limits
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 100;
        public int CurrentStock { get; set; } = 999;
    }
}
