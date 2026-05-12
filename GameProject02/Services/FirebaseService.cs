using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameProject02.Models;

namespace GameProject02.Services
{
    public static class FirebaseService
    {
        private const string FirebaseProjectId = "gameproject02-4207f";
        private const string FirebaseWebApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
        private static readonly HttpClient _httpClient = new HttpClient();

        // ═══════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════════

        public static async Task<bool> SavePlayerAsync(PlayerAccount player)
        {
            try
            {
                var fields = ConvertPlayerToFirestoreFields(player);
                var document = new { fields };
                var json = JsonSerializer.Serialize(document);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/players/{player.PlayerId}";
                var response = await _httpClient.PatchAsync($"{url}?key={FirebaseWebApiKey}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[FIREBASE] Save failed ({response.StatusCode}): {errorBody}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIREBASE] Save error: {ex.Message}");
                return false;
            }
        }

        public static async Task<PlayerAccount> LoadPlayerAsync(string playerId)
        {
            try
            {
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/players/{playerId}?key={FirebaseWebApiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[FIREBASE] Load failed ({response.StatusCode}): {errorBody}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("fields", out var fieldsElement))
                    return null;

                return ParsePlayerFromFirestoreFields(fieldsElement, playerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIREBASE] Load error: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // SERIALIZATION: PlayerAccount → Firestore Fields Dictionary
        // ═══════════════════════════════════════════════════════════════

        private static Dictionary<string, object> ConvertPlayerToFirestoreFields(PlayerAccount p)
        {
            var fields = new Dictionary<string, object>();

            // ── Identity & Meta ──
            fields["playerId"] = StringValue(p.PlayerId);
            fields["username"] = StringValue(p.Username);
            fields["passwordHash"] = StringValue(p.PasswordHash);
            fields["createdAt"] = TimestampValue(p.CreatedAt);
            fields["avatarPath"] = StringValue(p.AvatarPath);
            fields["gender"] = StringValue(p.Gender);
            fields["city"] = StringValue(p.City);
            fields["isVIP"] = BooleanValue(p.IsVIP);
            fields["achievementPoints"] = IntegerValue(p.AchievementPoints);
            fields["medals"] = IntegerValue(p.Medals);

            // ── Currencies ──
            fields["gold"] = IntegerValue(p.Gold);
            fields["diamonds"] = IntegerValue(p.Diamonds);
            fields["checks"] = IntegerValue(p.Checks);

            // ── Resource Bars ──
            fields["energy"] = IntegerValue(p.Energy);
            fields["maxEnergy"] = IntegerValue(p.MaxEnergy);
            fields["courage"] = IntegerValue(p.Courage);
            fields["maxCourage"] = IntegerValue(p.MaxCourage);
            fields["nobilityCurrent"] = IntegerValue(p.NobilityCurrent);
            fields["health"] = IntegerValue(p.Health);
            fields["maxHealth"] = IntegerValue(p.MaxHealth);

            // ── Combat Stats ──
            fields["strength"] = IntegerValue(p.Strength);
            fields["defense"] = IntegerValue(p.Defense);
            fields["speed"] = IntegerValue(p.Speed);
            fields["dexterity"] = IntegerValue(p.Dexterity);
            fields["intelligence"] = IntegerValue(p.Intelligence);

            // ── XP / Level Display ──
            fields["currentXP"] = IntegerValue(p.CurrentXP);
            fields["maxXP"] = IntegerValue(p.MaxXP);
            fields["levelProgress"] = DoubleValue(p.LevelProgress);
            fields["xpText"] = StringValue(p.XPText);

            // ── Nobility UI ──
            fields["nobilityChangeTimeInMilli"] = IntegerValue(p.NobilityChangeTimeInMilli);
            fields["nobilityProgress"] = DoubleValue(p.NobilityProgress);
            fields["nobilityText"] = StringValue(p.NobilityText);

            // ── General Stats ──
            fields["personalContribution"] = IntegerValue(p.PersonalContribution);
            fields["personalLoyalty"] = IntegerValue(p.PersonalLoyalty);
            fields["crystalCount"] = IntegerValue(p.CrystalCount);
            fields["crimeAttempts"] = IntegerValue(p.CrimeAttempts);
            fields["shovels"] = IntegerValue(p.Shovels);
            fields["hospitalVisits"] = IntegerValue(p.HospitalVisits);
            fields["jailTimes"] = IntegerValue(p.JailTimes);
            fields["flights"] = IntegerValue(p.Flights);
            fields["herbsUsed"] = IntegerValue(p.HerbsUsed);
            fields["itemsFound"] = IntegerValue(p.ItemsFound);

            // ── School Skills ──
            fields["crimeSuccessRate"] = IntegerValue(p.CrimeSuccessRate);
            fields["crimeGoldYield"] = IntegerValue(p.CrimeGoldYield);
            fields["crimeExperienceYield"] = IntegerValue(p.CrimeExperienceYield);
            fields["crimePunishmentReduction"] = IntegerValue(p.CrimePunishmentReduction);
            fields["hospitalTimeMultiplier"] = IntegerValue(p.HospitalTimeMultiplier);
            fields["firearmEfficiency"] = IntegerValue(p.FirearmEfficiency);
            fields["meleeWeaponEfficiency"] = IntegerValue(p.MeleeWeaponEfficiency);
            fields["bodyguardHPBonus"] = IntegerValue(p.BodyguardHPBonus);
            fields["lootBoxChance"] = IntegerValue(p.LootBoxChance);
            fields["artifactCrimeSuccess"] = IntegerValue(p.ArtifactCrimeSuccess);
            fields["stallTaxReduction"] = IntegerValue(p.StallTaxReduction);
            fields["damageReduction"] = IntegerValue(p.DamageReduction);
            fields["hackingCrimeSuccess"] = IntegerValue(p.HackingCrimeSuccess);
            fields["carCrimeSuccess"] = IntegerValue(p.CarCrimeSuccess);
            fields["estateModificationCostReduction"] = IntegerValue(p.EstateModificationCostReduction);
            fields["happinessMultiplier"] = IntegerValue(p.HappinessMultiplier);
            fields["estateHappinessBonus"] = IntegerValue(p.EstateHappinessBonus);
            fields["gymEfficiency"] = IntegerValue(p.GymEfficiency);

            // ── Skills ──
            fields["greatness"] = SkillToMap(p.Greatness);
            fields["killingDifficulty"] = SkillToMap(p.KillingDifficulty);
            fields["fastGhost"] = SkillToMap(p.FastGhost);
            fields["lightMovement"] = SkillToMap(p.LightMovement);

            // ── Estate Summary (flat) ──
            fields["estateType"] = StringValue(p.EstateType);
            fields["estateOwner"] = StringValue(p.EstateOwner);
            fields["estateHours"] = IntegerValue(p.EstateHours);
            fields["estateUpgrades"] = IntegerValue(p.EstateUpgrades);
            fields["estateWorkers"] = IntegerValue(p.EstateWorkers);
            fields["imageResource"] = StringValue(p.ImageResource);

            // ── Nested Game Objects ──
            fields["crimeObject"] = CrimeObjectToMap(p.CrimeObject);
            fields["combat"] = CombatObjectToMap(p.Combat);
            fields["armingObject"] = ArmingObjectToMap(p.ArmingObject);
            fields["stockObject"] = StockObjectToMap(p.StockObject);
            fields["museum"] = MuseumObjectToMap(p.Museum);
            fields["estate"] = EstateObjectToMap(p.Estate);
            fields["estates"] = ArrayValue(p.Estates?.Select(EstateObjectToMap).ToArray() ?? Array.Empty<object>());
            fields["primaryResidenceEstateId"] = IntegerValue(p.PrimaryResidenceEstateId);
            fields["primaryResidenceEstateInstanceId"] = StringValue(p.PrimaryResidenceEstateInstanceId);
            fields["workObject"] = WorkObjectToMap(p.WorkObject);
            fields["school"] = SchoolObjectToMap(p.School);
            fields["gym"] = GymObjectToMap(p.Gym);

            // ── Gang (store only reference ID) ──
            fields["gangId"] = StringValue(p.GangObject?.GangId ?? "");

            // ✅ NOTIFICATIONS (embedded list)
            fields["notifications"] = ArrayValue(
                p.Notifications?.Select(n => MapValue(new Dictionary<string, object>
                {
                    { "id", StringValue(n.Id) },
                    { "title", StringValue(n.Title) },
                    { "message", StringValue(n.Message) },
                    { "timestamp", TimestampValue(n.Timestamp) },
                    { "category", IntegerValue((int)n.Category) },
                    { "priority", IntegerValue((int)n.Priority) },
                    { "icon", StringValue(n.Icon) },
                    { "isRead", BooleanValue(n.IsRead) },
                    { "actionTarget", StringValue(n.ActionTarget) },
                    { "playerId", StringValue(n.PlayerId) }
                })).ToArray() ?? Array.Empty<object>()
            );

            return fields;
        }

        // ═══════════════════════════════════════════════════════════════
        // DESERIALIZATION
        // ═══════════════════════════════════════════════════════════════

        private static PlayerAccount ParsePlayerFromFirestoreFields(JsonElement fields, string playerId)
        {
            var p = new PlayerAccount();
            p.PlayerId = playerId;

            // ── Identity & Meta ──
            p.Username = fields.GetString("username") ?? "";
            p.PasswordHash = fields.GetString("passwordHash") ?? "";
            p.CreatedAt = fields.GetTimestamp("createdAt");
            p.AvatarPath = fields.GetString("avatarPath") ?? p.AvatarPath;
            p.Gender = fields.GetString("gender") ?? "ذكر";
            p.City = fields.GetString("city") ?? "مدينة العصابات";
            p.IsVIP = fields.GetBoolean("isVIP");
            p.AchievementPoints = fields.GetInt32("achievementPoints");
            p.Medals = fields.GetInt32("medals");

            // ── Currencies ──
            p.Gold = fields.GetInt32("gold");
            p.Diamonds = fields.GetInt32("diamonds");
            p.Checks = fields.GetInt32("checks");

            // ── Resource Bars ──
            p.Energy = fields.GetInt32("energy");
            p.MaxEnergy = fields.GetInt32("maxEnergy");
            p.Courage = fields.GetInt32("courage");
            p.MaxCourage = fields.GetInt32("maxCourage");
            p.NobilityCurrent = fields.GetInt32("nobilityCurrent");
            p.Health = fields.GetInt32("health");
            p.MaxHealth = fields.GetInt32("maxHealth");

            // ── Combat Stats ──
            p.Strength = fields.GetInt32("strength");
            p.Defense = fields.GetInt32("defense");
            p.Speed = fields.GetInt32("speed");
            p.Dexterity = fields.GetInt32("dexterity");
            p.Intelligence = fields.GetInt32("intelligence");

            // ── XP / Level ──
            p.CurrentXP = fields.GetInt32("currentXP");
            p.MaxXP = fields.GetInt32("maxXP");
            p.LevelProgress = fields.GetDouble("levelProgress");
            p.XPText = fields.GetString("xpText") ?? "";

            // ── Nobility UI ──
            p.NobilityChangeTimeInMilli = fields.GetInt64("nobilityChangeTimeInMilli");
            p.NobilityProgress = fields.GetDouble("nobilityProgress");
            p.NobilityText = fields.GetString("nobilityText") ?? "";

            // ── General Stats ──
            p.PersonalContribution = fields.GetInt32("personalContribution");
            p.PersonalLoyalty = fields.GetInt64("personalLoyalty");
            p.CrystalCount = fields.GetInt32("crystalCount");
            p.CrimeAttempts = fields.GetInt32("crimeAttempts");
            p.Shovels = fields.GetInt32("shovels");
            p.HospitalVisits = fields.GetInt32("hospitalVisits");
            p.JailTimes = fields.GetInt32("jailTimes");
            p.Flights = fields.GetInt32("flights");
            p.HerbsUsed = fields.GetInt32("herbsUsed");
            p.ItemsFound = fields.GetInt32("itemsFound");

            // ── School Skills ──
            p.CrimeSuccessRate = fields.GetInt32("crimeSuccessRate");
            p.CrimeGoldYield = fields.GetInt32("crimeGoldYield");
            p.CrimeExperienceYield = fields.GetInt32("crimeExperienceYield");
            p.CrimePunishmentReduction = fields.GetInt32("crimePunishmentReduction");
            p.HospitalTimeMultiplier = fields.GetInt32("hospitalTimeMultiplier");
            p.FirearmEfficiency = fields.GetInt32("firearmEfficiency");
            p.MeleeWeaponEfficiency = fields.GetInt32("meleeWeaponEfficiency");
            p.BodyguardHPBonus = fields.GetInt32("bodyguardHPBonus");
            p.LootBoxChance = fields.GetInt32("lootBoxChance");
            p.ArtifactCrimeSuccess = fields.GetInt32("artifactCrimeSuccess");
            p.StallTaxReduction = fields.GetInt32("stallTaxReduction");
            p.DamageReduction = fields.GetInt32("damageReduction");
            p.HackingCrimeSuccess = fields.GetInt32("hackingCrimeSuccess");
            p.CarCrimeSuccess = fields.GetInt32("carCrimeSuccess");
            p.EstateModificationCostReduction = fields.GetInt32("estateModificationCostReduction");
            p.HappinessMultiplier = fields.GetInt32("happinessMultiplier");
            p.EstateHappinessBonus = fields.GetInt32("estateHappinessBonus");
            p.GymEfficiency = fields.GetInt32("gymEfficiency");

            // ── Skills ──
            if (fields.TryGetProperty("greatness", out var greatnessProp)) p.Greatness = ParseSkill(greatnessProp);
            if (fields.TryGetProperty("killingDifficulty", out var killingDifficultyProp)) p.KillingDifficulty = ParseSkill(killingDifficultyProp);
            if (fields.TryGetProperty("fastGhost", out var fastGhostProp)) p.FastGhost = ParseSkill(fastGhostProp);
            if (fields.TryGetProperty("lightMovement", out var lightMovementProp)) p.LightMovement = ParseSkill(lightMovementProp);

            // ── Estate Summary ──
            p.EstateType = fields.GetString("estateType") ?? p.EstateType;
            p.EstateOwner = fields.GetString("estateOwner") ?? p.EstateOwner;
            p.EstateHours = fields.GetInt32("estateHours");
            p.EstateUpgrades = fields.GetInt32("estateUpgrades");
            p.EstateWorkers = fields.GetInt32("estateWorkers");
            p.ImageResource = fields.GetString("imageResource") ?? p.ImageResource;

            // ── Nested Game Objects ──
            if (fields.TryGetProperty("crimeObject", out var crimeObjProp)) p.CrimeObject = ParseCrimeObject(crimeObjProp);
            if (fields.TryGetProperty("combat", out var combatProp)) p.Combat = ParseCombatObject(combatProp);
            if (fields.TryGetProperty("armingObject", out var armingProp)) p.ArmingObject = ParseArmingObject(armingProp);
            if (fields.TryGetProperty("stockObject", out var stockProp)) p.StockObject = ParseStockObject(stockProp);
            if (fields.TryGetProperty("museum", out var museumProp)) p.Museum = ParseMuseumObject(museumProp);
            if (fields.TryGetProperty("estate", out var estateProp)) p.Estate = ParseEstateObject(estateProp);
            if (fields.TryGetProperty("estates", out var estatesProp)) p.Estates = ParseEstatesList(estatesProp);
            if (fields.TryGetProperty("workObject", out var workProp)) p.WorkObject = ParseWorkObject(workProp);
            if (fields.TryGetProperty("school", out var schoolProp)) p.School = ParseSchoolObject(schoolProp);
            if (fields.TryGetProperty("gym", out var gymProp)) p.Gym = ParseGymObject(gymProp);

            p.PrimaryResidenceEstateId = fields.GetInt32("primaryResidenceEstateId");
            p.PrimaryResidenceEstateInstanceId = fields.GetString("primaryResidenceEstateInstanceId") ?? "";

            // ✅ NOTIFICATIONS: Parse embedded array
            if (fields.TryGetProperty("notifications", out var notificationsProp))
            {
                p.Notifications = ParseNotificationsList(notificationsProp);
            }

            // Recalculate combat stats
            p.Combat.RecalculateStats(p);

            return p;
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS: Create Firestore value objects
        // ═══════════════════════════════════════════════════════════════

        private static object StringValue(string s) => new { stringValue = s ?? "" };
        private static object IntegerValue(int n) => new { integerValue = n };
        private static object IntegerValue(long n) => new { integerValue = n };
        private static object DoubleValue(double d) => new { doubleValue = d };
        private static object BooleanValue(bool b) => new { booleanValue = b };
        private static object TimestampValue(DateTime dt) => new { timestampValue = dt.ToString("o") };
        private static object ArrayValue(object[] items) => new { arrayValue = new { values = items } };
        private static object MapValue(Dictionary<string, object> fields) => new { mapValue = new { fields } };

        // ═══════════════════════════════════════════════════════════════
        // MODEL → MAP CONVERTERS
        // ═══════════════════════════════════════════════════════════════

        private static object SkillToMap(Skill s) => MapValue(new Dictionary<string, object> {
            { "name", StringValue(s.Name) },
            { "percentage", IntegerValue(s.Percentage) },
            { "baseValue", IntegerValue(s.BaseValue) },
            { "bonusValue", IntegerValue(s.BonusValue) }
        });

        private static object CrimeObjectToMap(CrimeObject c) => MapValue(new Dictionary<string, object> {
            { "isInPlane"             , BooleanValue(c.IsInPlane) },
            { "flightReleaseTime"     , IntegerValue(c.FlightReleaseTime) },
            { "currentCrimeType"         , IntegerValue(c.CurrentCrimeType) },
            { "isInPrison"               , BooleanValue(c.IsInPrison) },
            { "prisonReleaseTime"        , IntegerValue(c.PrisonReleaseTime) },
            { "prisonBailAmount"         , IntegerValue(c.PrisonBailAmount) },
            { "prisonReason"             , StringValue(c.PrisonReason) },
            { "isInHospital"             , BooleanValue(c.IsInHospital) },
            { "hospitalReleaseTime"      , IntegerValue(c.HospitalReleaseTime) },
            { "hospitalReason"           , StringValue(c.HospitalReason) },
            { "healthCurrent"            , IntegerValue(c.HealthCurrent) },
            { "healthMax"                , IntegerValue(c.HealthMax) },
            { "totalCrimesAttempted"     , IntegerValue(c.TotalCrimesAttempted) },
            { "totalCrimesSuccessful"    , IntegerValue(c.TotalCrimesSuccessful) },
            { "totalCrimesFailed"        , IntegerValue(c.TotalCrimesFailed) },
            { "totalPrisonVisits"        , IntegerValue(c.TotalPrisonVisits) },
            { "totalHospitalVisits"      , IntegerValue(c.TotalHospitalVisits) },
            { "currentMissionId"         , IntegerValue(c.CurrentMissionId) },
            { "currentTaskIndex"         , FromIntDict(c.CurrentTaskIndex) },
            { "currentTaskExecutionCount", FromIntDict(c.CurrentTaskExecutionCount) },
            { "taskProgress"             , FromIntDict(c.TaskProgress) },
            { "missionProgress"          , FromIntDict(c.MissionProgress) },
            { "maxCourage"               , IntegerValue(c.MaxCourage) },
            { "lastCourageRechargeTime"  , IntegerValue(c.LastCourageRechargeTime) }
        });

        private static object CombatObjectToMap(CombatObject c) => MapValue(new Dictionary<string, object> {
            { "totalHealth"      , IntegerValue(c.TotalHealth) },
            { "currentHealth"    , IntegerValue(c.CurrentHealth) },
            { "attackPower"      , IntegerValue(c.AttackPower) },
            { "defensePower"     , IntegerValue(c.DefensePower) },
            { "speed"            , IntegerValue(c.Speed) },
            { "criticalChance"   , IntegerValue(c.CriticalChance) },
            { "isInCombat"       , BooleanValue(c.IsInCombat) },
            { "battlesFought"    , IntegerValue(c.BattlesFought) },
            { "battlesWon"       , IntegerValue(c.BattlesWon) }
        });

        private static object ArmingObjectToMap(ArmingObject a) => MapValue(new Dictionary<string, object> {
            { "weaponId"              , StringValue(a.WeaponId) },
            { "weaponLevel"           , IntegerValue(a.WeaponLevel) },
            { "armorId"               , StringValue(a.ArmorId) },
            { "armorLevel"            , IntegerValue(a.ArmorLevel) },
            { "specialEquipmentId"    , StringValue(a.SpecialEquipmentId) },
            { "specialEquipmentLevel" , IntegerValue(a.SpecialEquipmentLevel) },
            { "bioChemicalId"         , StringValue(a.BioChemicalId) },
            { "bioChemicalLevel"      , IntegerValue(a.BioChemicalLevel) }
        });

        private static object StockObjectToMap(StockObject s) => MapValue(new Dictionary<string, object> {
            { "stockSpace"      , IntegerValue(s.StockSpace) },
            { "stockFreeSpace"  , IntegerValue(s.StockFreeSpace) },
            { "bagSpace"        , IntegerValue(s.BagSpace) },
            { "bagFreeSpace"    , IntegerValue(s.BagFreeSpace) },
            { "shopSpaces"      , IntegerValue(s.ShopSpaces) },
            { "maxShopSpaces"   , IntegerValue(s.MaxShopSpaces) },
            { "lockerSpace"     , IntegerValue(s.LockerSpace) },
            { "stallSpace"      , IntegerValue(s.StallSpace) },
            { "museumSpace"     , IntegerValue(s.MuseumSpace) },
            { "itemsInStock"    , StockItemsToMap(s.ItemsInStock) },
            { "lockedItemIds"   , ArrayValue(s.LockedItemIds?.Select(id => StringValue(id)).ToArray() ?? Array.Empty<object>()) }
        });

        private static object StockItemsToMap(Dictionary<string, StockItem> items)
        {
            var fields = new Dictionary<string, object>();
            if (items != null)
            {
                foreach (var kvp in items)
                {
                    fields[kvp.Key] = StockItemToMap(kvp.Value);
                }
            }
            return MapValue(fields);
        }

        private static object StockItemToMap(StockItem item) => MapValue(new Dictionary<string, object> {
            { "itemId"        , StringValue(item.ItemId) },
            { "name"          , StringValue(item.Name) },
            { "description"   , StringValue(item.Description) },
            { "imageResource" , StringValue(item.ImageResource) },
            { "categoryId"    , IntegerValue(item.CategoryId) },
            { "count"         , IntegerValue(item.Count) },
            { "originalPrice" , IntegerValue(item.OriginalPrice) },
            { "usedInArming"  , BooleanValue(item.UsedInArming) },
            { "isLocked"      , BooleanValue(item.IsLocked) },
            { "countInBag"    , IntegerValue(item.CountInBag) },
            { "damage"        , IntegerValue(item.Damage) },
            { "accuracy"      , IntegerValue(item.Accuracy) },
            { "defense"       , IntegerValue(item.Defense) },
            { "evasion"       , IntegerValue(item.Evasion) },
            { "isWeapon"      , BooleanValue(item.IsWeapon) },
            { "isGun"         , BooleanValue(item.IsGun) },
            { "gunType"       , IntegerValue(item.GunType) }
        });

        private static object MuseumObjectToMap(MuseumObject m) => MapValue(new Dictionary<string, object> {
            { "museumSpaces"        , IntegerValue(m.MuseumSpaces) },
            { "maxMuseumSpaces"     , IntegerValue(m.MaxMuseumSpaces) },
            { "backgroundId"        , IntegerValue(m.BackgroundId) },
            { "unlockedBackgrounds" , ArrayValue(m.UnlockedBackgrounds?.Select(b => IntegerValue(b)).ToArray() ?? Array.Empty<object>()) },
            { "items"               , ArrayValue(m.Items?.Select(MuseumItemToMap).ToArray() ?? Array.Empty<object>()) }
        });

        private static object MuseumItemToMap(MuseumItem i) => MapValue(new Dictionary<string, object> {
            { "itemId"        , StringValue(i.ItemId) },
            { "itemName"      , StringValue(i.ItemName) },
            { "imageResource" , StringValue(i.ImageResource) },
            { "quantity"      , IntegerValue(i.Quantity) },
            { "originalPrice" , IntegerValue(i.OriginalPrice) },
            { "damage"        , IntegerValue(i.Damage) },
            { "accuracy"      , IntegerValue(i.Accuracy) },
            { "defense"       , IntegerValue(i.Defense) },
            { "evasion"       , IntegerValue(i.Evasion) },
            { "isWeapon"      , BooleanValue(i.IsWeapon) },
            { "isGun"         , BooleanValue(i.IsGun) },
            { "gunType"       , IntegerValue(i.GunType) }
        });

        private static object EstateObjectToMap(EstateObject e) => MapValue(new Dictionary<string, object> {
            { "id"                       , IntegerValue(e.Id) },
            { "instanceId"               , StringValue(e.InstanceId) },
            { "estateOwnerId"            , StringValue(e.EstateOwnerId) },
            { "isUsed"                   , BooleanValue(e.IsUsed) },
            { "isSpouseUsed"             , BooleanValue(e.IsSpouseUsed) },
            { "isForSale"                , BooleanValue(e.IsForSale) },
            { "isForRent"                , BooleanValue(e.IsForRent) },
            { "isRentedEstate"           , BooleanValue(e.IsRentedEstate) },
            { "isRentedOut"              , BooleanValue(e.IsRentedOut) },
            { "rentEndTime"              , IntegerValue(e.RentEndTime) },
            { "rentedToPlayerId"         , StringValue(e.RentedToPlayerId) },
            { "rentedToPlayerName"       , StringValue(e.RentedToPlayerName) },
            { "estateImageUrl"           , StringValue(e.EstateImageUrl) },
            { "lastTaxPaidTime"          , IntegerValue(e.LastTaxPaidTime) },
            { "purchasedUpgrades"        , ArrayValue(e.PurchasedUpgrades?.Select(u => StringValue(u)).ToArray() ?? Array.Empty<object>()) },
            { "activeContracts"          , ArrayValue(e.ActiveContracts?.Select(c => StringValue(c)).ToArray() ?? Array.Empty<object>()) },
            { "contractStartTimes"       , FromStringLongDict(e.ContractStartTimes) },
            { "fixedModifications"       , ArrayValue(e.FixedModifications?.Select(b => BooleanValue(b)).ToArray() ?? Array.Empty<object>()) },
            { "servantContractStartTimes", ArrayValue(e.ServantContractStartTimes?.Select(t => IntegerValue(t)).ToArray() ?? Array.Empty<object>()) }
        });

        private static object WorkObjectToMap(WorkObject w) => MapValue(new Dictionary<string, object> {
            { "workType"              , IntegerValue(w.WorkType) },
            { "jobLevel"              , IntegerValue(w.JobLevel) },
            { "jobStartTimeMilli"     , IntegerValue(w.JobStartTimeMilli) },
            { "jobGotSalaryTimeMilli" , IntegerValue(w.JobGotSalaryTimeMilli) }
        });

        private static object SchoolObjectToMap(SchoolObject s) => MapValue(new Dictionary<string, object> {
            { "lawLessons"      , ArrayValue(s.LawLessons?.Select(i => IntegerValue(i)).ToArray() ?? Array.Empty<object>()) },
            { "militaryLessons" , ArrayValue(s.MilitaryLessons?.Select(i => IntegerValue(i)).ToArray() ?? Array.Empty<object>()) },
            { "historyLessons"  , ArrayValue(s.HistoryLessons?.Select(i => IntegerValue(i)).ToArray() ?? Array.Empty<object>()) },
            { "scienceLessons"  , ArrayValue(s.ScienceLessons?.Select(i => IntegerValue(i)).ToArray() ?? Array.Empty<object>()) },
            { "gymLessons"      , ArrayValue(s.GymLessons?.Select(i => IntegerValue(i)).ToArray() ?? Array.Empty<object>()) },
            { "isStudying"      , BooleanValue(s.IsStudying) },
            { "startStudyingTimeInMilli", IntegerValue(s.StartStudyingTimeInMilli) },
            { "currentCategory" , IntegerValue(s.CurrentCategory) },
            { "currentLesson"   , IntegerValue(s.CurrentLesson) }
        });

        private static object GymObjectToMap(GymObject g) => MapValue(new Dictionary<string, object> {
            { "selectedLesson"  , IntegerValue(g.SelectedLesson) },
            { "lessonProgress"  , ArrayValue(g.LessonProgress?.Select(p => IntegerValue(p)).ToArray() ?? Array.Empty<object>()) },
            { "lessonUnlocked"  , ArrayValue(g.LessonUnlocked?.Select(u => BooleanValue(u)).ToArray() ?? Array.Empty<object>()) }
        });

        private static object FromIntDict(Dictionary<int, int> dict) =>
            MapValue(dict?.ToDictionary(kvp => kvp.Key.ToString(), kvp => IntegerValue(kvp.Value))
                        ?? new Dictionary<string, object>());

        private static object FromStringLongDict(Dictionary<string, long> dict) =>
            MapValue(dict?.ToDictionary(kvp => kvp.Key, kvp => IntegerValue(kvp.Value))
                        ?? new Dictionary<string, object>());

        // ═══════════════════════════════════════════════════════════════
        // DESERIALIZATION PARSERS
        // ═══════════════════════════════════════════════════════════════

        private static Skill ParseSkill(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new Skill("", 0, 0, 0);
            return new Skill(
                fields.Value.GetString("name") ?? "",
                fields.Value.GetInt32("percentage"),
                fields.Value.GetInt32("baseValue"),
                fields.Value.GetInt32("bonusValue")
            );
        }

        private static CrimeObject ParseCrimeObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new CrimeObject();
            var c = new CrimeObject();
            c.IsInPlane = fields.Value.GetBoolean("isInPlane");
            c.FlightReleaseTime = fields.Value.GetInt64("flightReleaseTime");
            c.CurrentCrimeType = fields.Value.GetInt32("currentCrimeType");
            c.IsInPrison = fields.Value.GetBoolean("isInPrison");
            c.PrisonReleaseTime = fields.Value.GetInt64("prisonReleaseTime");
            c.PrisonBailAmount = fields.Value.GetInt64("prisonBailAmount");
            c.PrisonReason = fields.Value.GetString("prisonReason") ?? "";
            c.IsInHospital = fields.Value.GetBoolean("isInHospital");
            c.HospitalReleaseTime = fields.Value.GetInt64("hospitalReleaseTime");
            c.HospitalReason = fields.Value.GetString("hospitalReason") ?? "";
            c.HealthCurrent = fields.Value.GetInt32("healthCurrent");
            c.HealthMax = fields.Value.GetInt32("healthMax");
            c.TotalCrimesAttempted = fields.Value.GetInt32("totalCrimesAttempted");
            c.TotalCrimesSuccessful = fields.Value.GetInt32("totalCrimesSuccessful");
            c.TotalCrimesFailed = fields.Value.GetInt32("totalCrimesFailed");
            c.TotalPrisonVisits = fields.Value.GetInt32("totalPrisonVisits");
            c.TotalHospitalVisits = fields.Value.GetInt32("totalHospitalVisits");
            c.CurrentMissionId = fields.Value.GetInt32("currentMissionId");
            c.MaxCourage = fields.Value.GetInt32("maxCourage");
            c.LastCourageRechargeTime = fields.Value.GetInt64("lastCourageRechargeTime");

            if (fields.Value.TryGetProperty("currentTaskIndex", out var taskIndexProp))
                c.CurrentTaskIndex = ParseIntIntDict(taskIndexProp);
            if (fields.Value.TryGetProperty("currentTaskExecutionCount", out var execCountProp))
                c.CurrentTaskExecutionCount = ParseIntIntDict(execCountProp);
            if (fields.Value.TryGetProperty("taskProgress", out var tProgressProp))
                c.TaskProgress = ParseIntIntDict(tProgressProp);
            if (fields.Value.TryGetProperty("missionProgress", out var mProgressProp))
                c.MissionProgress = ParseIntIntDict(mProgressProp);
            return c;
        }

        private static CombatObject ParseCombatObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new CombatObject();
            var c = new CombatObject();
            c.CurrentHealth = fields.Value.GetInt32("currentHealth");
            c.IsInCombat = fields.Value.GetBoolean("isInCombat");
            c.BattlesFought = fields.Value.GetInt32("battlesFought");
            c.BattlesWon = fields.Value.GetInt32("battlesWon");
            return c;
        }

        private static ArmingObject ParseArmingObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new ArmingObject();
            return new ArmingObject
            {
                WeaponId = fields.Value.GetString("weaponId") ?? "",
                WeaponLevel = fields.Value.GetInt32("weaponLevel"),
                ArmorId = fields.Value.GetString("armorId") ?? "",
                ArmorLevel = fields.Value.GetInt32("armorLevel"),
                SpecialEquipmentId = fields.Value.GetString("specialEquipmentId") ?? "",
                SpecialEquipmentLevel = fields.Value.GetInt32("specialEquipmentLevel"),
                BioChemicalId = fields.Value.GetString("bioChemicalId") ?? "",
                BioChemicalLevel = fields.Value.GetInt32("bioChemicalLevel")
            };
        }

