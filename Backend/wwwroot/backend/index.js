const { ajax, defer, fromEvent, interval, filter, map, catchError, of } = rxjs;


const getConnections = () => {
    const url = '/api/Backend'
    return ajax.ajax.getJSON(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    );
};


function closeSocket(id) {
    const url = `/api/Frontend/CloseSocket/${id()}`
    ajax.ajax.put(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    ).subscribe({
        next: data => {
            console.log(data);
            
        },
        error: err => console.error('Error: ', err),
        complete: () => console.log('Request complete')
    });

}


var viewModel = {};

getConnections().subscribe({
    next: data => {
        viewModel.connections = ko.mapping.fromJS(data);
        ko.applyBindings(viewModel);
    },
    error: err => console.error('Error: ', err),
    complete: () => console.log('Request complete')
});

