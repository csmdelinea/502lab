using Yarp.ReverseProxy.Model;

namespace Frontend.Monitor
{
    public class ProbingModel
    {
        private readonly ClusterModel _clusterModel;
        private readonly DestinationModel _destinationModel;

        public ProbingModel(ClusterModel clusterModel, DestinationModel destinationModel)
        {
            _clusterModel = clusterModel;
            _destinationModel = destinationModel;
        }

        public ClusterModel ClusterModel
        {
            get { return _clusterModel; }
        }

        public DestinationModel DestinationModel
        {
            get { return _destinationModel; }
        }
    }
    public class DiagnosticsService
    {
        private Dictionary<string, ProbingModel> _clusterProbingDictionary = new();

        public Dictionary<string, ProbingModel> ClusterProbingDictionary
        {
            get { return _clusterProbingDictionary; }
        }

    }
}
