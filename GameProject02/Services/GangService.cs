using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services
{
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

        // ✅ CREATE NEW GANG (async version – preferred)
        public static async System.Threading.Tasks.Task<GangObject> CreateGangAsync(PlayerAccount player, string name, string tag)
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
            player.GangId = gang.GangId;

            // Save to Firestore
            await GangDatabaseService.SaveGangAsync(gang);
            _ = FirebaseService.SavePlayerAsync(player);   // also save player (gangId updated)

            return gang;
        }

        // ✅ Synchronous wrapper (for existing code) – fire and forget
        public static GangObject CreateGang(PlayerAccount player, string name, string tag)
        {
            // Create synchronously but still save to Firestore in background
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
            player.GangId = gang.GangId;

            // Save to Firestore in background
            _ = GangDatabaseService.SaveGangAsync(gang);
            _ = FirebaseService.SavePlayerAsync(player);

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
                    PersonalLoyalty = player?.PersonalLoyalty ?? 0,
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

            if (kickerPos == GangPosition.Leader) return true;
            if (kickerId == targetId) return false;
            if ((int)kickerPos <= (int)targetPos) return false;

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

            if (promoterPos == GangPosition.Leader) return true;
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

        public static async Task<(bool success, string message)> DonateToGangAsync(PlayerAccount player, GangObject gang, int amount)
        {
            if (player.Gold < amount) return (false, "ليس لديك ذهب كافٍ");
            if (amount <= 0) return (false, "يجب أن يكون المبلغ أكبر من صفر");

            player.Gold -= amount;
            gang.GangCash += amount;

            long loyaltyGain = amount / 100;
            if (loyaltyGain > 0)
            {
                player.PersonalLoyalty += loyaltyGain;
                gang.Loyalty += loyaltyGain;  // ✅ Add to gang's total loyalty
            }

            long respectGain = amount / 200;
            if (respectGain > 0)
            {
                gang.Respect += respectGain;
                gang.AvailableRespect += respectGain;
            }

            player.PersonalContribution += amount;

            await FirebaseService.SavePlayerAsync(player);
            await GangDatabaseService.SaveGangAsync(gang);

            MessagingCenter.Send(gang, "GangDataUpdated"); // Notify UI to refresh

            return (true, $"تم التبرع بمبلغ {amount} ذهب.\n" +
                          $"+{loyaltyGain} ولاء شخصي\n" +
                          $"+{loyaltyGain} ولاء للعصابة\n" +
                          $"+{respectGain} احترام للعصابة");
        }
        public static (bool success, string message) DonateToGang(PlayerAccount player, GangObject gang, int amount)
        {
            return Task.Run(() => DonateToGangAsync(player, gang, amount)).GetAwaiter().GetResult();
        }        // Keep a synchronous wrapper for existing code if needed

        public static void AddGangCash(GangObject gang, long amount, string source = "unknown")
        {
            gang.GangCash += amount;
            MessagingCenter.Send(gang, "GangDataUpdated");
            System.Diagnostics.Debug.WriteLine($"[GangCash] +{amount} from {source}. Total: {gang.GangCash}");
        }

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

        public static bool ChangeGangName(GangObject gang, string newName, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3 || newName.Length > 15) return false;
            gang.Name = newName.Trim();
            return true;
        }

        public static bool ChangeGangTag(GangObject gang, string newTag, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            newTag = newTag.ToUpper().Trim();
            if (newTag.Length != 3 || !newTag.All(char.IsLetter)) return false;
            gang.Tag = newTag;
            return true;
        }

        public static bool DisbandGang(GangObject gang, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            leader.GangObject = null;
            AccountService.SavePlayer(leader);
            GangDatabaseService.DeleteGang(gang.GangId);
            return true;
        }

        public static bool ChangeGangImage(GangObject gang, string imageUrl, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            if (string.IsNullOrEmpty(imageUrl)) return false;
            gang.ImageUrl = imageUrl;
            return true;
        }

        public static bool PromoteTo(GangObject gang, string targetId, GangPosition newPosition, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            if (!gang.MembersWithPositions.ContainsKey(targetId)) return false;
            if (newPosition == GangPosition.Leader) return false;
            gang.MembersWithPositions[targetId] = newPosition;
            return true;
        }

        public static bool TransferLeadership(GangObject gang, string newLeaderId, PlayerAccount currentLeader)
        {
            if (gang.LeaderId != currentLeader.PlayerId) return false;
            if (!gang.MembersWithPositions.ContainsKey(newLeaderId)) return false;
            gang.MembersWithPositions[newLeaderId] = GangPosition.Leader;
            gang.LeaderId = newLeaderId;
            gang.MembersWithPositions[currentLeader.PlayerId] = GangPosition.CoLeader;
            return true;
        }

        public static async Task<bool> ChangeGangImageAsync(GangObject gang, string imagePath, PlayerAccount leader)
        {
            if (gang.LeaderId != leader.PlayerId) return false;
            if (string.IsNullOrEmpty(imagePath)) return false;

            try
            {
                // Request permission on Android
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var status = await Permissions.RequestAsync<Permissions.StorageRead>();
                    if (status != PermissionStatus.Granted)
                        return false;
                }

                // Read the file from the actual path
                using var stream = System.IO.File.OpenRead(imagePath);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                string base64 = Convert.ToBase64String(bytes);
                gang.ImageUrl = $"data:image/png;base64,{base64}";
                await GangDatabaseService.SaveGangAsync(gang);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GANG] Image change error: {ex.Message}");
                return false;
            }
        }
    }
}