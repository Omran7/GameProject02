using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class GangDatabaseService
{
    // ✅ SHARED STORAGE (SIMULATES FIREBASE FOR TESTING)
    private static readonly Dictionary<string, GangObject> AllGangs = new();
    private static readonly List<GangJoinRequest> JoinRequests = new();

    // ✅ CREATE GANG
    public static GangObject CreateGang(GangObject gang)
    {
        if (AllGangs.ContainsKey(gang.GangId)) return null;
        AllGangs[gang.GangId] = gang;
        return gang;
    }

    // ✅ SEARCH GANGS
    public static List<GangObject> SearchGangs(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return AllGangs.Values.ToList();
        string q = query.ToLower().Trim();
        return AllGangs.Values.Where(g =>
            g.Name.ToLower().Contains(q) ||
            g.Tag.ToLower().Contains(q) ||
            g.GangId.ToLower().Contains(q)
        ).ToList();
    }

    // ✅ SEND JOIN REQUEST
    public static bool SendJoinRequest(string gangId, string playerId, string playerName)
    {
        if (AllGangs.TryGetValue(gangId, out var gang))
        {
            if (gang.MembersWithPositions.ContainsKey(playerId)) return false;
            if (JoinRequests.Any(r => r.GangId == gangId && r.PlayerId == playerId)) return false;

            JoinRequests.Add(new GangJoinRequest
            {
                GangId = gangId,
                PlayerId = playerId,
                PlayerName = playerName,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            return true;
        }
        return false;
    }

    // ✅ GET PENDING REQUESTS
    public static List<GangJoinRequest> GetJoinRequests(string gangId) =>
        JoinRequests.Where(r => r.GangId == gangId).ToList();

    // ✅ ACCEPT/REJECT REQUEST
    public static bool ProcessJoinRequest(string gangId, string playerId, bool accept)
    {
        var req = JoinRequests.FirstOrDefault(r => r.GangId == gangId && r.PlayerId == playerId);
        if (req == null) return false;

        JoinRequests.Remove(req);

        if (accept && AllGangs.TryGetValue(gangId, out var gang))
        {
            if (gang.MembersWithPositions.Count >= 50) return false;

            gang.MembersWithPositions[playerId] = GangPosition.Member;

            var joiningPlayer = AccountService.GetPlayerById(playerId);
            if (joiningPlayer != null)
            {
                if (joiningPlayer.GangObject != null && joiningPlayer.GangObject.GangId != gangId)
                {
                    joiningPlayer.GangObject.MembersWithPositions.Remove(playerId);
                }
                joiningPlayer.GangObject = gang;
                AccountService.SavePlayer(joiningPlayer);
            }
            return true;
        }
        return false;
    }

    // ✅ PROMOTE/KICK MEMBER
    public static bool UpdateMemberPosition(string gangId, string targetId, GangPosition? newPosition)
    {
        if (!AllGangs.TryGetValue(gangId, out var gang)) return false;
        if (!gang.MembersWithPositions.ContainsKey(targetId)) return false;

        if (newPosition.HasValue)
            gang.MembersWithPositions[targetId] = newPosition.Value;
        else
            gang.MembersWithPositions.Remove(targetId);

        return true;
    }

    // ✅ JOIN MILITIA (WITH COURAGE DEDUCTION & REWARDS: RESPECT, LOYALTY, CRYSTAL)
    public static JoinGangMilitiaResultObject JoinMilitia(string playerId, GangObject gang, int unitId, int requiredCourage, int respectReward)
    {
        var result = new JoinGangMilitiaResultObject();
        if (gang == null)
        {
            result.HasError = true;
            result.ErrorMessage = "بيانات العصابة فارغة";
            return result;
        }

        // ✅ منع الانضمام إلى أكثر من وحدة
        foreach (var unit in gang.MilitiaMembersByUnit)
        {
            if (unit.Value.Contains(playerId))
            {
                result.HasError = true;
                result.IsPlayerAlreadyJoined = true;
                result.ErrorMessage = $"أنت بالفعل عضو في الميليشيا {unit.Key}. لا يمكنك الانضمام إلى أكثر من وحدة.";
                return result;
            }
        }

        // التأكد من وجود الوحدة
        if (!gang.MilitiaMembersByUnit.ContainsKey(unitId))
            gang.MilitiaMembersByUnit[unitId] = new List<string>();

        var membersList = gang.MilitiaMembersByUnit[unitId];
        int maxMembers = unitId * 10;

        // التحقق من الامتلاء قبل الإضافة
        bool wasFullBeforeJoin = membersList.Count >= maxMembers;
        if (wasFullBeforeJoin)
        {
            long bonusRespect = respectReward * 2;
            gang.AvailableRespect += bonusRespect;
            gang.Loyalty += bonusRespect / 2;
            long cashBonus = respectReward * 5;
            gang.GangCash += cashBonus;
            membersList.Clear();
        }

        var player = AccountService.GetPlayerById(playerId);
        if (player == null)
        {
            result.HasError = true;
            result.ErrorMessage = "اللاعب غير موجود";
            return result;
        }

        int currentCourage = player.MainStatesObject?.CourageCurrent ?? 0;
        if (currentCourage < requiredCourage)
        {
            result.HasError = true;
            result.ErrorMessage = $"تحتاج {requiredCourage} شجاعة (لديك {currentCourage})";
            return result;
        }

        // خصم الشجاعة
        player.MainStatesObject.CourageCurrent -= requiredCourage;

        // منح الولاء الشخصي للعضو عند الانضمام إلى الميليشيا
        player.PersonalLoyalty += respectReward / 2; // مثال: نصف مكافأة الاحترام

        // المكافآت العادية
        gang.AvailableRespect += respectReward;
        gang.Loyalty += respectReward / 2;
        int crystalGain = unitId;
        player.CrystalCount += crystalGain;
        result.CrystalId = crystalGain.ToString();

        // إضافة اللاعب
        membersList.Add(playerId);
        AccountService.SavePlayer(player);

        // التحقق من الامتلاء بعد الإضافة
        if (membersList.Count >= maxMembers)
        {
            long fullBonus = respectReward * 2;
            gang.AvailableRespect += fullBonus;
            gang.Loyalty += fullBonus / 2;
            long cashBonus = respectReward * 5;
            gang.GangCash += cashBonus;
            membersList.Clear();
            membersList.Add(playerId);
        }

        gang.MembersIdsJoinedMilitia = string.Join(",", membersList);
        result.IsAllProcessSuccess = true;
        result.MembersIdsJoinedMilitia = gang.MembersIdsJoinedMilitia;
        MessagingCenter.Send(gang, "GangDataUpdated");
        return result;
    }
    // ✅ LEAVE MILITIA (only allowed for leader or if you enable it)
    public static JoinGangMilitiaResultObject LeaveMilitia(string playerId, GangObject gang, int unitId)
    {
        var result = new JoinGangMilitiaResultObject();
        if (gang == null)
        {
            result.HasError = true;
            result.ErrorMessage = "بيانات العصابة فارغة";
            return result;
        }

        if (!gang.MilitiaMembersByUnit.ContainsKey(unitId))
        {
            result.HasError = true;
            result.ErrorMessage = "الوحدة غير موجودة";
            return result;
        }

        var membersList = gang.MilitiaMembersByUnit[unitId];
        if (!membersList.Contains(playerId))
        {
            result.HasError = true;
            result.ErrorMessage = "أنت لست عضواً في هذه الوحدة";
            return result;
        }

        membersList.Remove(playerId);
        gang.MembersIdsJoinedMilitia = string.Join(",", membersList);
        result.IsAllProcessSuccess = true;
        return result;
    }

    // ✅ UPGRADE SKILL
    public static bool UpgradeSkill(string gangId, int skillIndex, long respectCost, long cashCost, int maxLevelPerGangLevel)
    {
        if (!AllGangs.TryGetValue(gangId, out var gang)) return false;
        int currentLevel = gang.SkillsLevel.ContainsKey(skillIndex.ToString()) ? gang.SkillsLevel[skillIndex.ToString()] : 0;
        int maxAllowed = gang.Level * maxLevelPerGangLevel;
        if (currentLevel >= maxAllowed) return false;
        if (gang.AvailableRespect < respectCost || gang.GangCash < cashCost) return false;

        gang.SkillsLevel[skillIndex.ToString()] = currentLevel + 1;
        gang.AvailableRespect -= respectCost;
        gang.GangCash -= cashCost;
        return true;
    }

    // ✅ DELETE GANG
    public static void DeleteGang(string gangId)
    {
        if (AllGangs.ContainsKey(gangId))
            AllGangs.Remove(gangId);
    }

    // ✅ GET GANG BY ID
    public static GangObject GetGang(string gangId) => AllGangs.TryGetValue(gangId, out var g) ? g : null;
}