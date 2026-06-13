using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class PrivateChatService
    {
        private const string ProjectId = "gameproject02-4207f";
        private const string ApiKey = "AIzaSyCM61YoJzqt9x7lOndV2oBJGeoBtU9U_Uo";
        private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
        private static readonly HttpClient _client = new();

        public static string GetConversationId(string userId1, string userId2)
        {
            var ids = new List<string> { userId1, userId2 };
            ids.Sort(StringComparer.Ordinal);
            return string.Join("_", ids);
        }

        private static object StringValue(string s) => new { stringValue = s ?? "" };
        private static object IntegerValue(long n) => new { integerValue = n };
        private static object ArrayValue(object[] values) => new { arrayValue = new { values } };

        // Ensure conversation document exists
        private static async Task<bool> EnsureConversationExistsAsync(string conversationId, string userId1, string userId2)
        {
            string convUrl = $"{BaseUrl}/private_chats/{conversationId}?key={ApiKey}";
            var getResponse = await _client.GetAsync(convUrl);
            if (getResponse.IsSuccessStatusCode) return true;

            // Create new conversation document with participants and empty unreadCounts
            var participantsArray = new[] { StringValue(userId1), StringValue(userId2) };
            var fields = new Dictionary<string, object>
    {
        { "participants", new { arrayValue = new { values = participantsArray } } },
        { "lastMessage", StringValue("") },
        { "lastTimestamp", IntegerValue(0) },
        { "createdAt", IntegerValue(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) },
        { "unreadCounts", new { mapValue = new { fields = new Dictionary<string, object>() } } }
    };
            var doc = new { fields };
            var json = JsonSerializer.Serialize(doc);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(convUrl, content);
            if (response.IsSuccessStatusCode) return true;
            return false;
        }
        // Send a message
        public static async Task<bool> SendPrivateMessageAsync(string conversationId, string senderId, string content)
        {
            try
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var participants = conversationId.Split('_');
                if (participants.Length != 2) return false;

                string receiverId = participants[0] == senderId ? participants[1] : participants[0];

                // Ensure conversation exists (creates with empty participants and unreadCounts if new)
                if (!await EnsureConversationExistsAsync(conversationId, participants[0], participants[1]))
                    return false;

                // 1. Add message to subcollection
                string msgCollectionUrl = $"{BaseUrl}/private_chats/{conversationId}/messages?key={ApiKey}";
                var msgFields = new Dictionary<string, object>
        {
            { "senderId", StringValue(senderId) },
            { "content", StringValue(content) },
            { "timestamp", IntegerValue(timestamp) }
        };
                var msgDoc = new { fields = msgFields };
                var msgJson = JsonSerializer.Serialize(msgDoc);
                var msgContent = new StringContent(msgJson, Encoding.UTF8, "application/json");
                var msgResponse = await _client.PostAsync(msgCollectionUrl, msgContent);
                if (!msgResponse.IsSuccessStatusCode) return false;

                // 2. Update conversation metadata (last message, timestamp, and unread count for receiver)
                string convUrl = $"{BaseUrl}/private_chats/{conversationId}?key={ApiKey}";

                // First, get current unreadCounts
                int currentReceiverUnread = 0;
                var getResponse = await _client.GetAsync(convUrl);
                if (getResponse.IsSuccessStatusCode)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("fields", out var fields) &&
                        fields.TryGetProperty("unreadCounts", out var unreadField) &&
                        unreadField.TryGetProperty("mapValue", out var mapVal) &&
                        mapVal.TryGetProperty("fields", out var mapFields) &&
                        mapFields.TryGetProperty(receiverId, out var receiverUnread) &&
                        receiverUnread.TryGetProperty("integerValue", out var v))
                    {
                        currentReceiverUnread = v.ValueKind == JsonValueKind.String ? int.Parse(v.GetString()!) : v.GetInt32();
                    }
                }

                // Build update fields
                var updateFields = new Dictionary<string, object>
        {
            { "lastMessage", StringValue(content) },
            { "lastTimestamp", IntegerValue(timestamp) }
        };
                // Update unreadCounts for receiver
                var unreadMap = new Dictionary<string, object>
        {
            { receiverId, IntegerValue(currentReceiverUnread + 1) }
        };
                updateFields["unreadCounts"] = new { mapValue = new { fields = unreadMap } };

                var updateDoc = new { fields = updateFields };
                var updateJson = JsonSerializer.Serialize(updateDoc);
                var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
                var convResponse = await _client.PatchAsync(convUrl, updateContent);
                if (!convResponse.IsSuccessStatusCode)
                {
                    var error = await convResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[PrivateChat] Update conversation failed: {convResponse.StatusCode} - {error}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] Send exception: {ex.Message}");
                return false;
            }
        }
        // Load messages from subcollection
        public static async Task<List<PrivateChatMessage>> GetMessagesAsync(string conversationId)
        {
            var messages = new List<PrivateChatMessage>();
            try
            {
                string url = $"{BaseUrl}/private_chats/{conversationId}/messages?key={ApiKey}&orderBy=timestamp";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Debug.WriteLine($"[PrivateChat] No messages yet for {conversationId}");
                        return messages;
                    }
                    Debug.WriteLine($"[PrivateChat] GetMessages failed: {response.StatusCode}");
                    return messages;
                }
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs)) return messages;
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("fields", out var f)) continue;
                    messages.Add(new PrivateChatMessage
                    {
                        SenderId = GetStr(f, "senderId"),
                        Content = GetStr(f, "content"),
                        Timestamp = GetLong(f, "timestamp")
                    });
                }
                // Messages are in ascending order by timestamp (oldest first)
                Debug.WriteLine($"[PrivateChat] Loaded {messages.Count} messages for {conversationId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] GetMessages error: {ex.Message}");
            }
            return messages;
        }

        // Get all conversations for a user (client-side filter)
        public static async Task<List<ConversationModel>> GetMyConversationsAsync(string userId)
        {
            var list = new List<ConversationModel>();
            try
            {
                string url = $"{BaseUrl}/private_chats?key={ApiKey}&pageSize=100";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return list;
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs)) return list;
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    string convId = nameProp.GetString().Split('/').Last();
                    var parts = convId.Split('_');
                    if (parts.Length != 2) continue;
                    string user1 = parts[0], user2 = parts[1];
                    if (user1 != userId && user2 != userId) continue;
                    string otherId = user1 == userId ? user2 : user1;
                    if (!d.TryGetProperty("fields", out var fields)) continue;
                    int unreadCount = 0;
                    if (fields.TryGetProperty("unreadCounts", out var unreadField) &&
                        unreadField.TryGetProperty("mapValue", out var mapVal) &&
                        mapVal.TryGetProperty("fields", out var mapFields) &&
                        mapFields.TryGetProperty(userId, out var userUnread) &&
                        userUnread.TryGetProperty("integerValue", out var v))
                    {
                        unreadCount = v.ValueKind == JsonValueKind.String ? int.Parse(v.GetString()!) : v.GetInt32();
                    }
                    list.Add(new ConversationModel
                    {
                        ConversationId = convId,
                        OtherUserId = otherId,
                        LastMessage = GetStr(fields, "lastMessage"),
                        Timestamp = GetLong(fields, "lastTimestamp"),
                        UnreadCount = unreadCount
                    });
                }
                list = list.OrderByDescending(c => c.Timestamp).ToList();
                Debug.WriteLine($"[PrivateChat] Found {list.Count} conversations for {userId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] GetConversations error: {ex.Message}");
            }
            return list;
        }

        public static async Task<bool> MarkConversationAsReadAsync(string conversationId, string userId)
        {
            try
            {
                string convUrl = $"{BaseUrl}/private_chats/{conversationId}?key={ApiKey}";
                var updateFields = new Dictionary<string, object>
        {
            { "unreadCounts", new { mapValue = new { fields = new Dictionary<string, object> { { userId, IntegerValue(0) } } } } }
        };
                var updateDoc = new { fields = updateFields };
                var updateJson = JsonSerializer.Serialize(updateDoc);
                var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync(convUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] Mark read error: {ex.Message}");
                return false;
            }
        }
        private static string GetStr(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v) ? v.GetString() ?? "" : "";

        private static long GetLong(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v)
                ? (v.ValueKind == JsonValueKind.String ? long.Parse(v.GetString()!) : v.GetInt64())
                : 0;
    }
}