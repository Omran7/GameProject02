using System;
using System.Collections.Generic;

namespace GameProject02.Models;

// 📋 Admin Request Model
public enum AdminRequestType
{
    AdminPromotion = 0,   // Player requests to become admin
    BanRequest = 1        // Manager requests to ban a player
}

public class AdminRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PlayerId { get; set; } = string.Empty;      // Requester ID
    public string PlayerName { get; set; } = string.Empty;
    public AdminRequestType RequestType { get; set; } = AdminRequestType.AdminPromotion;
    public string Reason { get; set; } = string.Empty;

    // For BanRequest only
    public string TargetPlayerId { get; set; } = string.Empty;
    public string TargetPlayerName { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;   // Optional proof image

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsReviewed { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public string ReviewedBy { get; set; } = string.Empty;    // Admin who reviewed
    public DateTime? ReviewedAt { get; set; }
    public string ReviewNote { get; set; } = string.Empty;

    // Computed properties for UI
    public string RequestTypeDisplay => RequestType == AdminRequestType.AdminPromotion ? "طلب ترقية" : "طلب حظر";
    public bool IsBanRequest => RequestType == AdminRequestType.BanRequest;
    public bool IsPromotion => RequestType == AdminRequestType.AdminPromotion;
}

// 🛡️ Admin Permission Flags
[Flags]
public enum AdminPermission
{
    BanFromChat = 1,
    BanFromProfileChanges = 2,
    BanFromNews = 4,
    BanFromPrivateMessages = 8,
    SendSystemAnnouncement = 16,
    ReviewAdminRequests = 32,
    ManageOtherAdmins = 64
}

// 📊 Admin Action Log
public class AdminAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AdminId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty; // "Ban", "Unban", "Announcement", etc.
    public string TargetPlayerId { get; set; } = string.Empty;
    public string TargetPlayerName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// 🎛️ Admin Panel Menu Item
public class AdminMenuItem
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Navigation target or action key
    public AdminPermission RequiredPermission { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
}

// 🧑‍💼 New: Player role info for UI
public class PlayerRoleInfo
{
    public string PlayerId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Admin", "Manager", "TemporaryAdmin", "Regular"
    public PlayerAccount Player { get; set; } = null!; // Full player object for further actions
}