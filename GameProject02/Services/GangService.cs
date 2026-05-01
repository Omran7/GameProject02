using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class GangService
{
    // ✅ VALIDATE GANG CREATION
    public static (bool isValid, string message) ValidateGangCreation(PlayerAccount player, string name, string tag)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 15)
            return (false, "اسم العصابة يجب أن يكون بين 3 و 15 حرفاً");
        if (string.IsNullOrWhiteSpace(tag) || tag.Length != 3)
            return (false, "رمز العصابة يجب أن يكون 3 أحرف بالضبط");
        if (!tag.All(char.IsLetter))
            return (false, "رمز العصابة يجب أن يحتوي على أحرف فقط");
        if (player.Gold < 1000)
            return (false, "تحتاج 1000 ذهب لإنشاء عصابة");
        if (player.Level < 10)
            return (false, "تحتاج المستوى 10 لإنشاء عصابة");
        if (player.GangObject != null && player.GangObject.IsMember(player.PlayerId))
            return (false, "أنت بالفعل عضو في عصابة");
        return (true, "");
    }

    // ✅ CREATE NEW GANG
    public static GangObject CreateGang(PlayerAccount player, string name, string tag)
    {
        var gang = new GangObject
        {
            Name = name,
            Tag = tag.ToUpper(),
            LeaderId = player.PlayerId,
            GangCash = 0,
            Respect = 0,
            AvailableRespect = 0,
            Loyalty = 0,
            Contribution = 0,
            Level = 1,
            MembersWithPositions = new Dictionary<string, GangPosition> { { player.PlayerId, GangPosition.Leader } },
            MilitiaMemberIds = new List<string>(),
            MilitiaMembersByUnit = new Dictionary<int, List<string>>(),
            SkillsLevel = new Dictionary<string, int>(),
            CreatedTimeInMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        player.Gold -= 1000;
        player.GangObject = gang;
        GangDatabaseService.CreateGang(gang);
        AccountService.SavePlayer(player);
        return gang;
    }

    // ✅ GET GANG MEMBERS
    public static List<GangMemberInfo> GetGangMembers(GangObject gang)
    {
        var members = new List<GangMemberInfo>();
        foreach (var kvp in gang.MembersWithPositions)
        {
            var player = AccountService.GetPlayerById(kvp.Key);
            members.Add(new GangMemberInfo
            {
                PlayerId = kvp.Key,
                Username = player?.Username ?? kvp.Key,
                Position = kvp.Value,
                IsOnline = false,
                PersonalLoyalty = player?.PersonalLoyalty ?? 0,  // ✅ قراءة الولاء الشخصي
                PersonalContribution = player?.PersonalContribution ?? 0
            });
        }
        return members.OrderBy(m => (int)m.Position).ThenByDescending(m => m.PersonalLoyalty).ToList();
    }

    public static bool CanPerformAction(GangObject gang, string playerId, GangAction action)
    {
        if (!gang.IsMember(playerId)) return false;
        var position = gang.GetPosition(playerId);

        switch (action)
        {
            case GangAction.DisbandGang:
            case GangAction.ChangeGangData:
            case GangAction.TransferLeadership:
            case GangAction.UpgradeGangLevel:
            case GangAction.SpendGangCash:
                return position == GangPosition.Leader;

            case GangAction.ManageSkills:
                return position == GangPosition.Leader || position == GangPosition.CoLeader;

            case GangAction.KickMember:
                // سيتم التحقق من الرتبة المستهدفة في مكان آخر
                return position == GangPosition.Leader ||
                       position == GangPosition.CoLeader ||
                       position == GangPosition.Vice ||
                       position == GangPosition.Elder;

            case GangAction.PromoteMember:
            case GangAction.DemoteMember:
                return position == GangPosition.Leader || position == GangPosition.CoLeader;

            case GangAction.AcceptJoinRequest:
            case GangAction.InviteMember:
                return position != GangPosition.Member && position != GangPosition.None;

            default:
                return false;
        }
    }

    public static bool CanKickMember(GangObject gang, string kickerId, string targetId)
    {
        var kickerPos = gang.GetPosition(kickerId);
        var targetPos = gang.GetPosition(targetId);

        // القائد يمكنه طرد أي شخص
        if (kickerPos == GangPosition.Leader) return true;

        // لا يمكن طرد النفس
        if (kickerId == targetId) return false;

        // لا يمكن طرد من هم في نفس الرتبة أو أعلى
        if ((int)kickerPos <= (int)targetPos) return false;

        // التحقق من الصلاحيات حسب الرتبة
        return kickerPos switch
        {
            GangPosition.CoLeader => targetPos != GangPosition.CoLeader && targetPos != GangPosition.Leader,
            GangPosition.Vice => targetPos != GangPosition.CoLeader && targetPos != GangPosition.Leader,
            GangPosition.Elder => targetPos == GangPosition.Member || targetPos == GangPosition.Officer,
            _ => false
        };
    }

    public static bool CanPromoteMember(GangObject gang, string promoterId, string targetId)
    {
        var promoterPos = gang.GetPosition(promoterId);
        var targetPos = gang.GetPosition(targetId);

        // القائد يمكنه ترقية أي شخص
        if (promoterPos == GangPosition.Leader) return true;

        // نائب القائد يمكنه ترقية الأعضاء إلى رتب أقل من نائب القائد
        if (promoterPos == GangPosition.CoLeader)
            return targetPos != GangPosition.Leader && targetPos != GangPosition.CoLeader;

        return false;
    }

    public static bool DemoteMember(GangObject gang, string targetId, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId && gang.GetPosition(leader.PlayerId) != GangPosition.CoLeader)
            return false;
        if (!gang.MembersWithPositions.ContainsKey(targetId)) return false;
        var currentPos = gang.MembersWithPositions[targetId];
        if (currentPos == GangPosition.Leader) return false;

        GangPosition newPos = currentPos switch
        {
            GangPosition.CoLeader => GangPosition.Vice,
            GangPosition.Vice => GangPosition.Elder,
            GangPosition.Elder => GangPosition.Officer,
            GangPosition.Officer => GangPosition.Member,
            _ => GangPosition.Member
        };
        gang.MembersWithPositions[targetId] = newPos;
        return true;
    }

    // ✅ DONATE TO GANG (ADD GANG CASH)
    public static (bool success, string message) DonateToGang(PlayerAccount player, GangObject gang, int amount)
    {
        if (player.Gold < amount) return (false, "ليس لديك ذهب كافٍ");
        if (amount <= 0) return (false, "يجب أن يكون المبلغ أكبر من صفر");

        player.Gold -= amount;
        gang.GangCash += amount;

        // ✅ إضافة الولاء الشخصي: كل 100 ذهب = 1 ولاء
        long loyaltyGain = amount / 100;
        if (loyaltyGain > 0)
        {
            player.PersonalLoyalty += loyaltyGain;
        }

        AccountService.SavePlayer(player);
        MessagingCenter.Send(gang, "GangDataUpdated");
        return (true, $"تم التبرع بمبلغ {amount} ذهب. حصلت على {loyaltyGain} ولاء شخصي!");
    }

    // ✅ ADD GANG CASH FROM ANY SOURCE (TAXES, WARS, SHOP SALES)
    public static void AddGangCash(GangObject gang, long amount, string source = "unknown")
    {
        gang.GangCash += amount;
        MessagingCenter.Send(gang, "GangDataUpdated");
        System.Diagnostics.Debug.WriteLine($"[GangCash] +{amount} from {source}. Total: {gang.GangCash}");
    }

    // ✅ UPGRADE GANG LEVEL
    public static (bool success, string message) UpgradeGangLevel(GangObject gang, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId)
            return (false, "فقط الزعيم يمكنه ترقية العصابة");
        int requiredMembers = gang.Level * 5;
        if (gang.MembersWithPositions.Count < requiredMembers)
            return (false, $"تحتاج {requiredMembers} أعضاء (لديك {gang.MembersWithPositions.Count})");
        long requiredRespect = gang.Level * 1000;
        if (gang.AvailableRespect < requiredRespect)
            return (false, $"تحتاج {requiredRespect} احترام (لديك {gang.AvailableRespect})");
        long requiredCash = gang.Level * 5000;
        if (gang.GangCash < requiredCash)
            return (false, $"تحتاج {requiredCash} نقد (لديك {gang.GangCash})");
        gang.AvailableRespect -= requiredRespect;
        gang.GangCash -= requiredCash;
        gang.Level++;
        MessagingCenter.Send(gang, "GangDataUpdated");
        return (true, $"تمت الترقية إلى المستوى {gang.Level}");
    }

    // ✅ CHANGE GANG NAME
    public static bool ChangeGangName(GangObject gang, string newName, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId) return false;
        if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3 || newName.Length > 15) return false;
        gang.Name = newName.Trim();
        return true;
    }

    // ✅ CHANGE GANG TAG
    public static bool ChangeGangTag(GangObject gang, string newTag, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId) return false;
        newTag = newTag.ToUpper().Trim();
        if (newTag.Length != 3 || !newTag.All(char.IsLetter)) return false;
        gang.Tag = newTag;
        return true;
    }

    // ✅ DISBAND GANG
    public static bool DisbandGang(GangObject gang, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId) return false;
        leader.GangObject = null;
        AccountService.SavePlayer(leader);
        GangDatabaseService.DeleteGang(gang.GangId);
        return true;
    }

    // ✅ CHANGE GANG IMAGE
    public static bool ChangeGangImage(GangObject gang, string imageUrl, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId) return false;
        if (string.IsNullOrEmpty(imageUrl)) return false;
        gang.ImageUrl = imageUrl;
        return true;
    }

    // ✅ PROMOTE MEMBER TO SPECIFIC POSITION
    public static bool PromoteTo(GangObject gang, string targetId, GangPosition newPosition, PlayerAccount leader)
    {
        if (gang.LeaderId != leader.PlayerId) return false;
        if (!gang.MembersWithPositions.ContainsKey(targetId)) return false;
        if (newPosition == GangPosition.Leader) return false;
        gang.MembersWithPositions[targetId] = newPosition;
        return true;
    }

    // ✅ TRANSFER LEADERSHIP
    public static bool TransferLeadership(GangObject gang, string newLeaderId, PlayerAccount currentLeader)
    {
        if (gang.LeaderId != currentLeader.PlayerId) return false;
        if (!gang.MembersWithPositions.ContainsKey(newLeaderId)) return false;
        gang.MembersWithPositions[newLeaderId] = GangPosition.Leader;
        gang.LeaderId = newLeaderId;
        gang.MembersWithPositions[currentLeader.PlayerId] = GangPosition.CoLeader;
        return true;
    }
}