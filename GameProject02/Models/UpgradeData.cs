namespace GameProject02.Models;

public class EstateUpgradeItem
{
    public string Name { get; set; } = string.Empty;
    public int Cost { get; set; }
    public int Happiness { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Upgrade"; // "Upgrade" or "Contract"
    public bool IsPurchased { get; set; } = false;
    public long? ContractStartTime { get; set; } = null; // For contracts only
}

public static class EstateUpgradesDatabase
{
    // ✅ COMPLETE UPGRADE & CONTRACT DATA FROM EXCEL FILE
    public static Dictionary<int, List<EstateUpgradeItem>> EstateUpgrades = new()
    {
        // بدروم (ID 1)
        { 1, new List<EstateUpgradeItem>
            {
                new() { Name = "ادوات بدائية", Cost = 180, Happiness = 10, Description = " ابحث عن الافضل لك", Type = "Upgrade" }
            }
        },
        
        // شقة (ID 2)
        { 2, new List<EstateUpgradeItem>
            {
                new() { Name = "ادوات صيد", Cost = 900, Happiness = 11, Description = "صيد الحيوانات ممتع قليلا", Type = "Upgrade" }
            }
        },
        
        // بيت بالضواحي (ID 3)
        { 3, new List<EstateUpgradeItem>
            {
                new() { Name = "صندوق خشبي", Cost = 1800, Happiness = 13, Description = "حفظ الاشياء المهمة داخل البيت", Type = "Upgrade" },
                new() { Name = "ادوات زراعية", Cost = 450, Happiness = 50, Description = "عمل شاق قليلا لكن فيه متعة", Type = "Contract" }
            }
        },
        
        // شاليه صغير (ID 4)
        { 4, new List<EstateUpgradeItem>
            {
                new() { Name = "عفش جديد", Cost = 4500, Happiness = 18, Description = "يصبح منزلنا اكثر جمالا", Type = "Upgrade" },
                new() { Name = "دهان للبيت", Cost = 4500, Happiness = 18, Description = "اضائة افضل والون جذابة اكثر", Type = "Upgrade" },
                new() { Name = "ادوات زراعية", Cost = 900, Happiness = 50, Description = "هذه الادوات تنتهي ولكن ممتازة", Type = "Contract" },
                new() { Name = "فلاح مساعد", Cost = 1800, Happiness = 100, Description = "يبدو هذا الشخص جيد ويعمل جيدا", Type = "Contract" }
            }
        },
        
        // بيت منعزل (ID 5)
        { 5, new List<EstateUpgradeItem>
            {
                new() { Name = "درج  للبيت", Cost = 90000, Happiness = 60, Description = "الدرج هذا افضل من التسلق", Type = "Upgrade" },
                new() { Name = "سقف جديد", Cost = 90000, Happiness = 60, Description = "لن تدخل المياه من المطر مجددا", Type = "Upgrade" },
                new() { Name = "خبير زراعه", Cost = 3600, Happiness = 100, Description = "الزراعه اصبحت تعطي مردود افضل", Type = "Contract" },
                new() { Name = "فلاح ماهر", Cost = 5400, Happiness = 150, Description = "العمل شاق ولكن هذا الشاب جيد", Type = "Contract" }
            }
        },
        
        // فيلا (ID 6)
        { 6, new List<EstateUpgradeItem>
            {
                new() { Name = "ازالت الصخور", Cost = 270000, Happiness = 70, Description = "مساحه افضل لعمل اكثر", Type = "Upgrade" },
                new() { Name = "قص الاشجار", Cost = 270000, Happiness = 70, Description = "مردود الخشب جيد والمساحة تكبر", Type = "Upgrade" },
                new() { Name = "بذور جديدة", Cost = 7200, Happiness = 150, Description = "احب الزراعه واحب الاصناف الجديدة", Type = "Contract" },
                new() { Name = "مجموعة فلاحين", Cost = 9000, Happiness = 200, Description = "فلاحين اكثر اموال اكثر ", Type = "Contract" }
            }
        },
        
        // مزرعة (ID 7)
        { 7, new List<EstateUpgradeItem>
            {
                new() { Name = "سور خشبي", Cost = 450000, Happiness = 85, Description = "لتحيط مزرعتك بالكامل ومعرفة حدودها", Type = "Upgrade" },
                new() { Name = "مستودع حبوب", Cost = 450000, Happiness = 85, Description = "مستودع لتخزين الحبوب في مزرعتك", Type = "Upgrade" },
                new() { Name = "مساعد عام", Cost = 18000, Happiness = 200, Description = "يقوم بكثير من الاعمال انه رائع", Type = "Contract" },
                new() { Name = "موظفي الحقول", Cost = 27000, Happiness = 250, Description = "الكثير من العمال والمال يأتي", Type = "Contract" }
            }
        },
        
        // بيت على الشاطئ (ID 8)
        { 8, new List<EstateUpgradeItem>
            {
                new() { Name = "احواض زهور", Cost = 540000, Happiness = 90, Description = "منظر اكثر جاذبية ومنزل اجمل", Type = "Upgrade" },
                new() { Name = "مسبح صغير", Cost = 540000, Happiness = 90, Description = "من الجيد امتلاك مثل هذا المسبح", Type = "Upgrade" },
                new() { Name = "معلم السباحه", Cost = 36000, Happiness = 250, Description = "تعلم السباحه معه بشكل جيد", Type = "Contract" },
                new() { Name = "منقذ", Cost = 45000, Happiness = 300, Description = "للحفاظ على حياتك وحياة افراد العائلة", Type = "Contract" }
            }
        },
        
        // بيت بفناء واسع (ID 9)
        { 9, new List<EstateUpgradeItem>
            {
                new() { Name = "غرف اضافيه", Cost = 720000, Happiness = 100, Description = "منزل اكبر وغرف اكثر ", Type = "Upgrade" },
                new() { Name = "تحف فنيه", Cost = 720000, Happiness = 100, Description = "تجميل الفيلا لتبدو اكثر ثراء", Type = "Upgrade" },
                new() { Name = "حراس البوابة", Cost = 90000, Happiness = 300, Description = "الامن مطلوب للحماية من اللصوص", Type = "Contract" },
                new() { Name = "كلاب حراسة", Cost = 180000, Happiness = 350, Description = "تساعد الحراس اكثر في حراسة المكان", Type = "Contract" }
            }
        },
        
        // منطقة (ID 10)
        /*{ 10, new List<EstateUpgradeItem>+
         * -
            {
                new() { Name = "كشافات اضاءة", Cost = 810000, Happiness = 110, Description = "كشافات الاضاءة تساعدك على التمرين في الليل", Type = "Upgrade" },
                new() { Name = "لوحة نتائج", Cost = 810000, Happiness = 110, Description = "لمتابعة نتيجة المباراة بطريقة احترافية", Type = "Upgrade" },
                new() { Name = "مدرب سلة", Cost = 180000, Happiness = 350, Description = "مساعد لتسهيل حياتك مقابل اجر كل اسبوع", Type = "Contract" },
                new() { Name = "جامع كرات", Cost = 360000, Happiness = 400, Description = "مساعد لتسهيل حياتك مقابل اجر كل اسبوع", Type = "Contract" }
            }
        },*/
        
        // برج (ID 11)
        { 10, new List<EstateUpgradeItem>
            {
                new() { Name = "نوافذ زجاجية", Cost = 900000, Happiness = 135, Description = "تغيير شكل القلعة من الخارج", Type = "Upgrade" },
                new() { Name = "سور خشبي", Cost = 900000, Happiness = 135, Description = "حماية اكثر ومساحة محددة اكثر", Type = "Upgrade" },
                new() { Name = "ابراج حراسة", Cost = 270000, Happiness = 350, Description = "لايمكن لاحد تجاوز السور ", Type = "Contract" },
                new() { Name = "حراس الابراج", Cost = 360000, Happiness = 400, Description = "هؤلاء الحراس لا ينامون ابدا", Type = "Contract" }
            }
        },
        
        // قصر (ID 12)
        { 11, new List<EstateUpgradeItem>
            {
                new() { Name = "طوابق اضافيه", Cost = 1800000, Happiness = 210, Description = "المساحه الاكبر افضل بكثير", Type = "Upgrade" },
                new() { Name = "جدران مزخرفة", Cost = 1800000, Happiness = 210, Description = "زخرفة المكان بالكامل يالروعة", Type = "Upgrade" },
                new() { Name = "مجموعة فلاحين", Cost = 450000, Happiness = 400, Description = "يعملون كثيران انهم جيدون", Type = "Contract" },
                new() { Name = "مراقب الفلاحين", Cost = 540000, Happiness = 450, Description = "لا تساهل بالعمل مع المراقب", Type = "Contract" }
            }
        },
        
        // قلعة (ID 13)
        { 12, new List<EstateUpgradeItem>
            {
                new() { Name = "برج للقصر", Cost = 4500000, Happiness = 322, Description = "اطلالة رائعة من الاعلى", Type = "Upgrade" },
                new() { Name = "اثاث فخم", Cost = 4500000, Happiness = 322, Description = "يصبح المكان اكثر جمالا", Type = "Upgrade" },
                new() { Name = "حراس قصر", Cost = 630000, Happiness = 450, Description = "لحراسة المكان من كل خطر", Type = "Contract" },
                new() { Name = "بعض الخدم", Cost = 720000, Happiness = 500, Description = "عيشة تشبه الملوك", Type = "Contract" }
            }
        },
        
        // جزيرة (ID 14)
        { 13, new List<EstateUpgradeItem>
            {
                new() { Name = "مشاعل نارية", Cost = 9000000, Happiness = 410, Description = "انارة القلعه بالكامل", Type = "Upgrade" },
                new() { Name = "رايات", Cost = 9000000, Happiness = 410, Description = "راية الحرب والسلم", Type = "Upgrade" },
                new() { Name = "قطار صغير", Cost = 100000000, Happiness = 410, Description = "مهم جدا لنقل البضائع", Type = "Upgrade" },
                new() { Name = "سائق قطار", Cost = 810000, Happiness = 500, Description = "سائق مبتدء لكنه يعمل", Type = "Contract" },
                new() { Name = "عامل تنظيف", Cost = 900000, Happiness = 600, Description = "تنظيف القطار باستمرار", Type = "Contract" }
            }
        },
        
        // محطة (ID 15)
        { 14, new List<EstateUpgradeItem>
            {
                new() { Name = "مجموعة خدم", Cost = 13500000, Happiness = 450, Description = "القيام باعمال القصر بالكامل", Type = "Upgrade" },
                new() { Name = "جيش صغير", Cost = 13500000, Happiness = 450, Description = "اصبح لدينا جيش صغير وقوي", Type = "Upgrade" },
                new() { Name = "قطار متوسط", Cost = 100000000, Happiness = 100, Description = "سرعه ممتازة وعربات اكثر", Type = "Upgrade" },
                new() { Name = "سائق خبير", Cost = 1400000, Happiness = 550, Description = "اسرع واكثر خبرة بالقيادة", Type = "Contract" },
                new() { Name = "عمال للقطار", Cost = 2300000, Happiness = 650, Description = "عمل اكثر وجهد اكبر", Type = "Contract" }
            }
        },
        
        // نطاق خاص (ID 16)
        { 15, new List<EstateUpgradeItem>
            {
                new() { Name = "مدفن", Cost = 13500000, Happiness = 500, Description = "الراحة الابدية بهذا المدفن", Type = "Upgrade" },
                new() { Name = "ابو الهول", Cost = 13500000, Happiness = 500, Description = "تخليد ذكراك الى الأبد", Type = "Upgrade" },
                new() { Name = "هرم صغير", Cost = 100000000, Happiness = 100, Description = "هذا الهرم له فوائد عظيمة", Type = "Upgrade" },
                new() { Name = "جيش عظيم", Cost = 1400000, Happiness = 600, Description = "انا الاقوى على الاطلاق", Type = "Contract" },
                new() { Name = "معارك اسبوعية", Cost = 2300000, Happiness = 700, Description = "لنسيطر على كامل الارض", Type = "Contract" }
            }
        }
    };
}