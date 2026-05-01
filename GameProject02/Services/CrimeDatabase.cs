using GameProject02.Models;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class CrimeDatabase
{
    private static readonly Dictionary<int, CrimeTypeDefinition> _crimeTypes = new();
    private static List<CrimeItemDefinition> _allTasks = new();
    private static Dictionary<(int crimeTypeId, int crimeItemId), int> _taskToLinearIndex = new();
    private static Dictionary<int, int> _linearIndexToCrimeTypeId = new();

    static CrimeDatabase()
    {
        InitializeCrimeTypes();
        BuildLinearOrder();
    }

    public static CrimeTypeDefinition GetCrimeType(int crimeTypeId)
        => _crimeTypes.TryGetValue(crimeTypeId, out var type) ? type : null;

    public static CrimeItemDefinition GetCrimeItem(int crimeTypeId, int crimeItemId)
    {
        var type = GetCrimeType(crimeTypeId);
        return type?.Crimes?.Count > crimeItemId ? type.Crimes[crimeItemId] : null;
    }

    public static IReadOnlyDictionary<int, CrimeTypeDefinition> GetAllCrimeTypes()
        => _crimeTypes;

    public static CrimeItemDefinition GetTaskByLinearIndex(int index)
        => (index >= 0 && index < _allTasks.Count) ? _allTasks[index] : null;

    public static int GetLinearIndex(int crimeTypeId, int crimeItemId)
        => _taskToLinearIndex.TryGetValue((crimeTypeId, crimeItemId), out var idx) ? idx : -1;

    public static int GetCrimeTypeIdByLinearIndex(int linearIndex)
        => _linearIndexToCrimeTypeId.TryGetValue(linearIndex, out var ct) ? ct : -1;

    public static CrimeItemDefinition GetCrimeItemByGlobalId(int globalId)
        => GetCrimeItem(globalId / 100, globalId % 100);

    private static void BuildLinearOrder()
    {
        int linearIdx = 0;
        for (int ct = 0; ct <= 16; ct++)
        {
            var crimeType = GetCrimeType(ct);
            if (crimeType != null)
                foreach (var task in crimeType.Crimes)
                {
                    _allTasks.Add(task);
                    _taskToLinearIndex[(ct, task.CrimeItemId)] = linearIdx;
                    _linearIndexToCrimeTypeId[linearIdx] = ct;
                    linearIdx++;
                }
        }
    }

    private static void InitializeCrimeTypes()
    {

        // ========== TYPE 0: مطاردة المال (6 tasks) ==========
        var type0 = new CrimeTypeDefinition
        {
            Id = 0,
            Name = "مطاردة المال",
            Description = "ابحث عن الأموال المفقودة في الأماكن العامة",
            ImageResource = "crime_type_one",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 2000,
                ExperienceReward = 200,
                ItemRewards = new List<CrimeItemReward>
                {
                    new CrimeItemReward { ItemId = "tool_sunglasses", ItemName = "اسطوانة فارغة", ImageResource = "market_crime_tool_blank_cd", MinCount = 10, MaxCount = 10, DropChance = 100 }
                }
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 0,
                    Name = "سرقة المحفظة من جيب رجل",
                    Description = "ابحث بين المقاعد والصناديق المفقودة",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 100,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 100, CashRewardMax = 300, ExperienceReward = 10
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 1,
                    Name = "اختطف هاتف شخص منشغل",
                    Description = "ابحث بين الحجارة والصناديق القديمة",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 50,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 200, CashRewardMax = 400, ExperienceReward = 15
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 2,
                    Name = "انتزع حقيبة امرأة عجوز",
                    Description = "ابحث بين أكياس الزبالة عن محتويات قيمة",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 100,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 300, CashRewardMax = 600, ExperienceReward = 20
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 3,
                    Name = "سرقة بزحام السوق الشعبي",
                    Description = "ابحث في الحدائق العامة والشوارع الجانبية",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 100,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 400, CashRewardMax = 800, ExperienceReward = 25
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 4,
                    Name = "تشتيت الانتباه ونشل الجيوب",
                    Description = "ابحث بين الخردة عن أشياء ذات قيمة",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 100,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 500, CashRewardMax = 1000, ExperienceReward = 30
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 0, CrimeItemId = 5,
                    Name = "نشل من المسافرين بالمطار",
                    Description = "ابحث في منطقة الميناء المهجورة",
                    ImageResource = "crime_type_1",
                    CourageCost = 1,
                    BaseSuccessChance = 100,
                    RequiredSuccesses = 5,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_sunglasses", ToolName = "نظارة شمسية", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 600, CashRewardMax = 1200, ExperienceReward = 35
                    }
                },
            }
        };
        _crimeTypes[0] = type0;

        // ========== TYPE 1: بيع اسطوانات (10 tasks) ==========
        var type1 = new CrimeTypeDefinition
        {
            Id = 1,
            Name = "اسرق محل بقالة ليلا",
            Description = "بيع اسطوانات مضروبة في الأسواق الشعبية",
            ImageResource = "crime_type_two",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 6000,
                ExperienceReward = 600,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 0,
                    Name = "تسلل لمحل ملابس بعد الإغلاق",
                    Description = "بع اسطوانة مضروبة لزبون عادي",
                    ImageResource = "crime_type_two_number_one",
                    CourageCost = 2,
                    BaseSuccessChance = 95,
                    RequiredSuccesses = 8,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 150, CashRewardMax = 300, ExperienceReward = 12
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 1,
                    Name = "اقتحم صيدلية واسرق الأدوية",
                    Description = "بع اسطوانتين لزبونين مختلفين",
                    ImageResource = "crime_type_two_number_two",
                    CourageCost = 2,
                    BaseSuccessChance = 92,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 200, CashRewardMax = 400, ExperienceReward = 15
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 2,
                    Name = "اسرق محل موبايلات حديثة",
                    Description = "بع ثلاث اسطوانات في نفس اليوم",
                    ImageResource = "crime_type_two_number_three",
                    CourageCost = 2,
                    BaseSuccessChance = 90,
                    RequiredSuccesses = 12,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 280, CashRewardMax = 560, ExperienceReward = 18
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 3,
                    Name = "سرقة من سوبر ماركت",
                    Description = "بع أربع اسطوانات لتجار صغار",
                    ImageResource = "crime_type_two_number_four",
                    CourageCost = 2,
                    BaseSuccessChance = 88,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 350, CashRewardMax = 700, ExperienceReward = 22
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 4,
                    Name = "سرقت ساعات فاخرة",
                    Description = "بع خمس اسطوانات دفعة واحدة",
                    ImageResource = "crime_type_two_number_five",
                    CourageCost = 2,
                    BaseSuccessChance = 85,
                    RequiredSuccesses = 16,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 450, CashRewardMax = 900, ExperienceReward = 26
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 5,
                    Name = "سرقة من محل مجوهرات",
                    Description = "بع دفعة كبيرة لتاجر جملة",
                    ImageResource = "crime_type_two_number_six",
                    CourageCost = 3,
                    BaseSuccessChance = 82,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 3 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 600, CashRewardMax = 1200, ExperienceReward = 30
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 6,
                    Name = "السرقة من مول كبير",
                    Description = "وزع اسطوانات على عدة مناطق",
                    ImageResource = "crime_type_two_number_seven",
                    CourageCost = 3,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 3 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 750, CashRewardMax = 1500, ExperienceReward = 35
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 7,
                    Name = "سرقت نقود من محل صرافه",
                    Description = "هرب دفعة اسطوانات عبر نقطة تفتيش",
                    ImageResource = "crime_type_two_number_eight",
                    CourageCost = 3,
                    BaseSuccessChance = 77,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 5 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1000, CashRewardMax = 2000, ExperienceReward = 40
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 8,
                    Name = "اسرق شحنات من مستودع",
                    Description = "بع لشبكة توزيع في عدة مدن",
                    ImageResource = "crime_type_two_number_nine",
                    CourageCost = 3,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 24,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 5 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1300, CashRewardMax = 2600, ExperienceReward = 46
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 1, CrimeItemId = 9,
                    Name = "صفقة الكمية الضخمة",
                    Description = "أتمم أضخم صفقة اسطوانات",
                    ImageResource = "crime_type_two_number_ten",
                    CourageCost = 3,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 26,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 8 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1800, CashRewardMax = 3600, ExperienceReward = 52
                    }
                },
            }
        };
        _crimeTypes[1] = type1;

        // ========== TYPE 2: سرقة محلات (8 tasks) ==========
        var type2 = new CrimeTypeDefinition
        {
            Id = 2,
            Name = "سرقة محلات",
            Description = "سرقة المحلات التجارية ليلاً",
            ImageResource = "crime_type_three",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 8000,
                ExperienceReward = 800,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 0,
                    Name = "سرقة بقالة صغيرة",
                    Description = "اقتحم بقالة صغيرة بعد الإغلاق",
                    ImageResource = "crime_type_three_number_one",
                    CourageCost = 3,
                    BaseSuccessChance = 88,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 300, CashRewardMax = 600, ExperienceReward = 20
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 1,
                    Name = "سرقة محل ملابس",
                    Description = "اقتحم محل ملابس وسرق البضاعة",
                    ImageResource = "crime_type_three_number_two",
                    CourageCost = 3,
                    BaseSuccessChance = 85,
                    RequiredSuccesses = 12,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 500, CashRewardMax = 1000, ExperienceReward = 26
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 2,
                    Name = "سرقة صيدلية",
                    Description = "اقتحم صيدلية واسرق الأدوية والمعدات",
                    ImageResource = "crime_type_three_number_three",
                    CourageCost = 3,
                    BaseSuccessChance = 82,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 700, CashRewardMax = 1400, ExperienceReward = 32
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 3,
                    Name = "سرقة محل إلكترونيات",
                    Description = "اقتحم محل إلكترونيات وسرق الأجهزة",
                    ImageResource = "crime_type_three_number_four",
                    CourageCost = 4,
                    BaseSuccessChance = 79,
                    RequiredSuccesses = 16,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1000, CashRewardMax = 2000, ExperienceReward = 38
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 4,
                    Name = "سرقة مكتبة ومحل هدايا",
                    Description = "اقتحم محلاً للهدايا وسرق قطع ثمينة",
                    ImageResource = "crime_type_three_number_five",
                    CourageCost = 4,
                    BaseSuccessChance = 76,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1300, CashRewardMax = 2600, ExperienceReward = 44
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 5,
                    Name = "سرقة محل ساعات",
                    Description = "اقتحم محل ساعات فاخرة",
                    ImageResource = "crime_type_three_number_six",
                    CourageCost = 4,
                    BaseSuccessChance = 73,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1700, CashRewardMax = 3400, ExperienceReward = 52
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 6,
                    Name = "سرقة محل المجوهرات",
                    Description = "اقتحم محل مجوهرات وسرق الذهب",
                    ImageResource = "crime_type_three_number_seven",
                    CourageCost = 5,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2200, CashRewardMax = 4400, ExperienceReward = 60
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 2, CrimeItemId = 7,
                    Name = "سرقة المركز التجاري",
                    Description = "اقتحم محلات في مركز تجاري كبير",
                    ImageResource = "crime_type_three_number_eight",
                    CourageCost = 5,
                    BaseSuccessChance = 67,
                    RequiredSuccesses = 24,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3000, CashRewardMax = 6000, ExperienceReward = 70
                    }
                },
            }
        };
        _crimeTypes[2] = type2;

        // ========== TYPE 3: تثبيت وسرقة (6 tasks) ==========
        var type3 = new CrimeTypeDefinition
        {
            Id = 3,
            Name = "تثبيت وسرقة",
            Description = "تثبيت الضحايا وسرقتهم في الشوارع",
            ImageResource = "crime_type_five",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 10000,
                ExperienceReward = 1000,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 0,
                    Name = "تثبيت شخص عادي",
                    Description = "أوقف شخصاً في الشارع واسرق حقيبته",
                    ImageResource = "crime_type_five_number_one",
                    CourageCost = 4,
                    BaseSuccessChance = 85,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 600, CashRewardMax = 1200, ExperienceReward = 30
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 1,
                    Name = "تثبيت تاجر",
                    Description = "أوقف تاجراً صغيراً واسرق أمواله",
                    ImageResource = "crime_type_five_number_two",
                    CourageCost = 4,
                    BaseSuccessChance = 82,
                    RequiredSuccesses = 13,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 900, CashRewardMax = 1800, ExperienceReward = 38
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 2,
                    Name = "تثبيت رجل أعمال",
                    Description = "أوقف رجل أعمال وانتزع حقيبته",
                    ImageResource = "crime_type_five_number_three",
                    CourageCost = 5,
                    BaseSuccessChance = 79,
                    RequiredSuccesses = 16,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1400, CashRewardMax = 2800, ExperienceReward = 48
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 3,
                    Name = "تثبيت مع طاقم",
                    Description = "نفذ عملية تثبيت جماعية مع فريق",
                    ImageResource = "crime_type_five_number_four",
                    CourageCost = 5,
                    BaseSuccessChance = 76,
                    RequiredSuccesses = 19,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2000, CashRewardMax = 4000, ExperienceReward = 58
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 4,
                    Name = "تثبيت مسؤول",
                    Description = "أوقف مسؤولاً حكومياً وانتزع وثائقه",
                    ImageResource = "crime_type_five_number_five",
                    CourageCost = 6,
                    BaseSuccessChance = 73,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3000, CashRewardMax = 6000, ExperienceReward = 70
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 3, CrimeItemId = 5,
                    Name = "تثبيت شخصية مشهورة",
                    Description = "استهدف شخصية معروفة وانتزع مقتنياتها",
                    ImageResource = "crime_type_five_number_six",
                    CourageCost = 6,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 25,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 4500, CashRewardMax = 9000, ExperienceReward = 85
                    }
                },
            }
        };
        _crimeTypes[3] = type3;

        // ========== TYPE 4: سرقة البيوت (5 tasks) ==========
        var type4 = new CrimeTypeDefinition
        {
            Id = 4,
            Name = "سرقة البيوت",
            Description = "اقتحام المنازل وسرقة محتوياتها",
            ImageResource = "crime_type_four",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 12000,
                ExperienceReward = 1200,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 4, CrimeItemId = 0,
                    Name = "سرقة شقة صغيرة",
                    Description = "اقتحم شقة صغيرة عبر النافذة",
                    ImageResource = "crime_type_four_number_one",
                    CourageCost = 5,
                    BaseSuccessChance = 82,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 800, CashRewardMax = 1600, ExperienceReward = 35
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 4, CrimeItemId = 1,
                    Name = "سرقة منزل عائلي",
                    Description = "تسلل ليلاً لمنزل عائلي وسرق الإلكترونيات",
                    ImageResource = "crime_type_four_number_two",
                    CourageCost = 5,
                    BaseSuccessChance = 78,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1400, CashRewardMax = 2800, ExperienceReward = 48
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 4, CrimeItemId = 2,
                    Name = "سرقة فيلا",
                    Description = "خطط واقتحم فيلا فاخرة",
                    ImageResource = "crime_type_four_number_three",
                    CourageCost = 6,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2200, CashRewardMax = 4400, ExperienceReward = 62
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 4, CrimeItemId = 3,
                    Name = "سرقة منزل الثري",
                    Description = "استهدف منزل ثري واكسر خزانته",
                    ImageResource = "crime_type_four_number_four",
                    CourageCost = 6,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 4 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3500, CashRewardMax = 7000, ExperienceReward = 80
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 4, CrimeItemId = 4,
                    Name = "سرقة القصر",
                    Description = "نفذ عملية معقدة لاقتحام قصر محروس",
                    ImageResource = "crime_type_four_number_five",
                    CourageCost = 7,
                    BaseSuccessChance = 66,
                    RequiredSuccesses = 26,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 5 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5500, CashRewardMax = 11000, ExperienceReward = 100
                    }
                },
            }
        };
        _crimeTypes[4] = type4;

        // ========== TYPE 5: سطو مسلح (4 tasks) ==========
        var type5 = new CrimeTypeDefinition
        {
            Id = 5,
            Name = "سطو مسلح",
            Description = "تنفيذ عمليات سطو مسلح على المحلات والبنوك",
            ImageResource = "crime_type_six",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 16000,
                ExperienceReward = 1600,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 5, CrimeItemId = 0,
                    Name = "سطو على محل صرافة",
                    Description = "سطو مسلح على محل صرافة",
                    ImageResource = "crime_type_six_number_one",
                    CourageCost = 7,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2000, CashRewardMax = 4000, ExperienceReward = 60
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 5, CrimeItemId = 1,
                    Name = "سطو على محل مجوهرات",
                    Description = "اقتحم محل مجوهرات بالقوة المسلحة",
                    ImageResource = "crime_type_six_number_two",
                    CourageCost = 8,
                    BaseSuccessChance = 75,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 4000, CashRewardMax = 8000, ExperienceReward = 85
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 5, CrimeItemId = 2,
                    Name = "سطو على بنك صغير",
                    Description = "نفذ سطواً على فرع بنكي صغير",
                    ImageResource = "crime_type_six_number_three",
                    CourageCost = 9,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 8000, CashRewardMax = 16000, ExperienceReward = 120
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 5, CrimeItemId = 3,
                    Name = "سطو على البنك الكبير",
                    Description = "نفذ أضخم عملية سطو مسلح على بنك رئيسي",
                    ImageResource = "crime_type_six_number_four",
                    CourageCost = 10,
                    BaseSuccessChance = 65,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 4 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 15000, CashRewardMax = 30000, ExperienceReward = 170
                    }
                },
            }
        };
        _crimeTypes[5] = type5;

        // ========== TYPE 6: تهريب (6 tasks) ==========
        var type6 = new CrimeTypeDefinition
        {
            Id = 6,
            Name = "تهريب",
            Description = "تهريب البضائع والأشخاص عبر الحدود",
            ImageResource = "crime_type_seven",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 20000,
                ExperienceReward = 2000,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 0,
                    Name = "تهريب سجائر",
                    Description = "هرب سجائر عبر نقطة تفتيش",
                    ImageResource = "crime_type_seven_number_one",
                    CourageCost = 6,
                    BaseSuccessChance = 83,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1200, CashRewardMax = 2400, ExperienceReward = 40
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 1,
                    Name = "تهريب كحول",
                    Description = "أخفِ مشروبات كحولية وهربها",
                    ImageResource = "crime_type_seven_number_two",
                    CourageCost = 6,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 13,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2000, CashRewardMax = 4000, ExperienceReward = 52
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 2,
                    Name = "تهريب إلكترونيات",
                    Description = "هرب أجهزة إلكترونية في حقائب مخفية",
                    ImageResource = "crime_type_seven_number_three",
                    CourageCost = 7,
                    BaseSuccessChance = 77,
                    RequiredSuccesses = 16,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3200, CashRewardMax = 6400, ExperienceReward = 65
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 3,
                    Name = "تهريب أسلحة خفيفة",
                    Description = "أخفِ أسلحة خفيفة وهربها عبر الحدود",
                    ImageResource = "crime_type_seven_number_four",
                    CourageCost = 7,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 19,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5000, CashRewardMax = 10000, ExperienceReward = 80
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 4,
                    Name = "تهريب مخدرات",
                    Description = "هرب مواد مخدرة في مخابئ مبتكرة",
                    ImageResource = "crime_type_seven_number_five",
                    CourageCost = 8,
                    BaseSuccessChance = 71,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 8000, CashRewardMax = 16000, ExperienceReward = 100
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 6, CrimeItemId = 5,
                    Name = "تهريب بشر",
                    Description = "نظم عملية تهريب بشر عبر الحدود",
                    ImageResource = "crime_type_seven_number_six",
                    CourageCost = 8,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 25,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 12000, CashRewardMax = 24000, ExperienceReward = 125
                    }
                },
            }
        };
        _crimeTypes[6] = type6;

        // ========== TYPE 7: زراعة فيروس (5 tasks) ==========
        var type7 = new CrimeTypeDefinition
        {
            Id = 7,
            Name = "زراعة فيروس",
            Description = "زرع الفيروسات والبرامج الخبيثة في الأجهزة",
            ImageResource = "crime_type_eight",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 25000,
                ExperienceReward = 2500,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 7, CrimeItemId = 0,
                    Name = "زرع فيروس بسيط",
                    Description = "زرع فيروس في جهاز شخصي",
                    ImageResource = "crime_type_eight_number_one",
                    CourageCost = 7,
                    BaseSuccessChance = 82,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1500, CashRewardMax = 3000, ExperienceReward = 50
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 7, CrimeItemId = 1,
                    Name = "زرع برنامج تجسس",
                    Description = "زرع برنامج تجسس في شبكة شركة",
                    ImageResource = "crime_type_eight_number_two",
                    CourageCost = 7,
                    BaseSuccessChance = 78,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 3 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3000, CashRewardMax = 6000, ExperienceReward = 70
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 7, CrimeItemId = 2,
                    Name = "زرع فيروس مصرفي",
                    Description = "زرع فيروس يسرق البيانات البنكية",
                    ImageResource = "crime_type_eight_number_three",
                    CourageCost = 8,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 5 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5000, CashRewardMax = 10000, ExperienceReward = 95
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 7, CrimeItemId = 3,
                    Name = "زرع برنامج فدية",
                    Description = "نشر برنامج فدية على شبكة حكومية",
                    ImageResource = "crime_type_eight_number_four",
                    CourageCost = 9,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 8 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 9000, CashRewardMax = 18000, ExperienceReward = 130
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 7, CrimeItemId = 4,
                    Name = "تدمير بنية تحتية",
                    Description = "تعطيل أنظمة بنية تحتية حيوية",
                    ImageResource = "crime_type_eight_number_five",
                    CourageCost = 10,
                    BaseSuccessChance = 66,
                    RequiredSuccesses = 26,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 10 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 15000, CashRewardMax = 30000, ExperienceReward = 175
                    }
                },
            }
        };
        _crimeTypes[7] = type7;

        // ========== TYPE 8: قتل واغتيال (4 tasks) ==========
        var type8 = new CrimeTypeDefinition
        {
            Id = 8,
            Name = "قتل واغتيال",
            Description = "تنفيذ عمليات اغتيال مأجورة",
            ImageResource = "crime_type_nine",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 30000,
                ExperienceReward = 3000,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 8, CrimeItemId = 0,
                    Name = "اغتيال مخبر",
                    Description = "تصفية مخبر يشكل خطراً",
                    ImageResource = "crime_type_nine_number_one",
                    CourageCost = 9,
                    BaseSuccessChance = 78,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5000, CashRewardMax = 10000, ExperienceReward = 100
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 8, CrimeItemId = 1,
                    Name = "اغتيال تاجر منافس",
                    Description = "تصفية منافس تجاري",
                    ImageResource = "crime_type_nine_number_two",
                    CourageCost = 10,
                    BaseSuccessChance = 73,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 10000, CashRewardMax = 20000, ExperienceReward = 145
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 8, CrimeItemId = 2,
                    Name = "اغتيال ضابط فاسد",
                    Description = "تصفية ضابط يعرقل العمليات",
                    ImageResource = "crime_type_nine_number_three",
                    CourageCost = 11,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 18000, CashRewardMax = 36000, ExperienceReward = 200
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 8, CrimeItemId = 3,
                    Name = "اغتيال مسؤول كبير",
                    Description = "تصفية مسؤول حكومي رفيع",
                    ImageResource = "crime_type_nine_number_four",
                    CourageCost = 12,
                    BaseSuccessChance = 63,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 30000, CashRewardMax = 60000, ExperienceReward = 270
                    }
                },
            }
        };
        _crimeTypes[8] = type8;

        // ========== TYPE 9: حرق منشآت (7 tasks) ==========
        var type9 = new CrimeTypeDefinition
        {
            Id = 9,
            Name = "حرق منشآت",
            Description = "إحراق المنشآت والمستودعات",
            ImageResource = "crime_type_ten",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 35000,
                ExperienceReward = 3500,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 0,
                    Name = "حرق سيارة",
                    Description = "أشعل النار في سيارة عدو",
                    ImageResource = "crime_type_ten_number_one",
                    CourageCost = 7,
                    BaseSuccessChance = 83,
                    RequiredSuccesses = 8,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 1800, CashRewardMax = 3600, ExperienceReward = 45
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 1,
                    Name = "حرق مستودع صغير",
                    Description = "أحرق مستودعاً صغيراً لمنافس",
                    ImageResource = "crime_type_ten_number_two",
                    CourageCost = 8,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 11,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3500, CashRewardMax = 7000, ExperienceReward = 62
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 2,
                    Name = "حرق محل تجاري",
                    Description = "أحرق محلاً تجارياً بالكامل",
                    ImageResource = "crime_type_ten_number_three",
                    CourageCost = 8,
                    BaseSuccessChance = 77,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5500, CashRewardMax = 11000, ExperienceReward = 80
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 3,
                    Name = "حرق مصنع",
                    Description = "أحرق مصنعاً كاملاً",
                    ImageResource = "crime_type_ten_number_four",
                    CourageCost = 9,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 17,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 8500, CashRewardMax = 17000, ExperienceReward = 102
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 4,
                    Name = "حرق مبنى إداري",
                    Description = "أحرق مبنى شركة أو إدارة",
                    ImageResource = "crime_type_ten_number_five",
                    CourageCost = 9,
                    BaseSuccessChance = 71,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 13000, CashRewardMax = 26000, ExperienceReward = 128
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 5,
                    Name = "حرق منشأة حكومية",
                    Description = "أحرق منشأة حكومية استراتيجية",
                    ImageResource = "crime_type_ten_number_six",
                    CourageCost = 10,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 23,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 20000, CashRewardMax = 40000, ExperienceReward = 160
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 9, CrimeItemId = 6,
                    Name = "حرق المنشأة الكبرى",
                    Description = "نفذ عملية إحراق ضخمة",
                    ImageResource = "crime_type_ten_number_seven",
                    CourageCost = 11,
                    BaseSuccessChance = 65,
                    RequiredSuccesses = 26,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 30000, CashRewardMax = 60000, ExperienceReward = 200
                    }
                },
            }
        };
        _crimeTypes[9] = type9;

        // ========== TYPE 10: سرقة سيارات (3 tasks) ==========
        var type10 = new CrimeTypeDefinition
        {
            Id = 10,
            Name = "سرقة سيارات",
            Description = "سرقة السيارات من الشوارع والمواقف",
            ImageResource = "crime_type_eleven",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 40000,
                ExperienceReward = 4000,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 10, CrimeItemId = 0,
                    Name = "سرقة سيارة عادية",
                    Description = "اسرق سيارة من الشارع",
                    ImageResource = "crime_type_eleven_number_one",
                    CourageCost = 8,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 12,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 3000, CashRewardMax = 6000, ExperienceReward = 80
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 10, CrimeItemId = 1,
                    Name = "سرقة سيارة فاخرة",
                    Description = "اسرق سيارة فاخرة من موقف مراقب",
                    ImageResource = "crime_type_eleven_number_two",
                    CourageCost = 9,
                    BaseSuccessChance = 74,
                    RequiredSuccesses = 17,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 8000, CashRewardMax = 16000, ExperienceReward = 120
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 10, CrimeItemId = 2,
                    Name = "سرقة شاحنة مال",
                    Description = "استولِ على شاحنة نقل أموال",
                    ImageResource = "crime_type_eleven_number_three",
                    CourageCost = 10,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_lockpick", ToolName = "مفك قفل", RequiredCount = 4 },
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 18000, CashRewardMax = 36000, ExperienceReward = 175
                    }
                },
            }
        };
        _crimeTypes[10] = type10;

        // ========== TYPE 11: تجارة اثار (2 tasks) ==========
        var type11 = new CrimeTypeDefinition
        {
            Id = 11,
            Name = "تجارة اثار",
            Description = "الاتجار بالقطع الأثرية المهربة",
            ImageResource = "crime_type_twelve",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 45000,
                ExperienceReward = 4500,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 11, CrimeItemId = 0,
                    Name = "بيع قطعة أثرية",
                    Description = "تفاوض سراً على بيع قطعة أثرية نادرة",
                    ImageResource = "crime_type_twelve_number_one",
                    CourageCost = 9,
                    BaseSuccessChance = 78,
                    RequiredSuccesses = 15,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 8000, CashRewardMax = 16000, ExperienceReward = 130
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 11, CrimeItemId = 1,
                    Name = "بيع كنز أثري",
                    Description = "أتمم صفقة كنز أثري نادر مع مقتنٍ دولي",
                    ImageResource = "crime_type_twelve_number_two",
                    CourageCost = 10,
                    BaseSuccessChance = 72,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 20000, CashRewardMax = 40000, ExperienceReward = 200
                    }
                },
            }
        };
        _crimeTypes[11] = type11;

        // ========== TYPE 12: تزوير (6 tasks) ==========
        var type12 = new CrimeTypeDefinition
        {
            Id = 12,
            Name = "تزوير",
            Description = "تزوير الوثائق والعملات والهويات",
            ImageResource = "crime_type_thirteen",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 50000,
                ExperienceReward = 5000,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 0,
                    Name = "تزوير هوية",
                    Description = "اطبع بطاقة هوية مزيفة لزبون",
                    ImageResource = "crime_type_thirteen_number_one",
                    CourageCost = 8,
                    BaseSuccessChance = 84,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 2500, CashRewardMax = 5000, ExperienceReward = 65
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 1,
                    Name = "تزوير رخصة قيادة",
                    Description = "صنع رخصة قيادة مزيفة احترافية",
                    ImageResource = "crime_type_thirteen_number_two",
                    CourageCost = 9,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 14,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 3 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 4500, CashRewardMax = 9000, ExperienceReward = 90
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 2,
                    Name = "تزوير جواز سفر",
                    Description = "اصنع جواز سفر مزيف بطوابع حقيقية",
                    ImageResource = "crime_type_thirteen_number_three",
                    CourageCost = 9,
                    BaseSuccessChance = 76,
                    RequiredSuccesses = 18,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 5 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 7500, CashRewardMax = 15000, ExperienceReward = 120
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 3,
                    Name = "تزوير عملة",
                    Description = "اطبع أوراقاً نقدية لا تختلف عن الأصلية",
                    ImageResource = "crime_type_thirteen_number_four",
                    CourageCost = 10,
                    BaseSuccessChance = 72,
                    RequiredSuccesses = 22,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 8 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 12000, CashRewardMax = 24000, ExperienceReward = 160
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 4,
                    Name = "تزوير عقود رسمية",
                    Description = "زور عقوداً وسنداً ملكية قانونية",
                    ImageResource = "crime_type_thirteen_number_five",
                    CourageCost = 10,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 26,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 8 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 19000, CashRewardMax = 38000, ExperienceReward = 210
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 12, CrimeItemId = 5,
                    Name = "تزوير أوراق حكومية",
                    Description = "زور وثائق حكومية سرية",
                    ImageResource = "crime_type_thirteen_number_six",
                    CourageCost = 11,
                    BaseSuccessChance = 64,
                    RequiredSuccesses = 30,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 10 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 28000, CashRewardMax = 56000, ExperienceReward = 270
                    }
                },
            }
        };
        _crimeTypes[12] = type12;

        // ========== TYPE 13: حراسة مجرمين (4 tasks) ==========
        var type13 = new CrimeTypeDefinition
        {
            Id = 13,
            Name = "حراسة مجرمين",
            Description = "تقديم الحماية لأشخاص مطلوبين للقانون",
            ImageResource = "crime_type_fourteen",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 55000,
                ExperienceReward = 5500,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 13, CrimeItemId = 0,
                    Name = "حراسة تاجر مشبوه",
                    Description = "احرس تاجراً مشبوهاً من منافسيه",
                    ImageResource = "crime_type_fourteen_number_one",
                    CourageCost = 9,
                    BaseSuccessChance = 80,
                    RequiredSuccesses = 10,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 5000, CashRewardMax = 10000, ExperienceReward = 100
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 13, CrimeItemId = 1,
                    Name = "حراسة قائد عصابة",
                    Description = "وفر الحماية لقائد عصابة صغيرة",
                    ImageResource = "crime_type_fourteen_number_two",
                    CourageCost = 10,
                    BaseSuccessChance = 75,
                    RequiredSuccesses = 15,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 10000, CashRewardMax = 20000, ExperienceReward = 145
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 13, CrimeItemId = 2,
                    Name = "حراسة مطلوب كبير",
                    Description = "احرس مجرماً مطلوباً دولياً",
                    ImageResource = "crime_type_fourteen_number_three",
                    CourageCost = 11,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 18000, CashRewardMax = 36000, ExperienceReward = 200
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 13, CrimeItemId = 3,
                    Name = "حراسة زعيم المافيا",
                    Description = "كن الحارس الشخصي لزعيم مافيا",
                    ImageResource = "crime_type_fourteen_number_four",
                    CourageCost = 12,
                    BaseSuccessChance = 65,
                    RequiredSuccesses = 25,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 30000, CashRewardMax = 60000, ExperienceReward = 270
                    }
                },
            }
        };
        _crimeTypes[13] = type13;

        // ========== TYPE 14: تجارة سلاح (2 tasks) ==========
        var type14 = new CrimeTypeDefinition
        {
            Id = 14,
            Name = "تجارة سلاح",
            Description = "بيع وتهريب الأسلحة",
            ImageResource = "crime_type_fifteen",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 60000,
                ExperienceReward = 6000,
                ItemRewards = new List<CrimeItemReward>
                {
                    new CrimeItemReward { ItemId = "weapon_pistol", ItemName = "مسدس غير مرخص", ImageResource = "item_gun", MinCount = 1, MaxCount = 1, DropChance = 100 }
                }
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 14, CrimeItemId = 0,
                    Name = "بيع أسلحة خفيفة",
                    Description = "بع أسلحة خفيفة لتجار السلاح",
                    ImageResource = "crime_type_fifteen_number_one",
                    CourageCost = 11,
                    BaseSuccessChance = 78,
                    RequiredSuccesses = 15,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 12000, CashRewardMax = 24000, ExperienceReward = 180
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 14, CrimeItemId = 1,
                    Name = "بيع أسلحة ثقيلة",
                    Description = "هرب أسلحة ثقيلة لجماعة مسلحة",
                    ImageResource = "crime_type_fifteen_number_two",
                    CourageCost = 12,
                    BaseSuccessChance = 70,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 25000, CashRewardMax = 50000, ExperienceReward = 260
                    }
                },
            }
        };
        _crimeTypes[14] = type14;

        // ========== TYPE 15: تفجير (2 tasks) ==========
        var type15 = new CrimeTypeDefinition
        {
            Id = 15,
            Name = "تفجير",
            Description = "زرع المتفجرات وتنفيذ عمليات التفجير",
            ImageResource = "crime_type_sixteen",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 65000,
                ExperienceReward = 6500,
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 15, CrimeItemId = 0,
                    Name = "تفجير سيارة",
                    Description = "زرع عبوة ناسفة في سيارة هدف",
                    ImageResource = "crime_type_sixteen_number_one",
                    CourageCost = 12,
                    BaseSuccessChance = 75,
                    RequiredSuccesses = 15,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 2 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 1 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 15000, CashRewardMax = 30000, ExperienceReward = 220
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 15, CrimeItemId = 1,
                    Name = "تفجير منشأة حيوية",
                    Description = "فجر منشأة استراتيجية بعملية محكمة",
                    ImageResource = "crime_type_sixteen_number_two",
                    CourageCost = 13,
                    BaseSuccessChance = 68,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_gloves", ToolName = "قفازات", RequiredCount = 3 },
                        new CrimeToolRequirement { ToolItemId = "tool_mask", ToolName = "قناع", RequiredCount = 2 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 35000, CashRewardMax = 70000, ExperienceReward = 320
                    }
                },
            }
        };
        _crimeTypes[15] = type15;

        // ========== TYPE 16: تهكير واستيلاء (2 tasks) ==========
        var type16 = new CrimeTypeDefinition
        {
            Id = 16,
            Name = "تهكير واستيلاء",
            Description = "اختراق الأنظمة والاستيلاء على الأموال",
            ImageResource = "crime_type_seventeen",
            TypeCompletionReward = new CrimeCompletionReward
            {
                CashReward = 75000,
                ExperienceReward = 7500,
                ItemRewards = new List<CrimeItemReward>
                {
                    new CrimeItemReward { ItemId = "special_hack_tool", ItemName = "أداة اختراق متطورة", ImageResource = "item_hack_tool", MinCount = 1, MaxCount = 1, DropChance = 100 }
                }
            },
            Crimes = new List<CrimeItemDefinition>
            {
                new CrimeItemDefinition {
                    CrimeTypeId = 16, CrimeItemId = 0,
                    Name = "اختراق شركة صغيرة",
                    Description = "اختراق نظام شركة صغيرة والاستيلاء على أموالها",
                    ImageResource = "crime_type_seventeen_number_one",
                    CourageCost = 13,
                    BaseSuccessChance = 75,
                    RequiredSuccesses = 15,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 5 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 18000, CashRewardMax = 36000, ExperienceReward = 250
                    }
                },
                new CrimeItemDefinition {
                    CrimeTypeId = 16, CrimeItemId = 1,
                    Name = "اختراق البنك المركزي",
                    Description = "اختراق النظام المالي للبنك المركزي",
                    ImageResource = "crime_type_seventeen_number_two",
                    CourageCost = 14,
                    BaseSuccessChance = 65,
                    RequiredSuccesses = 20,
                    ToolRequirements = new List<CrimeToolRequirement> {
                        new CrimeToolRequirement { ToolItemId = "tool_blank_cd", ToolName = "اسطوانة فارغة", RequiredCount = 10 },
                    },
                    Reward = new CrimeReward {
                        CashRewardMin = 40000, CashRewardMax = 80000, ExperienceReward = 380
                    }
                },
            }
        };
        _crimeTypes[16] = type16;

    }
}