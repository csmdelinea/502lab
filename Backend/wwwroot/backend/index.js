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
    const url = `/api/Backend/CloseSocket/${id()}`
    ajax.ajax.put(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    ).subscribe({
        next: data => {
            console.log(data);
            var item = viewModel.connections().filter((n) => n.id() === data.response.id);
            if (item) {
                item[0].socketState(data.response.socketState);

            }
        },
        error: err => console.error('Error: ', err),
        complete: () => console.log('Request complete')
    });

}

function abortSocket(id) {
    const url = `/api/Backend/AbortSocket/${id()}`
    ajax.ajax.put(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    ).subscribe({
        next: data => {
            console.log(data);
            var item = viewModel.connections().filter((n) => n.id() === data.response.id);
            if (item) {
                item[0].socketState(data.response.socketState);

            }
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

                s