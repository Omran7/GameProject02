using GameProject02.Models;
using System.Collections.Generic;

namespace GameProject02.Services
{
    public static class MissionService
    {
        private static readonly Dictionary<int, MissionDefinition> _missions = new();

        static MissionService()
        {
            LoadMissions();
        }

        private static void LoadMissions()
        {
            // Helper to map old game (goodIdx, goodCate) to your string item IDs
            // Adjust these mappings to match your actual item IDs (e.g., "tool_blank_cd", "herb_1", etc.)
            string GetItemId(int goodIdx, int goodCate)
            {
                // Simple fallback: use the index as a string
                return goodIdx.ToString();
                // TODO: Replace with real mapping when you know your item IDs.
                // Example: if (goodIdx == 23 && goodCate == 9) return "tool_blank_cd";
            }

            // Mission 1: 万事开头难
            _missions[1] = new MissionDefinition
            {
                MissionId = 1,
                TarProgress = 1,
                NextMission = 2,
                RewardExp = 5,
                RewardMoney = 100,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(23, 9), Category = 9, Count = 1 } },
                MissionName = "بداية كل شيء صعبة",
                MissionDesc = "ابحث عن وظيفة مناسبة."
            };

            // Mission 2: 必先利其器
            _missions[2] = new MissionDefinition
            {
                MissionId = 2,
                TarProgress = 1,
                NextMission = 3,
                RewardExp = 5,
                RewardMoney = 200,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(708, 5), Category = 5, Count = 20 } },
                MissionName = "شحذ أدواتك أولاً",
                MissionDesc = "اشترِ مستلزمات التدريب الأساسية."
            };

            // Mission 3: 搜集资金
            _missions[3] = new MissionDefinition
            {
                MissionId = 3,
                TarProgress = 1,
                NextMission = 6,
                RewardExp = 5,
                RewardMoney = 200,
                RequiredCrimeType = 0, // مطاردة المال (crime type 0)
                MissionName = "جمع الأموال",
                MissionDesc = "قم بجمع الأموال في محطة القطار مرة واحدة."
            };

            // Mission 4: 强身健体
            _missions[4] = new MissionDefinition
            {
                MissionId = 4,
                TarProgress = 1,
                NextMission = 16,
                RewardExp = 10,
                RewardMoney = 300,
                MissionName = "تقوية الجسم",
                MissionDesc = "اذهب إلى صالة الألعاب الرياضية مرة واحدة."
            };

            // Mission 5: 小试牛刀
            _missions[5] = new MissionDefinition
            {
                MissionId = 5,
                TarProgress = 1,
                NextMission = 7,
                RewardExp = 20,
                RewardMoney = 500,
                RequiredCrimeType = 1, // Assuming crime type 1 is attack on 包租婆
                MissionName = "اختبار المهارات",
                MissionDesc = "هاجم المالكة وابحث عن أموالها."
            };

            // Mission 6: 安家落户
            _missions[6] = new MissionDefinition
            {
                MissionId = 6,
                TarProgress = 1,
                NextMission = 31,
                RewardExp = 20,
                RewardMoney = 1000,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(23, 9), Category = 9, Count = 1 } },
                MissionName = "تأسيس منزل",
                MissionDesc = "اشترِ عقاراً."
            };

            // Mission 7: 线索
            _missions[7] = new MissionDefinition
            {
                MissionId = 7,
                TarProgress = 1,
                NextMission = 8,
                RewardExp = 20,
                RewardMoney = 500,
                RequiredCrimeType = 7, // Assuming crime type 7 is for 教练阿飞
                MissionName = "الدليل",
                MissionDesc = "هاجم المدرب آفي."
            };

            // Mission 8: 休息一下
            _missions[8] = new MissionDefinition
            {
                MissionId = 8,
                TarProgress = 1,
                NextMission = 9,
                RewardExp = 20,
                RewardMoney = 500,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(510, 3), Category = 3, Count = 20 } },
                MissionName = "خذ قسطاً من الراحة",
                MissionDesc = "اذهب إلى المستشفى للعلاج."
            };

            // Mission 9: 不要让他跑了！
            _missions[9] = new MissionDefinition
            {
                MissionId = 9,
                TarProgress = 1,
                NextMission = 10,
                RewardExp = 30,
                RewardMoney = 500,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(512, 3), Category = 3, Count = 1 } },
                RequiredCrimeType = 9, // Assuming crime type 9 is for 兰博
                MissionName = "لا تدعه يهرب!",
                MissionDesc = "اضرب رامبو بقوة."
            };

            // Mission 10: 重头再来
            _missions[10] = new MissionDefinition
            {
                MissionId = 10,
                TarProgress = 1,
                NextMission = 11,
                RewardExp = 30,
                RewardMoney = 500,
                MissionName = "ابدأ من جديد",
                MissionDesc = "استخدم الدواء للخروج من المستشفى مبكراً."
            };

            // Mission 11: 精挑细选
            _missions[11] = new MissionDefinition
            {
                MissionId = 11,
                TarProgress = 1,
                NextMission = 12,
                RewardExp = 30,
                RewardMoney = 500,
                MissionName = "اختيار دقيق",
                MissionDesc = "اشترِ سلاحاً."
            };

            // Mission 12: 战斗准备
            _missions[12] = new MissionDefinition
            {
                MissionId = 12,
                TarProgress = 1,
                NextMission = 13,
                RewardExp = 30,
                RewardMoney = 200,
                MissionName = "الاستعداد للقتال",
                MissionDesc = "جهِّز سلاحاً."
            };

            // Mission 13: 人靠衣装
            _missions[13] = new MissionDefinition
            {
                MissionId = 13,
                TarProgress = 1,
                NextMission = 14,
                RewardExp = 50,
                RewardMoney = 500,
                MissionName = "المظهر يهم",
                MissionDesc = "اشترِ معطفاً."
            };

            // Mission 14: 全副武装
            _missions[14] = new MissionDefinition
            {
                MissionId = 14,
                TarProgress = 1,
                NextMission = 15,
                RewardExp = 50,
                RewardMoney = 200,
                MissionName = "تسليح كامل",
                MissionDesc = "جهِّز معطفاً."
            };

            // Mission 15: 一雪前耻
            _missions[15] = new MissionDefinition
            {
                MissionId = 15,
                TarProgress = 1,
                NextMission = 18,
                RewardExp = 50,
                RewardMoney = 1000,
                RequiredCrimeType = 9, // Lan Bo again
                MissionName = "محو العار",
                MissionDesc = "اضرب رامبو بقوة."
            };

            // Mission 16: 精疲力竭
            _missions[16] = new MissionDefinition
            {
                MissionId = 16,
                TarProgress = 1,
                NextMission = 5,
                RewardExp = 40,
                RewardMoney = 1000,
                MissionName = "إرهاق تام",
                MissionDesc = "اشترِ جرعة طاقة."
            };

            // Mission 17: 办理户籍
            _missions[17] = new MissionDefinition
            {
                MissionId = 17,
                TarProgress = 1,
                NextMission = 18,
                RewardExp = 20,
                RewardMoney = 100,
                MissionName = "تسجيل الإقامة",
                MissionDesc = "أكمل معلوماتك الشخصية."
            };

            // Mission 18: 熟能手巧
            _missions[18] = new MissionDefinition
            {
                MissionId = 18,
                TarProgress = 1,
                NextMission = 19,
                RewardExp = 50,
                RewardMoney = 2000,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(23, 9), Category = 9, Count = 1 } },
                RequiredCrimeType = 101, // Assuming crime type 101 is 立交桥下拾荒
                MissionName = "الممارسة تجعل الكمال",
                MissionDesc = "اجمع القمامة تحت الجسر."
            };

            // Mission 19: 立足之地
            _missions[19] = new MissionDefinition
            {
                MissionId = 19,
                TarProgress = 1,
                NextMission = 20,
                RewardExp = 50,
                RewardMoney = 2000,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(709, 5), Category = 5, Count = 20 } },
                MissionName = "مكان للوقوف",
                MissionDesc = "هاجم الفتاة الصغيرة (شياو تاي مي 3)."
            };

            // Mission 20: 强化自身
            _missions[20] = new MissionDefinition
            {
                MissionId = 20,
                TarProgress = 1,
                NextMission = 21,
                RewardExp = 50,
                RewardMoney = 2000,
                MissionName = "تقوية الذات",
                MissionDesc = "استخدم نقاط الإنجاز."
            };

            // Mission 21: 广交友人
            _missions[21] = new MissionDefinition
            {
                MissionId = 21,
                TarProgress = 1,
                NextMission = 26,
                RewardExp = 150,
                RewardMoney = 2000,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(27, 9), Category = 9, Count = 5 } },
                MissionName = "تكوين صداقات",
                MissionDesc = "تحدث في قناة الدردشة."
            };

            // Mission 22: 做个买卖
            _missions[22] = new MissionDefinition
            {
                MissionId = 22,
                TarProgress = 1,
                NextMission = 46,
                RewardExp = 100,
                RewardMoney = 2000,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(23, 9), Category = 9, Count = 1 } },
                MissionName = "قم بعملية بيع",
                MissionDesc = "بع شيئاً في السوق."
            };

            // Mission 23: 塑造身形
            _missions[23] = new MissionDefinition
            {
                MissionId = 23,
                TarProgress = 20,
                NextMission = 45,
                RewardExp = 150,
                RewardMoney = 2000,
                MissionName = "تشكيل الجسم",
                MissionDesc = "استثمر 20 نقطة طاقة في صالة الألعاب الرياضية."
            };

            // Mission 24: (not in data)
            // Mission 25: (not in data)
            // Mission 26: 自我提高
            _missions[26] = new MissionDefinition
            {
                MissionId = 26,
                TarProgress = 10,
                NextMission = 49,
                RewardExp = 100,
                RewardMoney = 500,
                MissionName = "تطوير الذات",
                MissionDesc = "ارفع مستواك إلى 10."
            };

            // Mission 27: 与人交恶
            _missions[27] = new MissionDefinition
            {
                MissionId = 27,
                TarProgress = 1,
                NextMission = 22,
                RewardExp = 150,
                RewardMoney = 2000,
                MissionName = "العداء مع الآخرين",
                MissionDesc = "تنافس مع لاعب آخر."
            };

            // Mission 28: 大量搜集
            _missions[28] = new MissionDefinition
            {
                MissionId = 28,
                TarProgress = 5,
                NextMission = 30,
                RewardExp = 200,
                RewardMoney = 2000,
                MissionName = "جمع مكثف",
                MissionDesc = "نفذ 5 جرائم ناجحة."
            };

            // Mission 29: (not in data)
            // Mission 30: 渐入佳境
            _missions[30] = new MissionDefinition
            {
                MissionId = 30,
                TarProgress = 15,
                NextMission = 36,
                RewardExp = 150,
                RewardMoney = 5000,
                MissionName = "دخول في الأجواء",
                MissionDesc = "ارفع مستواك إلى 15."
            };

            // Mission 31: 搬家
            _missions[31] = new MissionDefinition
            {
                MissionId = 31,
                TarProgress = 1,
                NextMission = 4,
                RewardExp = 20,
                RewardMoney = 500,
                MissionName = "الانتقال",
                MissionDesc = "انتقل إلى منزلك الجديد."
            };

            // Mission 32-35: not in data (some are branch missions)
            // Mission 36: 飞往帝都
            _missions[36] = new MissionDefinition
            {
                MissionId = 36,
                TarProgress = 1,
                NextMission = 37,
                RewardExp = 150,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(7, 9), Category = 9, Count = 1 } },
                MissionName = "السفر إلى العاصمة",
                MissionDesc = "سافر إلى العاصمة."
            };

            // Mission 37: 开启跑商之旅1
            _missions[37] = new MissionDefinition
            {
                MissionId = 37,
                TarProgress = 1,
                NextMission = 38,
                RewardExp = 250,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(7, 9), Category = 9, Count = 1 } },
                RequiredCrimeType = null,
                MissionName = "بدء رحلة التجارة 1",
                MissionDesc = "اشترِ مشروب دو جي."
            };

            // Mission 38: 开启跑商之旅2
            _missions[38] = new MissionDefinition
            {
                MissionId = 38,
                TarProgress = 1,
                NextMission = 40,
                RewardExp = 280,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(158, 9), Category = 9, Count = 1 } },
                MissionName = "بدء رحلة التجارة 2",
                MissionDesc = "عد إلى مدينة خه‌شيه وبع المشروب."
            };

            // Mission 39: 打了鸡血1
            _missions[39] = new MissionDefinition
            {
                MissionId = 39,
                TarProgress = 1,
                NextMission = -1,
                RewardExp = 300,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(7, 9), Category = 9, Count = 1 } },
                MissionName = "منشط 1",
                MissionDesc = "اذهب إلى كيلونغ واشترِ منشطاً."
            };

            // Mission 40: 打了鸡血2
            _missions[40] = new MissionDefinition
            {
                MissionId = 40,
                TarProgress = 1,
                NextMission = 42,
                RewardExp = 350,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(507, 2), Category = 2, Count = 1 } },
                MissionName = "منشط 2",
                MissionDesc = "استخدم المنشط مرة واحدة."
            };

            // Mission 41: 前往湾仔
            _missions[41] = new MissionDefinition
            {
                MissionId = 41,
                TarProgress = 1,
                NextMission = -1,
                RewardExp = 450,
                RewardMoney = 0,
                MissionName = "اذهب إلى وان تساي",
                MissionDesc = "اذهب إلى وان تساي واشترِ فاصوليا سعيدة."
            };

            // Mission 42: 再次卖货
            _missions[42] = new MissionDefinition
            {
                MissionId = 42,
                TarProgress = 1,
                NextMission = 50,
                RewardExp = 950,
                RewardMoney = 2000,
                MissionName = "بيع مرة أخرى",
                MissionDesc = "اعرض فاصوليا سعيدة للبيع في السوق."
            };

            // Mission 43: 大卖场
            _missions[43] = new MissionDefinition
            {
                MissionId = 43,
                TarProgress = 1,
                NextMission = 44,
                RewardExp = 650,
                RewardMoney = 0,
                MissionName = "السوق الكبير",
                MissionDesc = "تفقّد فاصوليا سعيدة في السوق."
            };

            // Mission 44: 积累财富
            _missions[44] = new MissionDefinition
            {
                MissionId = 44,
                TarProgress = 1000000,
                NextMission = 45,
                RewardExp = 700,
                RewardMoney = 0,
                MissionName = "تراكم الثروة",
                MissionDesc = "اجمع مليون دولار."
            };

            // Mission 45: 结交友人
            _missions[45] = new MissionDefinition
            {
                MissionId = 45,
                TarProgress = 1,
                NextMission = 36,
                RewardExp = 200,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(8, 100), Category = 100, Count = 2 } },
                MissionName = "تكوين صديق",
                MissionDesc = "أضف صديقاً."
            };

            // Mission 46: 查看市场
            _missions[46] = new MissionDefinition
            {
                MissionId = 46,
                TarProgress = 1,
                NextMission = 23,
                RewardExp = 400,
                RewardMoney = 0,
                MissionName = "تفقّد السوق",
                MissionDesc = "تفقّد زيت الزهرة الحمراء في السوق."
            };

            // Mission 47: 多次历练
            _missions[47] = new MissionDefinition
            {
                MissionId = 47,
                TarProgress = 1,
                NextMission = -1,
                RewardExp = 900,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(8, 9), Category = 9, Count = 3 } },
                UnlocksCrimeType = 1, // Unlocks crime type 1 (بيع اسطوانات مضروبة)
                MissionName = "تجارب متعددة",
                MissionDesc = "افتح تجربة بيع الأقراص المقلدة."
            };

            // Mission 48: 升到30级
            _missions[48] = new MissionDefinition
            {
                MissionId = 48,
                TarProgress = 30,
                NextMission = 51,
                RewardExp = 1000,
                RewardMoney = 0,
                MissionName = "الوصول إلى المستوى 30",
                MissionDesc = "ارفع مستواك إلى 30."
            };

            // Mission 49: 使用支票购买道具
            _missions[49] = new MissionDefinition
            {
                MissionId = 49,
                TarProgress = 1,
                NextMission = 27,
                RewardExp = 100,
                RewardMoney = 0,
                MissionName = "شراء أدوات بشيك",
                MissionDesc = "استخدم شيكاً لشراء عنصر."
            };

            // Mission 50: 四项属性都达到50
            _missions[50] = new MissionDefinition
            {
                MissionId = 50,
                TarProgress = 1,
                NextMission = 48,
                RewardExp = 0,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(7, 9), Category = 9, Count = 1 } },
                MissionName = "كل الصفات الأربع تصل إلى 50",
                MissionDesc = "اجعل جميع الصفات الأربع 50."
            };

            // Mission 51: 进行1次副本
            _missions[51] = new MissionDefinition
            {
                MissionId = 51,
                TarProgress = 1,
                NextMission = 52,
                RewardExp = 0,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(7, 9), Category = 9, Count = 1 } },
                MissionName = "أداء الزنزانة مرة واحدة",
                MissionDesc = "استكشف مستودع القمامة."
            };

            // Mission 52: 天梯基隆挑战
            _missions[52] = new MissionDefinition
            {
                MissionId = 52,
                TarProgress = 10,
                NextMission = 54,
                RewardExp = 0,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(8, 9), Category = 9, Count = 4 } },
                MissionName = "تحدي سلم كيلونغ",
                MissionDesc = "اهزم 10 خصوم متتاليين."
            };

            // Mission 53: 进行1次副本
            _missions[53] = new MissionDefinition
            {
                MissionId = 53,
                TarProgress = 1,
                NextMission = -1,
                RewardExp = 0,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(111, 9), Category = 9, Count = 2 } },
                MissionName = "أداء الزنزانة مرة واحدة",
                MissionDesc = "استكشف مستودع القمامة."
            };

            // Mission 54: 兑换一次
            _missions[54] = new MissionDefinition
            {
                MissionId = 54,
                TarProgress = 1,
                NextMission = 53,
                RewardExp = 0,
                RewardMoney = 0,
                RewardGoods = new List<RewardItem> { new RewardItem { ItemId = GetItemId(294, 0), Category = 0, Count = 1 } },
                MissionName = "استبدال مرة واحدة",
                MissionDesc = "استبدل سمك الهامور المقلي."
            };
        }

        public static void OnCrimeDone(PlayerAccount player, int crimeType, bool success)
        {
            var mission = GetCurrentMission(player);
            if (mission == null) return;

            if (mission.RequiredCrimeType == crimeType && success)
            {
                int currentProgress = player.CrimeObject.MissionProgress.GetValueOrDefault(mission.MissionId, 0);
                currentProgress++;
                player.CrimeObject.MissionProgress[mission.MissionId] = currentProgress;

                if (currentProgress >= mission.TarProgress)
                {
                    CompleteMission(player, mission);
                }
            }
        }

        private static void CompleteMission(PlayerAccount player, MissionDefinition mission)
        {
            player.Gold += mission.RewardMoney;
            player.CurrentXP += mission.RewardExp;
            foreach (var reward in mission.RewardGoods)
            {
                if (!player.StockObject.ItemsInStock.ContainsKey(reward.ItemId))
                    player.StockObject.ItemsInStock[reward.ItemId] = new StockItem { Count = 0 };
                player.StockObject.ItemsInStock[reward.ItemId].Count += reward.Count;
            }

            if (mission.NextMission > 0)
            {
                player.CrimeObject.CurrentMissionId = mission.NextMission;
            }

            if (mission.UnlocksCrimeType != null)
            {
                player.CrimeObject.CurrentCrimeType = mission.UnlocksCrimeType.Value;
                if (!player.CrimeObject.CurrentTaskIndex.ContainsKey(mission.UnlocksCrimeType.Value))
                {
                    player.CrimeObject.CurrentTaskIndex[mission.UnlocksCrimeType.Value] = 0;
                    player.CrimeObject.CurrentTaskExecutionCount[mission.UnlocksCrimeType.Value] = 0;
                }
            }
        }

        public static MissionDefinition GetCurrentMission(PlayerAccount player)
        {
            _missions.TryGetValue(player.CrimeObject.CurrentMissionId, out var mission);
            return mission;
        }

        public static MissionDefinition GetMissionDefinition(int missionId)
        {
            _missions.TryGetValue(missionId, out var mission);
            return mission;
        }
    }

    public class MissionDefinition
    {
        public int MissionId { get; set; }
        public int TarProgress { get; set; }
        public int NextMission { get; set; }
        public int RewardExp { get; set; }
        public int RewardMoney { get; set; }
        public List<RewardItem> RewardGoods { get; set; } = new();
        public string MissionName { get; set; }
        public string MissionDesc { get; set; }
        public int? RequiredCrimeType { get; set; }
        public int? UnlocksCrimeType { get; set; }
    }

    public class RewardItem
    {
        public string ItemId { get; set; }
        public int Category { get; set; }
        public int Count { get; set; }
    }
}