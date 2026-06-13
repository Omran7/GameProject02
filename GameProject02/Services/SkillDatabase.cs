using GameProject02.Models;
using System;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class SkillDatabase
{
    public static List<SkillDefinition> AllSkills => new()
    {
        new(0,  "حقيبة",           "زيادة سعة المخزن",          SkillCategory.Utility,      s => 1.0 + (s.Level * 0.10)),
        new(1,  "القوة الشاملة",     "زيادة جميع الإحصائيات",     SkillCategory.Combat,       s => 1.0 + (s.Level * 0.05)),
        new(2,  "صعب القتل",         "زيادة الحد الأقصى للصحة",   SkillCategory.Combat,       s => 1.0 + (s.Level * 0.08)),
        new(3,  "سرعة خارقة",       "زيادة السرعة",              SkillCategory.Combat,       s => 1.0 + (s.Level * 0.10)),
        new(4,  "خطوات الظل",       "زيادة نسبة الهروب/النجاح",  SkillCategory.Crime,        s => 1.0 + (s.Level * 0.03)),
        new(5,  "خبير الأسلحة",     "زيادة ضرر الأسلحة البيضاء", SkillCategory.Combat,       s => 1.0 + (s.Level * 0.12)),
        new(6,  "خبير الدروع",      "زيادة الدفاع",              SkillCategory.Combat,       s => 1.0 + (s.Level * 0.10)),
        new(7,  "ضربة حرجة",        "زيادة فرصة الضربة الحرجة",  SkillCategory.Combat,       s => Math.Min(0.50, s.Level * 0.05)),
        new(8,  "تعافي سريع",       "تقليل وقت المستشفى",        SkillCategory.Utility,      s => Math.Max(0.50, 1.0 - (s.Level * 0.08))),
        new(9,  "لا رحمة",          "زيادة ضرر القتال",          SkillCategory.Combat,       s => 1.0 + (s.Level * 0.07)),
        new(10, "لص محترف",         "زيادة ذهب الجرائم",         SkillCategory.Crime,        s => 1.0 + (s.Level * 0.15)),
        new(11, "رخصة قتل",         "كفاءة الأسلحة النارية",     SkillCategory.Combat,       s => 1.0 + (s.Level * 0.12)),
        new(12, "نشيط",             "زيادة الطاقة القصوى",       SkillCategory.Regeneration, s => 1.0 + (s.Level * 0.08)),
        new(13, "لا يخاف",          "تقليل تكلفة الشجاعة",       SkillCategory.Crime,        s => Math.Max(0.60, 1.0 - (s.Level * 0.06))),
        new(14, "طويل العمر",      "تسريع تجديد الطاقة/الشجاعة",SkillCategory.Regeneration, s => 1.0 + (s.Level * 0.10)),
        new(15, "خبير",             "زيادة الخبرة المكتسبة",     SkillCategory.Utility,      s => 1.0 + (s.Level * 0.12)),
        new(16, "محظوظ",            "زيادة فرص الغنائم النادرة", SkillCategory.Crime,        s => 1.0 + (s.Level * 0.08)),
        new(17, "مسافر دائم",      "تقليل وقت/تكلفة السفر",     SkillCategory.Utility,      s => Math.Max(0.50, 1.0 - (s.Level * 0.10))),
        new(18, "ساوم صعب",         "تخفيض أسعار السوق",         SkillCategory.Economy,      s => Math.Max(0.70, 1.0 - (s.Level * 0.05))),
        new(19, "صانع المال",       "زيادة دخل العمل/التجديد",   SkillCategory.Economy,      s => 1.0 + (s.Level * 0.10)),
        new(20, "موظف مميز",       "كفاءة الدراسة/العمل",       SkillCategory.Economy,      s => 1.0 + (s.Level * 0.12)),
        new(21, "عبقري",            "سرعة إتمام الدراسة",        SkillCategory.Economy,      s => Math.Max(0.50, 1.0 - (s.Level * 0.10))),
        new(22, "الغنيمة غنيمته",   "زيادة عدد الغنائم",         SkillCategory.Crime,        s => 1 + (s.Level * 0.2))
    };

    public static SkillDefinition GetSkill(int id) => AllSkills.Find(s => s.Id == id) ?? new(0, "Unknown", "", SkillCategory.Utility, _ => 1.0);
}

public class SkillDefinition
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public SkillCategory Category { get; }
    public Func<Skill, double> Multiplier { get; }

    public SkillDefinition(int id, string name, string desc, SkillCategory cat, Func<Skill, double> mult)
    {
        Id = id; Name = name; Description = desc; Category = cat; Multiplier = mult;
    }
}

public enum SkillCategory { Combat, Crime, Economy, Utility, Regeneration }