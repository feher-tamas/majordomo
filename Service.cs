using Majordomo.Common;
using NetMQ;

namespace Majordomo
{
    internal class Service
    {
        private readonly List<Worker> _workers;                // list of known and active worker for this service 
        private readonly List<NetMQMessage> _pendingRequests;  // list of client requests for that service
        private readonly List<Worker> _waitingWorkers;         // queue of workers waiting for requests FIFO!

        /// <summary>
        /// The service name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     returns a readonly sequence of waiting workers - some of which may have expired
        /// </summary>
        /// <remarks>
        ///     we need to copy the array in order to allow the changing of it in an iteration
        ///     since we will change the list while iterating we must operate on a copy
        /// </remarks>
        public IEnumerable<Worker> WaitingWorkers => _waitingWorkers.ToArray();

        /// <summary>
        /// Returns a list of requests pending for being send to workers
        /// </summary>
        public List<NetMQMessage> PendingRequests => _pendingRequests;

        /// <summary>
        /// Ctor for a service
        /// </summary>
        /// <param name="name">the service name</param>
        public Service(string name)
        {
            Name = name;
            _workers = new List<Worker>();
            _pendingRequests = new List<NetMQMessage>();
            _waitingWorkers = new List<Worker>();
        }

        /// <summary>
        /// Returns true if workers are waiting and requests are pending and false otherwise
        /// </summary>
        public bool CanDispatchRequests()
        {
            return _waitingWorkers.Count > 0 && _pendingRequests.Count > 0;
        }

        /// <summary>
        /// Returns true if workers exist and false otherwise.
        /// </summary>
        public bool DoWorkersExist()
        {
            return _workers.Count > 0;
        }

        /// <summary>
        /// Get the longest waiting worker for this service and remove it from the waiting list
        /// </summary>
        /// <returns>the worker or if none is available <c>null</c></returns>
        public Worker GetNextWorker()
        {
            var worker = _waitingWorkers.Count == 0 ? null : _waitingWorkers[0];

            if (worker != null)
                _waitingWorkers.Remove(worker);

            return worker;
        }

        /// <summary>
        /// Adds a worker to the waiting worker list and if it is not known it adds it to the known workers as well
        /// </summary>
        /// <param name="worker">the worker to add</param>
        public void AddWaitingWorker(Worker worker)
        {
            if (!IsKnown(worker))
                _workers.Add(worker);

            if (!IsWaiting(worker))
            {
                // add to the end of the list
                // oldest is at the beginning of the list
                _waitingWorkers.Add(worker);
            }
        }

        /// <summary>
        /// Deletes worker from the list of known workers and
        /// if the worker is registered for waiting removes it 
        /// from that list as well
        /// in order to synchronize the deleting access to the 
        /// local lists <c>m_syncRoot</c> is used to lock
        /// </summary>
        /// <param name="worker">the worker to delete</param>
        public void DeleteWorker(Worker worker)
        {
            if (IsKnown(worker.Id))
                _workers.Remove(worker);

            if (IsWaiting(worker))
                _waitingWorkers.Remove(worker);
        }

        /// <summary>
        /// Add the request to the pending requests.
        /// </summary>
        /// <param name="message">the message to send</param>
        public void AddRequest(NetMQMessage message)
        {
            // add to the end, thus the oldest is the first element
            _pendingRequests.Add(message);
        }

        /// <summary>
        /// Return the oldest pending request or null if non exists.
        /// </summary>
        /// <remarks>
        ///     no synchronization necessary since no concurrent access
        /// </remarks>
        public NetMQMessage GetNextRequest()
        {
            // get one or null
            var request = _pendingRequests.Count > 0 ? _pendingRequests[0] : null;
            // remove from pending requests if it exists
            if (request is not null)
                _pendingRequests.Remove(request);

            return request;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"Name = {Name} / Worker {_workers.Count} - Waiting {_waitingWorkers.Count} - Pending REQ {_pendingRequests.Count}";
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            var other = obj as Service;

            return other is not null && Name == other.Name;
        }

        private bool IsKnown(string workerName) { return _workers.Exists(w => w.Id == workerName); }

        private bool IsKnown(Worker worker) { return _workers.Contains(worker); }

        private bool IsWaiting(Worker worker) { return _waitingWorkers.Contains(worker); }
    }
}