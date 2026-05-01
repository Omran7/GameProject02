using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameProject02.Models;

public class GangObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string _gangId = Guid.NewGuid().ToString();
    public string GangId
    {
        get => _gangId;
        set { _gangId = value; OnPropertyChanged(); }
    }

    private string _name = "عصابتك";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _tag = "XXX";
    public string Tag
    {
        get => _tag;
        set { _tag = value; OnPropertyChanged(); }
    }

    private string _imageUrl = string.Empty;
    public string ImageUrl
    {
        get => _imageUrl;
        set { _imageUrl = value; OnPropertyChanged(); }
    }

    private string _leaderId = string.Empty;
    public string LeaderId
    {
        get => _leaderId;
        set { _leaderId = value; OnPropertyChanged(); }
    }

    private long _gangCash = 0;
    public long GangCash
    {
        get => _gangCash;
        set { _gangCash = value; OnPropertyChanged(); }
    }

    private long _respect = 0;
    public long Respect
    {
        get => _respect;
        set { _respect = value; OnPropertyChanged(); }
    }

    private long _availableRespect = 0;
    public long AvailableRespect
    {
        get => _availableRespect;
        set { _availableRespect = value; OnPropertyChanged(); }
    }

    private long _loyalty = 0;
    public long Loyalty
    {
        get => _loyalty;
        set { _loyalty = value; OnPropertyChanged(); }
    }

    private long _contribution = 0;
    public long Contribution
    {
        get => _contribution;
        set { _contribution = value; OnPropertyChanged(); }
    }

    private int _level = 1;
    public int Level
    {
        get => _level;
        set { _level = value; OnPropertyChanged(); }
    }

    public Dictionary<string, int> SkillsLevel { get; set; } = new();
    public Dictionary<string, GangPosition> MembersWithPositions { get; set; } = new();
    public List<string> MilitiaMemberIds { get; set; } = new();
    public string MembersIdsJoinedMilitia { get; set; } = string.Empty;
    public Dictionary<int, List<string>> MilitiaMembersByUnit { get; set; } = new();
    public int MembersCount => MembersWithPositions.Count;

    public bool IsPlayerMainStatesChanged { get; set; } = true;
    public bool IsAllProcessSuccess { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public long CreatedTimeInMilli { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public bool IsMember(string playerId) => MembersWithPositions.ContainsKey(playerId);
    public GangPosition GetPosition(string playerId) =>
        MembersWithPositions.TryGetValue(playerId, out var pos) ? pos : GangPosition.None;
}