        private static StockObject ParseStockObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new StockObject();
            var s = new StockObject();
            s.StockSpace = fields.Value.GetInt32("stockSpace");
            s.StockFreeSpace = fields.Value.GetInt32("stockFreeSpace");
            s.BagSpace = fields.Value.GetInt32("bagSpace");
            s.BagFreeSpace = fields.Value.GetInt32("bagFreeSpace");
            s.ShopSpaces = fields.Value.GetInt32("shopSpaces");
            s.MaxShopSpaces = fields.Value.GetInt32("maxShopSpaces");
            s.LockerSpace = fields.Value.GetInt32("lockerSpace");
            s.StallSpace = fields.Value.GetInt32("stallSpace");
            s.MuseumSpace = fields.Value.GetInt32("museumSpace");

            if (fields.Value.TryGetProperty("itemsInStock", out var itemsProp))
                s.ItemsInStock = ParseStockItemsDict(itemsProp);
            if (fields.Value.TryGetProperty("lockedItemIds", out var lockedProp))
                s.LockedItemIds = ParseStringSet(lockedProp);
            return s;
        }

        private static Dictionary<string, StockItem> ParseStockItemsDict(JsonElement mapProp)
        {
            var dict = new Dictionary<string, StockItem>();
            if (mapProp.ValueKind != JsonValueKind.Object) return dict;
            if (mapProp.TryGetProperty("mapValue", out var mapVal) &&
                mapVal.TryGetProperty("fields", out var fieldsElement))
            {
                foreach (var kvp in fieldsElement.EnumerateObject())
                {
                    dict[kvp.Name] = ParseStockItem(kvp.Value);
                }
            }
            return dict;
        }

