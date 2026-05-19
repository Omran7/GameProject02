using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services;

public static class SkillService
{
    private const string ProjectId = "gameproject02-4207f";
    private const string ApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
    private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
    private static readonly HttpClient _client = new();

    public static void InitializeDefaultSkills(PlayerAccount player)
    {
        if (player.Skills.Count == 0)
        {
            foreach (var def in SkillDatabase.AllSkills)
                player.Skills.Add(new Skill { Id = def.Id, Name = def.Name, Level = 0, IsEquipped = false });
        }
    }

    // ✅ FIXED: Accept 3 arguments as UI expects
    public static (bool success, string message) UpgradeSkill(PlayerAccount player, int skillId, int points)
    {
        if (points <= 0) return (false, "عدد النقاط غير صالح");

        var skill = player.Skills.FirstOrDefault(s => s.Id == skillId);
        var def = SkillDatabase.GetSkill(skillId);
        if (skill == null || def == null) return (false, "المهارة غير موجودة");
        if (skill.Level >= 20) return (false, "الحد الأقصى للمستوى 20!");

        int cost = skill.GetUpgradeCost();
        if (player.Merits < cost)
            return (false, $"تحتاج {cost} استحقاق للترقية. لديك {player.Merits}.");

        player.Merits -= cost;
        skill.Level += points;
        skill.IsEquipped = true;

        AccountService.SavePlayer(player);
        return (true, $"تم ترقية {def.Name} إلى المستوى {skill.Level}!");
    }

    public static void ToggleEquip(PlayerAccount player, int skillId)
    {
        var skill = player.Skills.FirstOrDefault(s => s.Id == skillId);
        if (skill != null) { skill.IsEquipped = !skill.IsEquipped; AccountService.SavePlayer(player); }
    }

    // ✅ ADDED: Missing method
    public static async Task<bool> SaveSkillsToFirestoreAsync(string playerId, List<Skill> skills)
    {
        try
        {
            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths=skills";
            var skillsArray = skills.Select(s => new
            {
                mapValue = new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["id"] = new { integerValue = s.Id },
                        ["level"] = new { integerValue = s.Level },
                        ["isEquipped"] = new { booleanValue = s.IsEquipped },
                        ["name"] = new { stringValue = s.Name }
                    }
                }
            }).ToArray();

            var doc = new { fields = new { skills = new { arrayValue = new { values = skillsArray } } } };
            var json = JsonSerializer.Serialize(doc);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SKILL SAVE] Error: {ex.Message}");
            return false;
        }
    }

    // ✅ ADDED: Helper for parsing
    public static List<Skill> ParseSkillsFromFirestore(JsonElement skillsElement)
    {
        var skills = new List<Skill>();
        try
        {
            if (skillsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in skillsElement.EnumerateArray())
                {
                    if (item.TryGetProperty("mapValue", out var map) && map.TryGetProperty("fields", out var fields))
                    {
                        skills.Add(new Skill
                        {
                            Id = GetInt(fields, "id"),
                            Level = GetInt(fields, "level"),
                            IsEquipped = GetBool(fields, "isEquipped"),
                            Name = GetString(fields, "name") ?? ""
                        });
                    }
                }
            }
        }
        catch { }
        return skills;
    }

    private static string GetString(JsonElement f, string key) =>
        f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v) ? v.GetString() : null;

    private static int GetInt(JsonElement f, string key) =>
        f.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v) ? v.GetInt32() : 0;

    private static bool GetBool(JsonElement f, string key) =>
        f.TryGetProperty(key, out var p) && p.TryGetProperty("booleanValue", out var v) ? v.GetBoolean() : false;
}