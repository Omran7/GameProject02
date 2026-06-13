using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameProject02.Models;

namespace GameProject02.Services
{
    public class RealtimeJoinRequestListener : IDisposable
    {
        private readonly string _gangId;
        private Timer _timer;
        private List<GangJoinRequest> _lastRequests = new List<GangJoinRequest>();
        private bool _isPolling = false;
        private readonly object _lock = new object();

        // Event fired when new requests are found
        public event EventHandler<List<GangJoinRequest>> RequestsChanged;

        public RealtimeJoinRequestListener(string gangId)
        {
            _gangId = gangId;
        }

        public void StartListening(int intervalSeconds = 5)
        {
            if (_timer == null)
            {
                _timer = new Timer(async _ => await PollAsync(), null, 0, intervalSeconds * 1000);
            }
        }

        public void StopListening()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private async Task PollAsync()
        {
            // Prevent overlapping polls
            if (_isPolling) return;
            _isPolling = true;

            try
            {
                var currentRequests = await GangDatabaseService.GetJoinRequestsAsync(_gangId);
                lock (_lock)
                {
                    // Compare with last snapshot
                    bool changed = !AreEqual(_lastRequests, currentRequests);
                    if (changed)
                    {
                        _lastRequests = currentRequests;
                        // Raise event on UI thread
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            RequestsChanged?.Invoke(this, currentRequests);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Listener] Poll error: {ex.Message}");
            }
            finally
            {
                _isPolling = false;
            }
        }

        private bool AreEqual(List<GangJoinRequest> a, List<GangJoinRequest> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].PlayerId != b[i].PlayerId ||
                    a[i].GangId != b[i].GangId ||
                    a[i].PlayerName != b[i].PlayerName ||
                    a[i].Timestamp != b[i].Timestamp)
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}