        private static StockItem ParseStockItem(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new StockItem();
            var item = new StockItem();
            item.ItemId = fields.Value.GetString("itemId") ?? "";
            item.Name = fields.Value.GetString("name") ?? "";
            item.Description = fields.Value.GetString("description") ?? "";
            item.ImageResource = fields.Value.GetString("imageResource") ?? "";
            item.CategoryId = fields.Value.GetInt32("categoryId");
            item.Count = fields.Value.GetInt32("count");
            item.OriginalPrice = fields.Value.GetInt32("originalPrice");
            item.UsedInArming = fields.Value.GetBoolean("usedInArming");
            item.IsLocked = fields.Value.GetBoolean("isLocked");
            item.CountInBag = fields.Value.GetInt32("countInBag");
            item.Damage = fields.Value.GetInt32("damage");
            item.Accuracy = fields.Value.GetInt32("accuracy");
            item.Defense = fields.Value.GetInt32("defense");
            item.Evasion = fields.Value.GetInt32("evasion");
            item.IsWeapon = fields.Value.GetBoolean("isWeapon");
            item.IsGun = fields.Value.GetBoolean("isGun");
            item.GunType = fields.Value.GetInt32("gunType");
            return item;
        }

        private static MuseumObject ParseMuseumObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new MuseumObject();
            var m = new MuseumObject();
            m.MuseumSpaces = fields.Value.GetInt32("museumSpaces");
            m.MaxMuseumSpaces = fields.Value.GetInt32("maxMuseumSpaces");
            m.BackgroundId = fields.Value.GetInt32("backgroundId");
            m.UnlockedBackgrounds = ParseIntList(fields.Value, "unlockedBackgrounds");
            if (fields.Value.TryGetProperty("items", out var itemsProp))
                m.Items = ParseMuseumItemsList(itemsProp);
            return m;
        }

        private static List<MuseumItem> ParseMuseumItemsList(JsonElement prop)
        {
            var list = new List<MuseumItem>();
            if (prop.TryGetProperty("arrayValue", out var arrayVal) &&
                arrayVal.TryGetProperty("values", out var values))
            {
                foreach (var val in values.EnumerateArray())
                {
                    list.Add(ParseMuseumItem(val));
                }
            }
            return list;
        }

        private static MuseumItem ParseMuseumItem(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new MuseumItem();
            return new MuseumItem
            {
                ItemId = fields.Value.GetString("itemId") ?? "",
                ItemName = fields.Value.GetString("itemName") ?? "",
                ImageResource = fields.Value.GetString("imageResource") ?? "",
                Quantity = fields.Value.GetInt32("quantity"),
                OriginalPrice = fields.Value.GetInt32("originalPrice"),
                Damage = fields.Value.GetInt32("damage"),
                Accuracy = fields.Value.GetInt32("accuracy"),
                Defense = fields.Value.GetInt32("defense"),
                Evasion = fields.Value.GetInt32("evasion"),
                IsWeapon = fields.Value.GetBoolean("isWeapon"),
                IsGun = fields.Value.GetBoolean("isGun"),
                GunType = fields.Value.GetInt32("gunType")
            };
        }

        private static EstateObject ParseEstateObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new EstateObject();
            var e = new EstateObject();
            e.Id = fields.Value.GetInt32("id");
            e.InstanceId = fields.Value.GetString("instanceId") ?? Guid.NewGuid().ToString().Substring(0, 8);
            e.EstateOwnerId = fields.Value.GetString("estateOwnerId") ?? "";
            e.IsUsed = fields.Value.GetBoolean("isUsed");
            e.IsSpouseUsed = fields.Value.GetBoolean("isSpouseUsed");
            e.IsForSale = fields.Value.GetBoolean("isForSale");
            e.IsForRent = fields.Value.GetBoolean("isForRent");
            e.IsRentedEstate = fields.Value.GetBoolean("isRentedEstate");
            e.IsRentedOut = fields.Value.GetBoolean("isRentedOut");
            e.RentEndTime = fields.Value.GetInt64("rentEndTime");
            e.RentedToPlayerId = fields.Value.GetString("rentedToPlayerId") ?? "";
            e.RentedToPlayerName = fields.Value.GetString("rentedToPlayerName") ?? "";
            e.EstateImageUrl = fields.Value.GetString("estateImageUrl") ?? "";
            e.LastTaxPaidTime = fields.Value.GetInt64("lastTaxPaidTime");
            e.PurchasedUpgrades = ParseStringList(fields.Value, "purchasedUpgrades");
            e.ActiveContracts = ParseStringList(fields.Value, "activeContracts");
            e.ContractStartTimes = ParseStringLongDict(fields.Value, "contractStartTimes");
            e.FixedModifications = ParseBoolList(fields.Value, "fixedModifications");
            e.ServantContractStartTimes = ParseLongList(fields.Value, "servantContractStartTimes");
            return e;
        }

        private static List<EstateObject> ParseEstatesList(JsonElement prop)
        {
            var list = new List<EstateObject>();
            if (prop.TryGetProperty("arrayValue", out var arrayVal) &&
                arrayVal.TryGetProperty("values", out var values))
            {
                foreach (var val in values.EnumerateArray())
                {
                    list.Add(ParseEstateObject(val));
                }
            }
            return list;
        }

        private static WorkObject ParseWorkObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new WorkObject();
            return new WorkObject
            {
                WorkType = fields.Value.GetInt32("workType"),
                JobLevel = fields.Value.GetInt32("jobLevel"),
                JobStartTimeMilli = fields.Value.GetInt64("jobStartTimeMilli"),
                JobGotSalaryTimeMilli = fields.Value.GetInt64("jobGotSalaryTimeMilli")
            };
        }

        private static SchoolObject ParseSchoolObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new SchoolObject();
            var s = new SchoolObject();
            s.LawLessons = ParseIntList(fields.Value, "lawLessons");
            s.MilitaryLessons = ParseIntList(fields.Value, "militaryLessons");
            s.HistoryLessons = ParseIntList(fields.Value, "historyLessons");
            s.ScienceLessons = ParseIntList(fields.Value, "scienceLessons");
            s.GymLessons = ParseIntList(fields.Value, "gymLessons");
            s.IsStudying = fields.Value.GetBoolean("isStudying");
            s.StartStudyingTimeInMilli = fields.Value.GetInt64("startStudyingTimeInMilli");
            s.CurrentCategory = fields.Value.GetInt32("currentCategory");
            s.CurrentLesson = fields.Value.GetInt32("currentLesson");
            return s;
        }

        private static GymObject ParseGymObject(JsonElement mapField)
        {
            var fields = mapField.GetMapFieldsNullable();
            if (fields == null) return new GymObject();
            var g = new GymObject();
            g.SelectedLesson = fields.Value.GetInt32("selectedLesson");
            g.LessonProgress = ParseIntList(fields.Value, "lessonProgress");
            g.LessonUnlocked = ParseBoolList(fields.Value, "lessonUnlocked");
            return g;
        }

        // ✅ NEW: Parse the notifications array from Firestore fields
        private static List<NotificationItem> ParseNotificationsList(JsonElement prop)
        {
            var list = new List<NotificationItem>();
            if (prop.TryGetProperty("arrayValue", out var arrVal) &&
                arrVal.TryGetProperty("values", out var values))
            {
                foreach (var val in values.EnumerateArray())
                {
                    var fields = val.GetMapFieldsNullable();
                    if (fields != null)
                    {
                        list.Add(new NotificationItem
                        {
                            Id = fields.Value.GetString("id") ?? Guid.NewGuid().ToString(),
                            Title = fields.Value.GetString("title") ?? "",
                            Message = fields.Value.GetString("message") ?? "",
                            Timestamp = fields.Value.GetTimestamp("timestamp"),
                            Category = (NotificationCategory)fields.Value.GetInt32("category"),
                            Priority = (GameNotificationPriority)fields.Value.GetInt32("priority"),
                            Icon = fields.Value.GetString("icon") ?? "🔔",
                            IsRead = fields.Value.GetBoolean("isRead"),
                            ActionTarget = fields.Value.GetString("actionTarget") ?? "",
                            PlayerId = fields.Value.GetString("playerId") ?? ""
                        });
                    }
                }
            }
            return list;
        }

        // ═══════════════════════════════════════════════════════════════
        // GENERIC DESERIALIZATION HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static JsonElement? GetMapFieldsNullable(this JsonElement element)
        {
            if (element.TryGetProperty("mapValue", out var mapObj) &&
                mapObj.TryGetProperty("fields", out var fieldsObj))
                return fieldsObj;
            return null;
        }

        private static Dictionary<int, int> ParseIntIntDict(JsonElement prop)
        {
            var dict = new Dictionary<int, int>();
            var fields = prop.GetMapFieldsNullable();
            if (fields == null) return dict;
            foreach (var kvp in fields.Value.EnumerateObject())
            {
                if (int.TryParse(kvp.Name, out int key))
                {
                    dict[key] = kvp.Value.GetInt32Value();
                }
            }
            return dict;
        }

        private static HashSet<string> ParseStringSet(JsonElement prop)
        {
            var set = new HashSet<string>();
            if (prop.TryGetProperty("arrayValue", out var arrVal) &&
                arrVal.TryGetProperty("values", out var values))
            {
                foreach (var v in values.EnumerateArray())
                {
                    set.Add(v.GetStringValue() ?? "");
                }
            }
            return set;
        }

        private static List<int> ParseIntList(JsonElement fields, string key)
        {
            var list = new List<int>();
            if (fields.TryGetProperty(key, out var prop))
            {
                ParseArrayValues(prop, (v) => list.Add(v.GetInt32Value()));
            }
            return list;
        }

        private static List<long> ParseLongList(JsonElement fields, string key)
        {
            var list = new List<long>();
            if (fields.TryGetProperty(key, out var prop))
            {
                ParseArrayValues(prop, (v) => list.Add(v.GetInt64Value()));
            }
            return list;
        }

        private static List<string> ParseStringList(JsonElement fields, string key)
        {
            var list = new List<string>();
            if (fields.TryGetProperty(key, out var prop))
            {
                ParseArrayValues(prop, (v) => list.Add(v.GetStringValue() ?? ""));
            }
            return list;
        }

        private static List<bool> ParseBoolList(JsonElement fields, string key)
        {
            var list = new List<bool>();
            if (fields.TryGetProperty(key, out var prop))
            {
                ParseArrayValues(prop, (v) => list.Add(v.GetBooleanValue()));
            }
            return list;
        }

        private static Dictionary<string, long> ParseStringLongDict(JsonElement fields, string key)
        {
            var dict = new Dictionary<string, long>();
            if (fields.TryGetProperty(key, out var prop))
            {
                var f = prop.GetMapFieldsNullable();
                if (f != null)
                {
                    foreach (var kvp in f.Value.EnumerateObject())
                    {
                        dict[kvp.Name] = kvp.Value.GetInt64Value();
                    }
                }
            }
            return dict;
        }

        private static void ParseArrayValues(JsonElement prop, Action<JsonElement> action)
        {
            if (prop.TryGetProperty("arrayValue", out var arrVal) &&
                arrVal.TryGetProperty("values", out var values))
            {
                foreach (var v in values.EnumerateArray())
                {
                    action(v);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // CORRECTED EXTENSION METHODS FOR FIRESTORE VALUES
        // (handles string‑encoded integers)
        // ═══════════════════════════════════════════════════════════════

        private static string GetString(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("stringValue", out var v))
                return v.GetString();
            return null;
        }

        private static int GetInt32(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out int result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt32();
            }
            return 0;
        }

        private static long GetInt64(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out long result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt64();
            }
            return 0;
        }

        private static double GetDouble(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("doubleValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetDouble();
                if (v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), out double result))
                    return result;
            }
            return 0;
        }

        private static bool GetBoolean(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("booleanValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False)
                    return v.GetBoolean();
                if (v.ValueKind == JsonValueKind.String && bool.TryParse(v.GetString(), out bool result))
                    return result;
            }
            return false;
        }

        private static DateTime GetTimestamp(this JsonElement e, string property)
        {
            if (e.TryGetProperty(property, out var prop) &&
                prop.TryGetProperty("timestampValue", out var v) &&
                DateTime.TryParse(v.GetString(), out var dt))
                return dt;
            return DateTime.UtcNow;
        }

        // Raw‑value helpers (used inside parsers)
        private static string GetStringValue(this JsonElement e) =>
            e.TryGetProperty("stringValue", out var v) ? v.GetString() : "";

        private static int GetInt32Value(this JsonElement e)
        {
            if (e.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out int result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt32();
            }
            return 0;
        }

        private static long GetInt64Value(this JsonElement e)
        {
            if (e.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out long result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt64();
            }
            return 0;
        }

        private static double GetDoubleValue(this JsonElement e) =>
            e.TryGetProperty("doubleValue", out var v) ? v.GetDouble() : 0;

        private static bool GetBooleanValue(this JsonElement e)
        {
            if (e.TryGetProperty("booleanValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False)
                    return v.GetBoolean();
            }
            return false;
        }
    }
}