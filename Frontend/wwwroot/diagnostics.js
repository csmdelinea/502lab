const { ajax, defer, fromEvent, interval, filter, map, catchError, of } = rxjs;
var viewModel = {}

function getApiData(url) {
    return ajax.ajax.getJSON(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    );
}
const getViewModel = getApiData('/api/diagnostics');
const checkHealth = (clusterId) => getApiData(`/api/diagnostics/${clusterId}`); 

getViewModel.subscribe({
    next: data => {
        viewModel = ko.mapping.fromJS(data);        
        viewModel.queryHealth = function (options, event) {

            checkHealth(options.model.clusterId()).subscribe({
                next: health => {
                    if(health.isHealthy)
                        options.lastHealthyProbeUtc(health.lastHealthyProbeUtc);
                    else
                        options.lastHealthyProbeUtc('Failed');
                }
            })
        };
        ko.applyBindings(viewModel);
    },
    error: err => console.error('Error: ', err),
    complete: () => console.log('Request complete')
  });


